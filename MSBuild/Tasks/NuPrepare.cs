//===========================================================================
// MODULE:  NuPrepare.cs
// PURPOSE: NuBuild prepare MSBuild task
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Xml.Linq;
using NuGet;

// Project References

namespace NuBuild.MSBuild
{
   /// <summary>
   /// Prepare task
   /// </summary>
   /// <remarks>
   /// This task configures the NuBuild Compile items for packaging,
   /// by attaching custom metadata and determining the build sources/
   /// targets for incremental builds. The task also establishes the
   /// build/version number for the current run.
   /// </remarks>
   public sealed class NuPrepare : Task
   {
      private List<ITaskItem> sourceList = new List<ITaskItem>();
      private List<ITaskItem> targetList = new List<ITaskItem>();
      private VersionSource versionSource;

      #region Task Parameters
      /// <summary>
      /// The source .nuspec file items
      /// </summary>
      [Required]
      [Output]
      public ITaskItem[] NuSpec { get; set; }
      /// <summary>
      /// The name of the project being build
      /// </summary>
      [Required]
      public String ProjectName { get; set; }
      /// <summary>
      /// The version source string (see the VersionSource enum above)
      /// </summary>
      [Required]
      public String VersionSource { get; set; }
      /// <summary>
      /// The project output directory path
      /// </summary>
      [Required]
      public String OutputPath { get; set; }
      /// <summary>
      /// The project build number, or 0 to generate for auto-versioning
      /// </summary>
      public Int32 BuildNumber { get; set; }
      /// <summary>
      /// Specifies whether to include the version number in the package 
      /// file name
      /// </summary>
      public Boolean VersionFileName { get; set; }
      /// <summary>
      /// The list of DLLs referenced by the current
      /// NuGet project
      /// </summary>
      public ITaskItem[] ReferenceLibraries { get; set; }
      /// <summary>
      /// Return the list of source files (dependencies) for the project 
      /// via here
      /// </summary>
      [Output]
      public ITaskItem[] Sources { get; set; }
      /// <summary>
      /// Return the list of build target files via here
      /// </summary>
      [Output]
      public ITaskItem[] Targets { get; set; }
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
            Directory.CreateDirectory(this.OutputPath);
            // add build dependencies from the nuspec file(s)
            // and the list of project references
            this.sourceList.AddRange(this.NuSpec);
            this.sourceList.AddRange(this.ReferenceLibraries);
            // parse the version source name
            this.versionSource = (VersionSource)Enum.Parse(
               typeof(VersionSource), 
               this.VersionSource, 
               true
            );
            // configure each nuspec item
            foreach (var specItem in this.NuSpec)
               PreparePackage(specItem);
            // return the list of build sources/targets
            this.Sources = this.sourceList.ToArray();
            this.Targets = this.targetList.ToArray();
         }
         catch (Exception e)
         {
            Log.LogError("{0} ({1})", e.Message, e.GetType().Name);
            return false;
         }
         return true;
      }
      /// <summary>
      /// Prepares a single .nuspec file for packaging
      /// </summary>
      /// <param name="specItem">
      /// The .nuspec file to prepare
      /// </param>
      private void PreparePackage (ITaskItem specItem)
      {
         // parse the .nuspec file
         // extract elements without namespaces to avoid
         // issues with multiple .nuspec versions
         var specPath = specItem.GetMetadata("FullPath");
         var specDoc = XDocument.Load(specPath);
         var fileElems = specDoc
            .Root
            .Elements()
            .Where(e => e.Name.LocalName == "files")
            .SelectMany(e => e.Elements())
            .Where(e => e.Attribute("src") != null);
         // construct the path to the NuGet package to build
         var pkgID = GetPackageID(specDoc);
         var pkgVersion = GetPackageVersion(specItem, specDoc);
         var pkgFile = this.VersionFileName ?
            String.Format("{0}.{1}.nupkg", pkgID, pkgVersion) :
            String.Format("{0}.nupkg", pkgID);
         var pkgPath = Path.Combine(this.OutputPath, pkgFile);
         // add custom metadata to the .nuspec build item
         specItem.SetMetadata("NuPackagePath", pkgPath);
         if (pkgVersion != null)
            specItem.SetMetadata("NuPackageVersion", pkgVersion.ToString());
         // add the list of files referenced by the .nuspec file
         // to the dependency list for the build, and add the
         // package file to the target list
         this.sourceList.AddRange(
            fileElems.Select(
               e => new TaskItem(
                  Path.Combine(
                     specItem.GetMetadata("RootDir"),
                     specItem.GetMetadata("Directory"),
                     e.Attribute("src").Value
                  )
               )
            )
         );
         this.targetList.Add(new TaskItem(pkgPath));
      }
      /// <summary>
      /// Retrieves the package's ID
      /// </summary>
      /// <param name="specDoc">
      /// The nuspec document
      /// </param>
      /// <returns>
      /// The package ID, after replacements have been applied
      /// </returns>
      String GetPackageID (XDocument specDoc)
      {
         var id = specDoc
            .Root
            .Elements()
            .Single(e => e.Name.LocalName == "metadata")
            .Elements()
            .Single(e => e.Name.LocalName == "id")
            .Value;
         if (id == "$id$")
         {
            foreach (var libItem in this.ReferenceLibraries)
            {
               var assemblyName = (String)null;
               try
               {
                  assemblyName = RemoteAssemblyProxy.ExecuteGetter<String>("TempPackageDomain", libItem.GetMetadata("FullPath"),
                      ra => ra.GetPropertyValue<String>(
                         a => a.GetName().Name));
               }
               catch { }
               if (assemblyName != null)
                  return assemblyName;
            }
         }
         return id;
      }
      /// <summary>
      /// Constructs a nuget package version for the current project
      /// </summary>
      /// <param name="specItem">
      /// The .nuspec file to compile
      /// </param>
      /// <param name="specDoc">
      /// The parsed .nuspec XML document
      /// </param>
      /// <returns>
      /// The current package version
      /// </returns>
      private SemanticVersion GetPackageVersion (ITaskItem specItem, XDocument specDoc)
      {
         var verElem = specDoc
            .Root
            .Elements()
            .Single(e => e.Name.LocalName == "metadata")
            .Elements()
            .Single(e => e.Name.LocalName == "version");
         var specVer = new SemanticVersion(
            verElem.Value != "$version$" ? verElem.Value : "0.0.0.0"
         );
         switch (this.versionSource)
         {
            case NuBuild.VersionSource.Library:
               return GetLibraryVersion(specItem, specVer, specDoc);
            case NuBuild.VersionSource.Auto:
               return GetAutoVersion(specItem, specVer, specDoc);
            case NuBuild.VersionSource.Manual:
               return specVer;
            default:
               return null;
         }
      }
      /// <summary>
      /// Constructs a nuget package version from a library referenced
      /// by the package
      /// </summary>
      /// <param name="specItem">
      /// The .nuspec file to compile
      /// </param>
      /// <param name="specDoc">
      /// The parsed .nuspec XML document
      /// </param>
      /// <param name="specVer">
      /// The .nuspec manifest version
      /// </param>
      /// <returns>
      /// The version of the first library referenced by the package
      /// that includes a version number if found
      /// The version in the .nuspec file otherwise
      /// </returns>
      private SemanticVersion GetLibraryVersion (
         ITaskItem specItem, 
         SemanticVersion specVer,
         XDocument specDoc)
      {
         var fileElems = specDoc
            .Root
            .Elements()
            .Where(e => e.Name.LocalName == "files")
            .SelectMany(e => e.Elements())
            .Where(e => e.Attribute("src") != null);
         // retrieve the library version
         // . iterate through the libraries specified either
         //   through project references or explicitly in the
         //   .nuspec file
         // . attempt to retrieve the product version for each
         //   file
         // . return the version of the first file with a version
         return this.ReferenceLibraries
            .Select(i => i.GetMetadata("FullPath"))
            .Concat(
               fileElems.Select(
                  e => Path.Combine(
                     specItem.GetMetadata("RootDir"),
                     specItem.GetMetadata("Directory"),
                     e.Attribute("src").Value
                  )
               )
            ).Where(p => File.Exists(p))
            .Select(p => FileVersionInfo.GetVersionInfo(p).ProductVersion)
            .Where(v => v != null)
            .Select(v => new SemanticVersion(v))
            .Where(v => v != new SemanticVersion(0, 0, 0, 0))
            .DefaultIfEmpty(specVer)
            .First();
      }
      /// <summary>
      /// Constructs the nuget package version using either the build
      /// number specified in the project file (for TFS builds) or
      /// using a text file in the output directory
      /// </summary>
      /// <param name="specItem">
      /// The .nuspec file to compile
      /// </param>
      /// <param name="specDoc">
      /// The parsed .nuspec XML document
      /// </param>
      /// <param name="specVer">
      /// The .nuspec manifest version
      /// </param>
      /// <returns>
      /// The current version number; the major/minor parts of the version
      /// are taken from the .nuspec file; the build number is determined
      /// automatically
      /// </returns>
      private SemanticVersion GetAutoVersion (
         ITaskItem specItem,
         SemanticVersion specVer,
         XDocument specDoc)
      {
         // if the build number was not specified in the project 
         // configuration (using $(BuildNumber)), generate it using a 
         // text file <project>.build.number in the output directory
         if (this.BuildNumber == 0)
         {
            // retrieve the current build number
            var buildFile = Path.Combine(
               this.OutputPath,
               String.Format("{0}.build.number", this.ProjectName)
            );
            var buildStr = (File.Exists(buildFile)) ?
               File.ReadAllText(buildFile) : null;
            // parse, increment, and store the new build number
            var buildNum = 0;
            Int32.TryParse(buildStr, out buildNum);
            this.BuildNumber = buildNum + 1;
            File.WriteAllText(buildFile, this.BuildNumber.ToString());
         }
         return new SemanticVersion(
            specVer.Version.Major, 
            specVer.Version.Minor, 
            this.BuildNumber,
            specVer.Version.Revision
         );
      }
   }
}
