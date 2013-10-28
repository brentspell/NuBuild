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
   [Guid("091FF2B3-1819-4909-9B3A-C8D561640F6E")]
   partial class NuBuildPropertyPageBuildView : PageView
   {
      public const string OutputPathPropertyTag = "OutputPath";
      public const string IncludePdbsPropertyTag = "NuBuildIncludePdbs";

      #region Constructors
      /// <summary>
      /// This is the runtime constructor.
      /// </summary>
      /// <param name="site">Site for the page.</param>
      public NuBuildPropertyPageBuildView(IPageViewSite site)
         : base(site)
      {
         InitializeComponent();
      }
      /// <summary>
      /// This constructor is only to enable winform designers
      /// </summary>
      public NuBuildPropertyPageBuildView()
      {
         InitializeComponent();
      }
      #endregion

      private PropertyControlTable propertyControlTable;

      /// <summary>
      /// This property is used to map the control on a PageView object to a property
      /// in PropertyStore object.
      /// 
      /// This property will be called in the base class's constructor, which means that
      /// the InitializeComponent has not been called and the Controls have not been
      /// initialized.
      /// </summary>
      protected override PropertyControlTable PropertyControlTable
      {
         get
         {
            if (propertyControlTable == null)
            {
               // This is the list of properties that will be persisted and their
               // assciation to the controls.
               propertyControlTable = new PropertyControlTable();

               // This means that this CustomPropertyPageView object has not been
               // initialized.
               if (string.IsNullOrEmpty(base.Name))
               {
                  this.InitializeComponent();
               }

               // Add two Property Name / Control KeyValuePairs. 
               propertyControlTable.Add(OutputPathPropertyTag, tbOutputPath);
               propertyControlTable.Add(IncludePdbsPropertyTag, chkIncludePdbs);
            }
            return propertyControlTable;
         }
      }
   }
}