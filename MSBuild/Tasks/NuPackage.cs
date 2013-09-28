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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet;
// Project References

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
      private string version;
      private IPropertyProvider propertyProvider;

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
      /// The EmbeddedResource file items
      /// </summary>
      [Required]
      public ITaskItem[] Embedded { get; set; }
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
      /// <summary>
      /// Specifies whether to add binaries (.dll and .exe files) from referenced projects into subfolders
      /// (eg. lib\net40) based on TargetFrameworkVersion 
      /// </summary>
      public Boolean AddBinariesToSubfolder { get; set; }
      /// <summary>
      /// Specifies whether to add PDB files for binaries
      /// automatically to the package
      /// </summary>
      public Boolean IncludePdbs { get; set; }
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
            // prepare the task for execution
            if (this.ReferenceLibraries == null)
               this.ReferenceLibraries = new ITaskItem[0];
            this.OutputPath = Path.GetFullPath(this.OutputPath);
            propertyProvider = new PropertyProvider(ProjectPath, ReferenceLibraries);
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
         var builder = new NuGet.PackageBuilder(
            specPath,
            this as NuGet.IPropertyProvider,
            true
         );
         // initialize dynamic manifest properties
         version = specItem.GetMetadata("NuPackageVersion");
         if (!String.IsNullOrEmpty(version))
            builder.Version = new NuGet.SemanticVersion(version);
         // add a new file to the lib/tools/content folder for each project
         // referenced by the current project
         AddLibraries(builder);
         // add a new file to the tools/content folder for each project
         // specified as embedded resource by the current project
         AddEmbedded(builder);
         // write the configured package out to disk
         var pkgPath = specItem.GetMetadata("NuPackagePath");
         using (var pkgFile = File.Create(pkgPath))
            builder.Save(pkgFile);
      }
      /// <summary>
      /// Adds project references to the package lib/tools/content section
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
         // . folders may be overridden using NuBuildTargetFolder metadata (lib\net40, etc.)
         // . folders may be overridden using TargetFramework attribute (lib\net40, etc.)
         foreach (var libItem in this.ReferenceLibraries)
         {
            var srcPath = libItem.GetMetadata("FullPath");
            var srcExt = Path.GetExtension(srcPath).ToLower();
            var tgtFolder = "content";
            var hasPdb = false;
            if (srcExt == ".dll")
            {
               tgtFolder = "lib";
               hasPdb = true;
            }
            else if (srcExt == ".exe")
            {
               tgtFolder = "tools";
               hasPdb = true;
            }
            // apply the custom folder override on the reference, or based on TargetFramework
            var customFolder = libItem.GetMetadata("NuBuildTargetFolder");
            if (!String.IsNullOrWhiteSpace(customFolder))
               tgtFolder = customFolder;
            else if (AddBinariesToSubfolder)
               try
               {
                  var targetFrameworkName = AssemblyReader.Read(srcPath).TargetFrameworkName;
                  if (!String.IsNullOrWhiteSpace(targetFrameworkName))
                     tgtFolder = Path.Combine(tgtFolder, VersionUtility.GetShortFrameworkName(new FrameworkName(targetFrameworkName)));
               }
               catch { }
            // add the source library file to the package
            builder.Files.Add(
               new NuGet.PhysicalPackageFile()
               {
                  SourcePath = srcPath,
                  TargetPath = String.Format(
                     @"{0}\{1}",
                     tgtFolder,
                     Path.GetFileName(srcPath)
                  )
               }
            );
            // add PDBs if specified and exist
            if (hasPdb && this.IncludePdbs)
            {
               var pdbPath = Path.ChangeExtension(srcPath, ".pdb");
               if (File.Exists(pdbPath))
                  builder.Files.Add(
                     new NuGet.PhysicalPackageFile()
                     {
                        SourcePath = pdbPath,
                        TargetPath = String.Format(
                           @"{0}\{1}",
                           tgtFolder,
                           Path.GetFileName(pdbPath)
                        )
                     }
                  );
            }
         }
      }
      /// <summary>
      /// Adds embedded resources to the package tools/content section
      /// </summary>
      /// <param name="builder">
      /// The current package builder
      /// </param>
      private void AddEmbedded(NuGet.PackageBuilder builder)
      {
         // add package files from project embedded resources
         foreach (var fileItem in this.Embedded)
         {
            // only link items has Link metadata, that is the path, where the link itself is located (not the referred item)
            var tgtPath = fileItem.GetMetadata("Link");
            // if it is not a link, handle as normal file
            if (String.IsNullOrEmpty(tgtPath))
               tgtPath = fileItem.GetMetadata("Identity");
            if (tgtPath.IndexOf('\\') == -1)
               // warning if file is not in a subfolder
               Log.LogWarning(
                  "The source item '{0}' is not a valid content! Files has to be in a subfolder, like content or tools! File skipped.",
                  tgtPath);
            else
            {
               // determine pre package processing necessity
               var prePackProc = tgtPath.EndsWith(".ppp");
               if (prePackProc)
                  tgtPath = tgtPath.Substring(0, tgtPath.Length - 4);
               // create the source file
               var file = prePackProc ? new TokenProcessingPhysicalPackageFile(this) : new PhysicalPackageFile();
               file.SourcePath = fileItem.GetMetadata("FullPath");
               file.TargetPath = tgtPath;
               // add the source file to the package
               builder.Files.Add(file);
            }
         }
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
         if (property == "version" && !String.IsNullOrEmpty(version))
            return version;
         else
            return propertyProvider.GetPropertyValue(property);
      }
      #endregion
   }
}
