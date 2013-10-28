//===========================================================================
// MODULE:  NuBuildPackage.cs
// PURPOSE: NuBuild visual studio package
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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
// Project References

namespace NuBuild.VS
{
   /// <summary>
   /// Visual Studio package
   /// </summary>
   /// <remarks>
   /// This class is the entry point into the NuBuild project system from
   /// Visual Studio. It registers the project node factory with VS for
   /// servicing requests for the .nuproj project type.
   /// </remarks>
   [PackageRegistration(UseManagedResourcesOnly = true)]
   [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
   [Guid(NuBuildPackage.PackageGuidString)]
   [ProvideProjectFactory(
      typeof(NuBuildFactory),
      "NuGet packaging project",
      null,
      null,
      null,
      @"..\Templates\Projects")]
   [ProvideObject(typeof(NuBuildPropertyPagePackage), RegisterUsing = RegistrationMethod.CodeBase)]
   [ProvideObject(typeof(NuBuildPropertyPageBuild), RegisterUsing = RegistrationMethod.CodeBase)]
   public sealed class NuBuildPackage : Package
   {
      //public const String PackageGuidString = "e09dd79a-4488-4ab9-8d3f-a7eee78bf432";
      public const String PackageGuidString = "165BA684-99B5-4AC7-AE57-2E317F2181F1";
      public static readonly Guid PackageGuid = new Guid(PackageGuidString);

      /// <summary>
      /// Initializes a new package instance
      /// </summary>
      protected override void Initialize ()
      {
         base.Initialize();
         RegisterProjectFactory(new NuBuildFactory(this));
      }
   }
}
