//===========================================================================
// MODULE:  NuBuildFactory.cs
// PURPOSE: NuBuild project node factory
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
using Microsoft.VisualStudio.Shell.Flavor;
// Project References

namespace NuBuild.VS
{
   /// <summary>
   /// Project node factory
   /// </summary>
   /// <remarks>
   /// This class represents the .nuproj project type and creates instances 
   /// of the NuBuildNode project node for each NuBuild project loaded in 
   /// Visual Studio.
   /// </remarks>
   [Guid(NuBuildFactory.FactoryGuidString)]
   public sealed class NuBuildFactory : FlavoredProjectFactoryBase
   {
      public const String FactoryGuidString = "e09dd79a-4488-4ab9-8d3f-a7eee78bf432";
      public static readonly Guid FactoryGuid = new Guid(FactoryGuidString);
      NuBuildPackage package;

      /// <summary>
      /// Initializes a new factory instance
      /// </summary>
      /// <param name="package">
      /// The current NuBuild VS package
      /// </param>
      public NuBuildFactory (NuBuildPackage package) : base()
      {
         this.package = package;
      }

      /// <summary>
      /// Create an instance of NuBuildNode. 
      /// The initialization will be done later when Visual Studio calls
      /// InitalizeForOuter on it.
      /// </summary>
      /// <param name="outerProjectIUnknown">
      /// This value points to the outer project. It is useful if there is a 
      /// Project SubType of this Project SubType.
      /// </param>
      /// <returns>
      /// An NuBuildNode instance that has not been initialized.
      /// </returns>
      protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
      {
         var project = new NuBuildNode(this.package);
         return project;
      }
   }
}
