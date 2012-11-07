using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace NuBuild.Vsix
{
   public sealed class NuBuildNode : ProjectNode
   {
      NuBuildPackage package;
      static Int32 imageIdx = 0;

      public NuBuildNode (NuBuildPackage package)
      {
         base.CanProjectDeleteItems = true;
         this.package = package;
         imageIdx = this.ImageHandler.ImageList.Images.Count;
         this.ImageHandler.ImageList.Images.Add(
            new System.Drawing.Icon(
               Assembly.GetExecutingAssembly()
                  .GetManifestResourceStream("NuBuild.Vsix.Resources.NuBuild.ico")
            )
         );
      }
      public override Guid ProjectGuid
      {
         get { return NuBuildFactory.FactoryGuid; }
      }
      public override String ProjectType
      {
         get { return "NuGet"; }
      }
      public override Int32 ImageIndex
      {
         get { return imageIdx; }
      }
      protected override void InitializeProjectProperties ()
      {
         // no default project properties
      }
      public override FileNode CreateFileNode (ProjectElement item)
      {
         if (item.Item.ItemType == "Compile")
            return new NuSpecFileNode(this, item);
         return base.CreateFileNode(item);
      }
      public override Boolean IsCodeFile (String fileName)
      {
         return (String.Compare(Path.GetExtension(fileName), ".nuspec", true) == 0);
      }

      private class NuSpecFileNode : FileNode
      {
         public NuSpecFileNode (ProjectNode root, ProjectElement element)
            : base(root, element)
         {
         }
         public override Int32 ImageIndex
         {
            get { return imageIdx; }
         }
      }
   }
}
