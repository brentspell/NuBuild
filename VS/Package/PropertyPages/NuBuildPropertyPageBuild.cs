//===========================================================================
// MODULE:  EnumDescriptionTypeConverter.cs
// PURPOSE: DescriptionAttribute-based type converter for enums
// 
// Copyright © 2013
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
using System.Runtime.InteropServices;
// Project References
using CSVSXProjectSubType.PropertyPageBase;

namespace NuBuild.VS
{
   [Guid("1409A43A-213F-4C1D-8BE8-E72C484238B2")]
   class NuBuildPropertyPageBuild : PropertyPage
   {

      #region Overriden Properties and Methods

      /// <summary>
      /// Help keyword that should be associated with the page
      /// </summary>
      protected override string HelpKeyword
      {
         // TODO: Put your help keyword here
         get { return String.Empty; }
      }

      /// <summary>
      /// Title of the property page.
      /// </summary>
      public override string Title
      {
         get { return "Build"; }
      }

      /// <summary>
      /// Provide the view of our properties.
      /// </summary>
      /// <returns></returns>
      protected override IPageView GetNewPageView()
      {
         return new NuBuildPropertyPageBuildView(this);
      }

      /// <summary>
      /// Use a store implementation designed for flavors.
      /// </summary>
      /// <returns>Store for our properties</returns>
      protected override IPropertyStore GetNewPropertyStore()
      {
         return new NuBuildPropertyPagePropertyStore();
      }

      #endregion
   }
}