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
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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
      private Project propertyProject = null;
      private AppDomain asmDomain;

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
            // parepare the task for execution
            if (this.ReferenceLibraries == null)
               this.ReferenceLibraries = new ITaskItem[0];
            this.OutputPath = Path.GetFullPath(this.OutputPath);
            this.asmDomain = AppDomain.CreateDomain("NuBuild.MSBuild.NuPackage.Domain");
            // compile the nuget package
            foreach (var specItem in this.NuSpec)
               BuildPackage(specItem);
         }
         catch (Exception e)
         {
            Log.LogError("{0} ({1})", e.Message, e.GetType().Name);
            return false;
         }
         finally
         {
            if (this.asmDomain != null)
               AppDomain.Unload(this.asmDomain);
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
         // . folders may be overridden using NuBuildTargetFolder (lib\net40, etc.)
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
            // apply the custom folder override on the reference
            var customFolder = libItem.GetMetadata("NuBuildTargetFolder");
            if (!String.IsNullOrWhiteSpace(customFolder))
               tgtFolder = customFolder;
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
               var pdbPath = srcPath.Substring(0, srcPath.LastIndexOf('.')) + ".pdb";
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
      /// Retrieves a property from the current NuGet project file
      /// </summary>
      /// <param name="name">
      /// The name of the property to retrieve
      /// </param>
      /// <returns>
      /// The specified property value if found
      /// Null otherwise
      /// </returns>
      private String GetProjectProperty (String name)
      {
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
               .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.Name, name) == 0)
               .Select(p => p.EvaluatedValue)
               .FirstOrDefault();
         return null;
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
            try
            {
               var props = AssemblyReader.Read(libItem.GetMetadata("FullPath"));
               if (property == "id")
                  return props.Name;
               if (property == "version" && props.Version != null)
                  return props.Version.ToString();
               if (property == "description")
                  return props.Description;
               if (property == "author")
                  return props.Company;
            }
            catch { }
         }
         // if the property was not yet resolved, retrieve it
         // from the project properties
         return GetProjectProperty(property);
      }
      #endregion
   }
}
