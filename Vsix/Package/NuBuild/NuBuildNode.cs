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
      Int32 imageIdx = 0;

      public NuBuildNode (NuBuildPackage package)
      {
         this.package = package;
         this.imageIdx = this.ImageHandler.ImageList.Images.Count;
         this.ImageHandler.ImageList.Images.Add(
            new System.Drawing.Icon(
               Assembly.GetExecutingAssembly()
                  .GetManifestResourceStream("NuBuild.Vsix.Resources.NuGet.ico")
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
      public override int ImageIndex
      {
         get { return this.imageIdx; }
      }
   }
}
