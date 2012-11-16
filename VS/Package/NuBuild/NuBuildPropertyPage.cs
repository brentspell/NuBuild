//===========================================================================
// MODULE:  NuBuildPropertyPage.cs
// PURPOSE: NuBuild project main property page
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
// Project References

namespace NuBuild.VS
{
   /// <summary>
   /// NuGet property page
   /// </summary>
   /// <remarks>
   /// This property page edits the custom properties of the .nuproj
   /// property file.
   /// </remarks>
   [ComVisible(true)]
   [Guid("E9E3E8A2-43B6-4D03-AE14-A495135C2057")]
   public sealed class NuBuildPropertyPage : SettingsPage
   {
      private String outputPath;
      private VersionSource versionSource;

      /// <summary>
      /// Initializes a new property page instance
      /// </summary>
      public NuBuildPropertyPage ()
      {
         this.Name = "NuGet";
      }

      /// <summary>
      /// The path to the package output directory
      /// </summary>
      [Category("Package Generation")]
      [DisplayName("Output Path")]
      [Description("The path to which to copy project targets.")]
      public String OutputPath
      {
         get { return this.outputPath; }
         set { this.outputPath = value; this.IsDirty = true; }
      }

      /// <summary>
      /// The package version number source
      /// </summary>
      [Category("Package Generation")]
      [DisplayName("Version Source")]
      [Description("The version number generation method.")]
      public VersionSource VersionSource
      {
         get { return this.versionSource; }
         set { this.versionSource = value; this.IsDirty = true; }
      }

      /// <summary>
      /// Retrieves property values from the project file
      /// </summary>
      protected override void BindProperties ()
      {
         this.outputPath = this.ProjectMgr.GetProjectProperty(
            "OutputPath", 
            true
         );
         this.versionSource = (VersionSource)Enum.Parse(
            typeof(VersionSource),
            this.ProjectMgr.GetProjectProperty("NuBuildVersionSource", true),
            true
         );
      }
      /// <summary>
      /// Assigns property values to the project file
      /// </summary>
      /// <returns>
      /// S_OK if successful
      /// failure code otherwise
      /// </returns>
      protected override Int32 ApplyChanges ()
      {
         this.ProjectMgr.SetProjectProperty(
            "OutputPath", 
            this.outputPath
         );
         this.ProjectMgr.SetProjectProperty(
            "NuBuildVersionSource", 
            this.versionSource.ToString()
         );
         this.IsDirty = false;
         return VSConstants.S_OK;
      }
   }
}
