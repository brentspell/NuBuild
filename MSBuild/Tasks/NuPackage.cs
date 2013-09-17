//===========================================================================
// MODULE:  NuPackage.cs
// PURPOSE: NuBuild package MSBuild task
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
// System References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
// Project References
using NuGet;

namespace NuBuild.MSBuild
{
   /// <summary>
   /// Package task
   /// </summary>
   /// <remarks>
   /// This task compiles a set of .nuspec items into a corresponding set
   /// of NuGet packages (.nupkg) files.
   /// </remarks>
   public sealed class NuPackage : Task, NuGet.IPropertyProvider
   {
      private Project propertyProject = null;

      #region Task Parameters
      /// <summary>
      /// The full project path
      /// </summary>
      [Required]
      public String ProjectPath { get; set; }
      /// <summary>
      /// The source .nuspec file items
      /// </summary>
      [Required]
      public ITaskItem[] NuSpec { get; set; }
      /// <summary>
      /// The project output directory path
      /// </summary>
      [Required]
      public String OutputPath { get; set; }
      /// <summary>
      /// The list of DLLs referenced by the current
      /// NuGet project
      /// </summary>
      public ITaskItem[] ReferenceLibraries { get; set; }
      #endregion

      /// <summary>
      /// Task execution override
      /// </summary>
      /// <returns>
      /// True if successful
      /// False otherwise
      /// </returns>
      public override Boolean Execute ()
      {
         try
         {
            // parepare the task for execution
            if (this.ReferenceLibraries == null)
               this.ReferenceLibraries = new ITaskItem[0];
            this.OutputPath = Path.GetFullPath(this.OutputPath);
            // compile the nuget package
            foreach (var specItem in this.NuSpec)
               BuildPackage(specItem);
         }
         catch (Exception e)
         {
            Log.LogError("{0} ({1})", e.Message, e.GetType().Name);
            return false;
         }
         return true;
      }
      /// <summary>
      /// Compiles a single .nuspec file
      /// </summary>
      /// <param name="specItem">
      /// The .nuspec file to compile
      /// </param>
      private void BuildPackage (ITaskItem specItem)
      {
         // load the package manifest (nuspec) from the task item
         // using the nuget package builder
         var specPath = specItem.GetMetadata("FullPath");
         var builder = (NuGet.PackageBuilder)null;
         using (var specFile = File.OpenRead(specPath))
            builder = new NuGet.PackageBuilder(
               specFile,
               Path.GetDirectoryName(specPath),
               this as NuGet.IPropertyProvider
            );
         // initialize dynamic manifest properties
         var version = specItem.GetMetadata("NuPackageVersion");
         if (!String.IsNullOrEmpty(version))
            builder.Version = new NuGet.SemanticVersion(version);
         // add a new file to the folder for each project
         // referenced by the current project
         AddLibraries(builder);
         // write the configured package out to disk
         var pkgPath = specItem.GetMetadata("NuPackagePath");
         using (var pkgFile = File.Create(pkgPath))
            builder.Save(pkgFile);
      }
      /// <summary>
      /// Adds project references to the package lib section
      /// </summary>
      /// <param name="builder">
      /// The current package builder
      /// </param>
      private void AddLibraries (NuGet.PackageBuilder builder)
      {
         // add package files from project references
         // . DLL references go in the lib package folder
         // . EXE references go in the tools package folder
         // . everything else goes in the content package folder
         foreach (var libItem in this.ReferenceLibraries)
         {
            var ext = libItem.GetMetadata("Extension").ToLower();
            var folder = (String)null;
            if (ext == ".dll")
               folder = "lib";
            else if (ext == ".exe")
               folder = "tools";
            if (folder == null)
               folder = "content";
            else
            {
               var frameworkFolder = GetShortTargetFrameworkName(libItem.GetMetadata("FullPath"));
               if (frameworkFolder != null)
                  folder = Path.Combine(folder, frameworkFolder);
            }
            builder.Files.Add(
               new NuGet.PhysicalPackageFile()
               {
                  SourcePath = libItem.GetMetadata("FullPath"),
                  TargetPath = String.Format(
                     @"{0}\{1}{2}",
                     folder,
                     libItem.GetMetadata("Filename"),
                     libItem.GetMetadata("Extension")
                  )
               }
            );
         }
      }
      /// <summary>
      /// Returns the NuGet like short version of the TargetFramework custom attribute.
      /// </summary>
      /// <param name="assemblyFile">
      /// The path of the file that contains the manifest of the assembly.
      /// </param>
      private String GetShortTargetFrameworkName(String assemblyFile)
      {
         var targetFrameworkName = RemoteAssemblyProxy.ExecuteGetter<String>("TempPackageDomain", assemblyFile,
            ra => ra.GetCustomAttribute<String>(
               ad => ad.Constructor.DeclaringType == typeof(TargetFrameworkAttribute),
               ad => (String)(ad.ConstructorArguments[0].Value)));
         // TODO
         // before .Net40, TargetFrameworkAttribute is not stored in the assembly, we should parse the .csproj file
         // from libItem.GetMetadata("MSBuildSourceProjectFile")
         //if (targetFrameworkName == null)
         //{ }
         return targetFrameworkName == null ? null : VersionUtility.GetShortFrameworkName(new FrameworkName(targetFrameworkName));
      }

