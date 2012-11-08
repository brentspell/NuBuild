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
   public sealed class NuPackage : Task
   {
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
         using (var specFile = 
            new FileStream(
               specPath, 
               FileMode.Open, 
               FileAccess.Read
            )
         )
            builder = new NuGet.PackageBuilder(
               specFile,
               Path.GetDirectoryName(specPath)
            );
         // initialize dynamic manifest properties
         builder.Version = new NuGet.SemanticVersion(
            specItem.GetMetadata("NuPackageVersion")
         );
         // add a new file to the "lib" folder for each project DLL
         // referenced by the current project
         if (this.ReferenceLibraries != null)
            foreach (var libItem in this.ReferenceLibraries)
               builder.Files.Add(
                  new NuGet.PhysicalPackageFile()
                  {
                     SourcePath = libItem.GetMetadata("FullPath"),
                     TargetPath = String.Format(
                        @"lib\{0}{1}",
                        libItem.GetMetadata("Filename"),
                        libItem.GetMetadata("Extension")
                     )
                  }
               );
         // write the configured package out to disk
         var pkgPath = specItem.GetMetadata("NuPackagePath");
         using (var pkgFile = 
            new FileStream(
               pkgPath, 
               FileMode.Create, 
               FileAccess.Read | FileAccess.Write
            )
         )
            builder.Save(pkgFile);
      }
   }
}
