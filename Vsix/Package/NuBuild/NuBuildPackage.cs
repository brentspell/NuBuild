using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace NuBuild.Vsix
{
   [PackageRegistration(UseManagedResourcesOnly = true)]
   [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
   [Guid(NuBuildPackage.PackageGuidString)]
   [ProvideProjectFactory(
      typeof(NuBuildFactory),
      null,
      "NuBuild Project Files (*.nuproj)",
      "nuproj",
      "nuproj",
      @".\NullPath",
      LanguageVsTemplate = "NuGet")
   ]
   public sealed class NuBuildPackage : ProjectPackage
   {
      public const String PackageGuidString = "d2ab0959-d15b-4e44-8a06-c999cc386e34";
      public static readonly Guid PackageGuid = new Guid(PackageGuidString);

      public NuBuildPackage ()
      {
      }

      #region ProjectPackage Implementation
      protected override void Initialize ()
      {
         base.Initialize();
         RegisterProjectFactory(new NuBuildFactory(this));
      }
      public override String ProductUserContext
      {
         get { return null; }
      }
      #endregion
   }
}