      #region IPropertyProvider Implementation
      /// <summary>
      /// Retrieves nuget replacement values from a referenced
      /// assembly library or MSBuild property, as specified here:
      /// http://docs.nuget.org/docs/reference/nuspec-reference#Replacement_Tokens
      /// </summary>
      /// <param name="property">
      /// The replacement property to retrieve
      /// </param>
      /// <returns>
      /// The replacement property value
      /// </returns>
      public dynamic GetPropertyValue (String property)
      {
         // attempt to resolve the property from the referenced libraries
         foreach (var libItem in this.ReferenceLibraries)
         {
            var appDomain = (AppDomain)null;
            try
            {
               appDomain = RemoteAssemblyProxy.CreateDomain("TempPackageDomain");
               var asm = new RemoteAssemblyProxy(appDomain);
               asm.ReflectionOnlyLoadFrom(libItem.GetMetadata("FullPath"));
               if (property == "id")
                  return asm.GetPropertyValue<String>(ra => ra.GetName().Name);
               if (property == "version")
               {
                  var version = asm.GetPropertyValue<String>(ra => ra.GetName().Version.ToString());
                  if (version != new Version(0, 0, 0, 0).ToString())
                     return version;
               }
               if (property == "author")
               {
                  var attr = asm.GetCustomAttribute<String>(
                     ad => ad.Constructor.DeclaringType == typeof(AssemblyCompanyAttribute),
                     ad => (String)(ad.ConstructorArguments[0].Value));
                  if (attr != null)
                     return attr;
               }
               if (property == "description")
               {
                  var attr = asm.GetCustomAttribute<String>(
                     ad => ad.Constructor.DeclaringType == typeof(AssemblyDescriptionAttribute),
                     ad => (String)(ad.ConstructorArguments[0].Value)); 
                  if (attr != null)
                     return attr;
               }
            }
            catch { }
            finally
            {
               if (appDomain != null)
                  RemoteAssemblyProxy.UnloadDomain(appDomain);
            }            
         }
         // attempt to resolve the requested property
         // from the project properties
         if (this.propertyProject == null)
         {
            // attempt to retrieve the project from the global collection
            // this should always work in Visual Studio
            this.propertyProject = ProjectCollection
               .GlobalProjectCollection
               .LoadedProjects
               .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.FullPath, this.ProjectPath) == 0)
               .FirstOrDefault();
            //---------------------------------------------------------------
            // HACK: bspell - 6/25/2013
            // . unfortunately, MSBuild does not maintain the current project
            //   in the global project collection, nor does it expose the
            //   global properties collection for generic property retrieval
            // . retrieve the build parameters here using reflection to
            //   get the global MSBuild properties collection and load the
            //   project manually
            // . accessing private members using reflection is awful, but the
            //   alternative would be to pass in all possible MSBuild 
            //   properties to the custom task, which wouldn't even work for
            //   application-specific properties
            // . this method may be incompatible with future versions of 
            //   MSBuild
            //---------------------------------------------------------------
            if (this.propertyProject == null)
            {
               var prop = BuildManager.DefaultBuildManager.GetType().GetProperty(
                  "Microsoft.Build.BackEnd.IBuildComponentHost.BuildParameters", 
                  BindingFlags.Instance | BindingFlags.NonPublic
               );
               if (prop == null)
                  throw new NotSupportedException("Unable to retrieve MSBuild parameters using reflection");
               var param = (BuildParameters)prop
                  .GetValue(BuildManager.DefaultBuildManager, null);
               this.propertyProject = ProjectCollection.GlobalProjectCollection
                  .LoadProject(
                     this.ProjectPath, 
                     param.GlobalProperties, 
                     ProjectCollection.GlobalProjectCollection.DefaultToolsVersion
                  );
            }
         }
         if (this.propertyProject != null)
            return this.propertyProject.AllEvaluatedProperties
               .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.Name, property) == 0)
               .Select(p => p.EvaluatedValue)
               .FirstOrDefault();
         return null;
      }
      #endregion
   }
}
