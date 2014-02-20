//===========================================================================
// MODULE:  ProjectFactory.cs
// PURPOSE: Collects dependency information
// 
// Copyright © 2012
// Brent M. Spell. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 3 of the License, or 
// (at your option) any later version. This library is distributed in the 
// hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU Lesser General Public License for more details. You should 
// have received a copy of the GNU Lesser General Public License along with 
// this library; if not, write to 
//    Free Software Foundation, Inc. 
//    51 Franklin Street, Fifth Floor 
//    Boston, MA 02110-1301 USA
//===========================================================================
// this class is based on http://nuget.codeplex.com/
//===========================================================================
// System References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Build.Evaluation;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public class ProjectFactoryEqualityComparer : IEqualityComparer<ProjectFactory>
   {
      public bool Equals(ProjectFactory x, ProjectFactory y)
      {
         return x.FullPath == y.FullPath;
      }

      public int GetHashCode(ProjectFactory obj)
      {
         return obj.FullPath.GetHashCode();
      }
   }

   public class ProjectFactory
   {
      private readonly Project project;
      private ISettings settings;

      private const string ContentItemType = "Content";
      private const string NoneItemType = "None";
      private const string ProjectReferenceItemType = "ProjectReference";
      private const string ReferenceItemType = "Reference";
      private const string PackagesFolder = "packages";

      public ProjectFactory(string path)
         : this(ProjectCollection
            .GlobalProjectCollection
            .LoadedProjects
            .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.FullPath, path) == 0)
            .FirstOrDefault()
            ?? new Project(path)) 
      { }

      private ProjectFactory(Project project)
      {
         this.project = project;
         this.settings = null;

         var targetFrameworkMoniker = project.GetPropertyValue("TargetFrameworkMoniker");
         if (!String.IsNullOrEmpty(targetFrameworkMoniker))
            TargetFramework = new FrameworkName(targetFrameworkMoniker);
      }

      public string FullPath
      {
         get
         {
            return project.FullPath;
         }
      }

      private string targetPath;
      public string TargetPath
      {
         get
         {
            if (string.IsNullOrEmpty(targetPath))
            {
               var resolvedTargetPath = ResolveTargetPath();
               if (!File.Exists(resolvedTargetPath))
                  throw new InvalidOperationException(
                     string.Format("Unable to find build output '{0}', make sure the project is referred.", resolvedTargetPath));
               targetPath = resolvedTargetPath;
            }
            return targetPath;
         }
      }

      private string ResolveTargetPath()
      {
         // Re-evaluate the project so that the new property values are applied
         project.ReevaluateIfNecessary();

         // Return the new target path
         return project.GetPropertyValue("TargetPath");
      }

      private IEnumerable<string> nuTargets;
      public IEnumerable<string> NuTargets
      {
         get
         {
            if (nuTargets == null)
            {
               var resolvedNuTargets = ResolveNuTargets();
               foreach (var resolvedNuTarget in resolvedNuTargets)
                  if (!File.Exists(resolvedNuTarget))
                     throw new InvalidOperationException(
                        string.Format("Unable to find build output '{0}', make sure the project is referred.", resolvedNuTarget));
               nuTargets = resolvedNuTargets;
            }
            return nuTargets;
         }
      }

      private IEnumerable<string> ResolveNuTargets()
      {
         // MsBuild can't cache these projects (no binary output), these reference information are stored in intermediate files
         return System.IO.File.ReadAllLines(ProjectHelper.GetNupkgsFullPath(project))
            .AsEnumerable();
      }

      public FrameworkName TargetFramework
      {
         get;
         private set;
      }

      private ISettings DefaultSettings
      {
         get
         {
            if (null == settings)
               settings = Settings.LoadDefaultSettings(new PhysicalFileSystem(project.DirectoryPath), null, null);
            return settings;
         }
      }

      private string SolutionDir
      {
         get
         {
            return ProjectHelper.GetSolutionDir(project.DirectoryPath);
         }
      }

      private string RepositoryPath
      {
         get
         {
            return DefaultSettings.GetRepositoryPath();
         }
      }
      
      private IEnumerable<string> GetReferencesIdentity()
      {
         var defaultReferences = ProjectHelper.GetDefaultReferences(project.FullPath);
         return project
            .GetItems(ReferenceItemType)
            .Where(item =>
            {
               var identity = item.GetMetadataValue("Identity");
               if (defaultReferences.Contains(identity))
                  return false;
               var copyLocal = item.GetMetadataValue("Private");
               if (string.IsNullOrEmpty(copyLocal))
                  return GacApi.AssemblyExist(identity);
               else
                  return string.Compare(copyLocal, "false", true) == 0;
            })
            .Select(item => item.GetMetadataValue("Identity"));
      }

      private string GetPackageReferenceFilePath()
      {
         return project
            .GetItems(ContentItemType, NoneItemType)
            .Select(item => item.GetMetadataValue("FullPath"))
            .FirstOrDefault(file => Path.GetFileName(file).Equals(Constants.PackageReferenceFile, StringComparison.OrdinalIgnoreCase));
      }

      /// <summary>
      /// Collects all targetFramework, project, frameworkAssembly and package references by target framework
      /// accessible through this project.
      /// </summary>
      /// <param name="targetFrameworks">Collects all referenced target frameworks here.</param>
      /// <param name="projectReferencesByFramework">Collects all project references by framework here.</param>
      /// <param name="frameworkAssembliesByFramework">Collects all framework assembly references by framework here.</param>
      /// <param name="packagesAndDependenciesByFramework">Collects all package references by framework here.</param>
      public void CollectDependencies(
         HashSet<ProjectFactory> referenceProjectFactories,
         HashSet<FrameworkName> targetFrameworks,
         Dictionary<FrameworkName, HashSet<string>> frameworkAssembliesByFramework,
         Dictionary<FrameworkName, Dictionary<string, Tuple<IPackage, PackageDependency>>> packagesAndDependenciesByFramework,
         bool recursively)
      {
         // Create/get target framework specific collections
         HashSet<string> frameworkAssemblies;
         Dictionary<string, Tuple<IPackage, PackageDependency>> packagesAndDependencies;
         if (targetFrameworks.Add(TargetFramework))
         {
            frameworkAssembliesByFramework.Add(TargetFramework,
               frameworkAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            packagesAndDependenciesByFramework.Add(TargetFramework,
               packagesAndDependencies = new Dictionary<string, Tuple<IPackage, PackageDependency>>());
         }
         else
         {
            frameworkAssemblies = frameworkAssembliesByFramework[TargetFramework];
            packagesAndDependencies = packagesAndDependenciesByFramework[TargetFramework];
         }

         if (recursively)
            // get all packages and dependencies, including the ones in project references
            RecursivelyApplyOnProjectReferences(
               ProjectHelper.ValidItem,
               p => p.AddDependencies(referenceProjectFactories, frameworkAssemblies, packagesAndDependencies));
         else
            AddDependencies(referenceProjectFactories, frameworkAssemblies, packagesAndDependencies);
      }

      public void CollectNuBuildDependencies(
         HashSet<ProjectFactory> nuBuildReferenceProjectFactories,
         HashSet<FrameworkName> targetFrameworks,
         Dictionary<FrameworkName, HashSet<string>> frameworkAssembliesByFramework,
         Dictionary<FrameworkName, Dictionary<string, Tuple<IPackage, PackageDependency>>> packagesAndDependenciesByFramework,
         bool recursively)
      {
         // Create/get target framework specific collections
         if (targetFrameworks.Add(TargetFramework))
         {
            frameworkAssembliesByFramework.Add(TargetFramework,
               new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            packagesAndDependenciesByFramework.Add(TargetFramework,
               new Dictionary<string, Tuple<IPackage, PackageDependency>>());
         }

         if (recursively)
            // get all packages and dependencies, including the ones in project references
            RecursivelyApplyOnProjectReferences(
               ProjectHelper.NuspecItem,
               p => p.AddNuBuildDependencies(nuBuildReferenceProjectFactories));
         else
            AddNuBuildDependencies(nuBuildReferenceProjectFactories);
      }

      private void RecursivelyApplyOnProjectReferences(Predicate<string> predicate, Action<ProjectFactory> action)
      {
         var alreadyAppliedProjects = new HashSet<string>();
         RecursivelyApplyOnProjectReferences(predicate, action, alreadyAppliedProjects);
      }

      private void RecursivelyApplyOnProjectReferences(Predicate<string> predicate, Action<ProjectFactory> action,
         HashSet<string> alreadyAppliedProjects)
      {
         action(this);
         alreadyAppliedProjects.Add(FullPath);
         foreach (var fullPath in project
            .GetItems(ProjectReferenceItemType)
            .FullPath()
            .Where(item => predicate(item))
            .UnappliedItem(alreadyAppliedProjects))
         {
            var referencedProject = new ProjectFactory(fullPath);
            referencedProject.TargetFramework = TargetFramework;
            referencedProject.RecursivelyApplyOnProjectReferences(predicate, action, alreadyAppliedProjects);
         }
      }

      private void AddDependencies(
         HashSet<ProjectFactory> referenceProjectFactories,
         HashSet<string> frameworkAssemblies,
         Dictionary<string, Tuple<IPackage, PackageDependency>> packagesAndDependencies)
      {
         referenceProjectFactories.Add(this);

         foreach (var reference in GetReferencesIdentity())
            frameworkAssemblies.Add(reference);

         var packageReferenceFile = GetPackageReferenceFile();
         if (packageReferenceFile == null)
            return;
         var packagesRepository = GetPackagesRepository();
         if (packagesRepository == null)
            return;

         // Collect all packages
         var packageReferences =
             packageReferenceFile.GetPackageReferences()
             .Where(r => !r.IsDevelopmentDependency)
             .ToDictionary(r => new PackageName(r.Id, r.Version));
         // add all packages and create an associated dependency to the dictionary
         foreach (var reference in packageReferences.Values)
         {
            var package = packagesRepository.FindPackage(reference.Id, reference.Version);
            if (package != null && !packagesAndDependencies.ContainsKey(package.Id))
            {
               var spec = GetVersionConstraint(packageReferences, package);
               var dependency = new PackageDependency(package.Id, spec);
               packagesAndDependencies.Add(package.Id, new Tuple<IPackage, PackageDependency>(package, dependency));
            }
         }
      }

      private void AddNuBuildDependencies(HashSet<ProjectFactory> nuBuildReferenceProjectFactories)
      {
         nuBuildReferenceProjectFactories.Add(this);
      }

      private static IVersionSpec GetVersionConstraint(IDictionary<PackageName, PackageReference> packageReferences, IPackage package)
      {
         var defaultVersionConstraint = VersionUtility.ParseVersionSpec(package.Version.ToString());

         PackageReference packageReference;
         var key = new PackageName(package.Id, package.Version);
         if (!packageReferences.TryGetValue(key, out packageReference))
            return defaultVersionConstraint;

         return packageReference.VersionConstraint ?? defaultVersionConstraint;
      }

      private PackageReferenceFile GetPackageReferenceFile()
      {
         var packagesReferenceFilePath = GetPackageReferenceFilePath();
         if (!string.IsNullOrEmpty(packagesReferenceFilePath))
            return new PackageReferenceFile(packagesReferenceFilePath);
         return null;
      }

      private IPackageRepository GetPackagesRepository()
      {
         var solutionDir = SolutionDir;
         var defaultValue = RepositoryPath;

         string target = null;
         if (!string.IsNullOrEmpty(solutionDir))
         {
            var configValue = GetConfiguredPackagesRepositoryPath(solutionDir);
            // solution dir exists, no default packages folder specified anywhere,
            // default to hardcoded "packages" folder under solution
            if (string.IsNullOrEmpty(configValue) && string.IsNullOrEmpty(defaultValue))
               configValue = PackagesFolder;

            if (!string.IsNullOrEmpty(configValue))
               target = Path.Combine(solutionDir, configValue);
         }

         if (string.IsNullOrEmpty(target))
            target = defaultValue;

         if (!string.IsNullOrEmpty(target) && Directory.Exists(target))
            return new SharedPackageRepository(target);

         return null;
      }

      private static string GetConfiguredPackagesRepositoryPath(string solutionDir)
      {
         var configPath = Path.Combine(solutionDir, Constants.SettingsFileName);

         try
         {
            // Support the hidden feature
            if (File.Exists(configPath))
               using (var stream = File.OpenRead(configPath))
               {
                  // It's possible for the repositoryPath element to be missing in older versions of 
                  // a NuGet.config file.
                  var repositoryPathElement = XmlUtility.LoadSafe(stream).Root.Element("repositoryPath");
                  if (repositoryPathElement != null)
                     return repositoryPathElement.Value;
               }
         }
         catch (FileNotFoundException)
         { }

         return null;
      }
   }
}
