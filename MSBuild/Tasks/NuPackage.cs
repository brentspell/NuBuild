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

      #region Task Parameters
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
         this.OutputPath = Path.GetFullPath(this.OutputPath);
         try
         {
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
         if (this.ReferenceLibraries != null)
            AddLibraries(builder);
         // write the configured package out to disk
         var pkgPath = specItem.GetMetadata("NuPackagePath");
         using (var pkgFile = File.Create(pkgPath))
            builder.Save(pkgFile);
      }
      private void AddLibraries (NuGet.PackageBuilder builder)
      {
         // add package files from project references
         // . DLL references go in the lib package folder
         // . EXE references go in the tools package folder
         // . everything else goes in the content package folder
         foreach (var libItem in this.ReferenceLibraries)
         {
            var ext = libItem.GetMetadata("Extension").ToLower();
            var folder = "content";
            if (ext == ".dll")
               folder = "lib";
            else if (ext == ".exe")
               folder = "tools";
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
            var asm = (Assembly)null;
            try
            {
               asm = Assembly.Load(File.ReadAllBytes(libItem.GetMetadata("FullPath")));
            }
            catch { }
            if (asm != null)
            {
               if (property == "id")
                  return asm.GetName().Name;
               if (property == "version")
                  if (asm.GetName().Version != new Version(0, 0, 0, 0))
                     return asm.GetName().Version.ToString();
               if (property == "author")
               {
                  var attr = (AssemblyCompanyAttribute)asm
                     .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
                     .FirstOrDefault();
                  if (attr != null)
                     return attr.Company;
               }
               if (property == "description")
               {
                  var attr = (AssemblyDescriptionAttribute)asm
                     .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                     .FirstOrDefault();
                  if (attr != null)
                     return attr.Description;
               }
            }
         }
         // attempt to resolve the property from MSBuild
         if (this.propertyProject == null)
            this.propertyProject = new Project(System.Xml.XmlReader.Create(this.BuildEngine.ProjectFileOfTaskNode));
         return this.propertyProject.GetPropertyValue(property);
      }
      #endregion
   }
}
