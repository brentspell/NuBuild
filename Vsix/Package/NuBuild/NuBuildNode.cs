//===========================================================================
// MODULE:  NuBuildNode.cs
// PURPOSE: NuBuild project node
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
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Project;
// Project References

namespace NuBuild.Vsix
{
   /// <summary>
   /// Project node
   /// </summary>
   /// <remarks>
   /// This class represents a running project instance within Visual 
   /// Studio. It relies on the MPF ProjectNode implementation, overriding
   /// default behavior where necessary.
   /// </remarks>
   public sealed class NuBuildNode : ProjectNode
   {
      NuBuildPackage package;

      /// <summary>
      /// Initializes a new project instance
      /// </summary>
      /// <param name="package">
      /// The current NuBuild VS package
      /// </param>
      public NuBuildNode (NuBuildPackage package)
      {
         base.CanProjectDeleteItems = true;
         this.package = package;
         this.NuProjImageIndex = this.ImageHandler.ImageList.Images.Count;
         this.ImageHandler.ImageList.Images.Add(
            new System.Drawing.Icon(
               Assembly.GetExecutingAssembly()
                  .GetManifestResourceStream("NuBuild.Vsix.Resources.NuProj.ico")
            )
         );
         this.NuSpecImageIndex = this.ImageHandler.ImageList.Images.Count;
         this.ImageHandler.ImageList.Images.Add(
            new System.Drawing.Icon(
               Assembly.GetExecutingAssembly()
                  .GetManifestResourceStream("NuBuild.Vsix.Resources.NuSpec.ico")
            )
         );
      }
      /// <summary>
      /// NuBuild project type GUID
      /// </summary>
      public override Guid ProjectGuid
      {
         get { return NuBuildFactory.FactoryGuid; }
      }
      /// <summary>
      /// NuBuild project type name
      /// </summary>
      public override String ProjectType
      {
         get { return "NuGet"; }
      }
      /// <summary>
      /// The image list index for the project tree node
      /// </summary>
      public override Int32 ImageIndex
      {
         get { return this.NuProjImageIndex; }
      }
      public Int32 NuProjImageIndex
      {
         get; private set;
      }
      public Int32 NuSpecImageIndex
      {
         get; private set;
      }
      /// <summary>
      /// Creates the default MSBuild properties for the project
      /// </summary>
      protected override void InitializeProjectProperties ()
      {
         // no default project properties
      }
      /// <summary>
      /// Creates a new file in the project
      /// </summary>
      /// <param name="item">
      /// The project element being added
      /// </param>
      /// <returns>
      /// The new project element file node
      /// </returns>
      public override FileNode CreateFileNode (ProjectElement item)
      {
         if (item.Item.ItemType == "Compile")
            return new NuSpecFileNode(this, item);
         return base.CreateFileNode(item);
      }
      /// <summary>
      /// Specifies whether a project item is a Compile item
      /// </summary>
      /// <param name="fileName">
      /// The project item file name
      /// </param>
      /// <returns>
      /// True if the project item is a Compile item
      /// False otherwise
      /// </returns>
      public override Boolean IsCodeFile (String fileName)
      {
         return (String.Compare(Path.GetExtension(fileName), ".nuspec", true) == 0);
      }
      /// <summary>
      /// Retrieves the IDs of the project property pages
      /// </summary>
      /// <returns>
      /// Property page GUID list
      /// </returns>
      protected override Guid[] GetConfigurationIndependentPropertyPages ()
      {
         return new[] { typeof(NuBuildPropertyPage).GUID };
      }
      /// <summary>
      /// Retrieves the IDs of the project property pages
      /// </summary>
      /// <returns>
      /// Property page GUID list
      /// </returns>
      protected override Guid[] GetPriorityProjectDesignerPages ()
      {
         return new[] { typeof(NuBuildPropertyPage).GUID };
      }

      /// <summary>
      /// Specialized file tree node for .nuspec files
      /// </summary>
      private class NuSpecFileNode : FileNode
      {
         NuBuildNode project;

         /// <summary>
         /// Initializes a new node instance
         /// </summary>
         /// <param name="project">
         /// The current project node
         /// </param>
         /// <param name="element">
         /// The project file element being added
         /// </param>
         public NuSpecFileNode (
            NuBuildNode project, 
            ProjectElement element)
            : base(project, element)
         {
            this.project = project;
         }
         /// <summary>
         /// The image list index for the file tree node
         /// </summary>
         public override Int32 ImageIndex
         {
            get { return this.project.NuSpecImageIndex; }
         }
      }
   }
}
