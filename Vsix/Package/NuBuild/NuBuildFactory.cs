using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NuBuild.Vsix
{
   [Guid(NuBuildFactory.FactoryGuidString)]
   public sealed class NuBuildFactory : ProjectFactory
   {
      public const String FactoryGuidString = "e09dd79a-4488-4ab9-8d3f-a7eee78bf432";
      public static readonly Guid FactoryGuid = new Guid(FactoryGuidString);
      NuBuildPackage package;

      public NuBuildFactory (NuBuildPackage package) : base(package)
      {
         this.package = package;
      }

      protected override ProjectNode CreateProject ()
      {
         var project = new NuBuildNode(this.package);
         var provider = (IServiceProvider)this.package;
         var olesite = (IOleServiceProvider)provider.GetService(
            typeof(IOleServiceProvider)
         );
         project.SetSite(olesite);
         return project;
      }
      protected override void CreateProject (string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled)
      {
         base.CreateProject(fileName, location, name, flags, ref projectGuid, out project, out canceled);
      }
   }
}
