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
using System.Reflection;
using System.Drawing;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
// Project References

namespace NuBuild.VS
{
   /// <summary>
   /// Project node
   /// </summary>
   /// <remarks>
   /// This class represents a running project flavor instance within Visual 
   /// Studio. It overrides default behavior where necessary.
   /// </remarks>
   public sealed class NuBuildNode : FlavoredProjectBase
   {
      NuBuildPackage package;
      Icon projectIcon;
      
      /// <summary>
      /// Initializes a new project instance
      /// </summary>
      /// <param name="package">
      /// The current NuBuild VS package
      /// </param>
      public NuBuildNode (NuBuildPackage package)
      {
         this.package = package;
         this.projectIcon = new System.Drawing.Icon(
            Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("NuBuild.VS.Resources.NuProj.ico"));
      }

      /// <summary>
      /// This is were all QI for interface on the inner object should happen. 
      /// Then set the inner project wait for InitializeForOuter to be called to do
      /// the real initialization
      /// </summary>
      /// <param name="innerIUnknown"></param>
      protected override void SetInnerProject(IntPtr innerIUnknown)
      {
         if (base.serviceProvider == null)
            base.serviceProvider = this.package;
         base.SetInnerProject(innerIUnknown);
      }

      /// <summary>
      ///  By overriding GetProperty method and using propId parameter containing one of 
      ///  the values of the __VSHPROPID2 enumeration, we can filter, add or remove project
      ///  properties. 
      ///  
      ///  For example, to add a page to the configuration-dependent property pages, we
      ///  need to filter configuration-dependent property pages and then add a new page 
      ///  to the existing list. 
      /// </summary>
      protected override int GetProperty(uint itemId, int propId, out object property)
      {
         if(propId == (int)__VSHPROPID.VSHPROPID_IconIndex || propId == (int)__VSHPROPID.VSHPROPID_OpenFolderIconIndex)
         {
            // remove original project icon
            if (itemId == VSConstants.VSITEMID_ROOT)
            {
               property = null;                                
               return VSConstants.E_NOTIMPL;
            }
         }
         else if(propId == (int)__VSHPROPID.VSHPROPID_IconHandle || propId == (int)__VSHPROPID.VSHPROPID_OpenFolderIconHandle)
         {
            // new project icon
            if (itemId == VSConstants.VSITEMID_ROOT)
            {
               property = projectIcon.Handle;
               return VSConstants.S_OK;
            }
         } 
         else if (propId == (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList)
         {
            // configuration dependent property pages
            // original pages:
            // Build "{A54AD834-9219-4AA6-B589-607AF21C3E26}"
            // Debug "{6185191F-1008-4FB2-A715-3A4E4F27E610}"
            // Code Analysis "{984AE51A-4B21-44E7-822C-DD5E046893EF}"

            string pageList = string.Empty;
            AddToCLSIDList(ref pageList, new string[]
            {
               typeof(NuBuildPropertyPageBuild).GUID.ToString("B"),
            });

            property = pageList;
            return VSConstants.S_OK;
         }
         else if (propId == (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList)
         {
            // project property pages
            // original pages:
            // Application "{5E9A8AC2-4F34-4521-858F-4C248BA31532}"
            // Build Events "{1E78F8DB-6C07-4D61-A18F-7514010ABD56}"
            // Services "{43E38D2E-43B8-4204-8225-9357316137A4}"
            // Reference Paths "{031911C8-6148-4E25-B1B1-44BCA9A0C45C}"
            // Signing "{F8D6553F-F752-4DBF-ACB6-F291B744A792}"
            // Security "{DF8F7042-0BB1-47D1-8E6D-DEB3D07698BD}"
            // Publish "{CC4014F5-B18D-439C-9352-F99D984CCA85}"

            string pageList = string.Empty;
            AddToCLSIDList(ref pageList, new string[]
            {
               typeof(NuBuildPropertyPagePackage).GUID.ToString("B"),
               "{1E78F8DB-6C07-4D61-A18F-7514010ABD56}", // original Build Events
            });
            
            property = pageList;
            return VSConstants.S_OK;
         }
         else if (propId == (int)__VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList)
         {
            // order of property pages
            // original pages:
            // Application "{5E9A8AC2-4F34-4521-858F-4C248BA31532}"
            // Build "{A54AD834-9219-4AA6-B589-607AF21C3E26}"
            // Build Events "{1E78F8DB-6C07-4D61-A18F-7514010ABD56}"
            // Debug "{6185191F-1008-4FB2-A715-3A4E4F27E610}"
            // Resources "{FF4D6ACA-9352-4A5F-821E-F4D6EBDCAB11}"
            // Services "{43E38D2E-43B8-4204-8225-9357316137A4}"
            // Settings "{6D2695F9-5365-4A78-89ED-F205C045BFE6}"
            // Reference Paths "{031911C8-6148-4E25-B1B1-44BCA9A0C45C}"
            // Signing "{F8D6553F-F752-4DBF-ACB6-F291B744A792}"
            // Security "{DF8F7042-0BB1-47D1-8E6D-DEB3D07698BD}"
            // Publish "{CC4014F5-B18D-439C-9352-F99D984CCA85}"

            string pageList = string.Empty;
            AddToCLSIDList(ref pageList, new string[]
            {
               typeof(NuBuildPropertyPagePackage).GUID.ToString("B"),
               typeof(NuBuildPropertyPageBuild).GUID.ToString("B"),
               "{1E78F8DB-6C07-4D61-A18F-7514010ABD56}", // Build Events
            });
            
            property = pageList;
            return VSConstants.S_OK;
         }
         return base.GetProperty(itemId, propId, out property);
      }

      //private void RemoveFromCLSIDList(ref string pageList, string pageGuidString)
      //{
      //   // Remove the specified page guid from the string of guids.
      //   int index =
      //       pageList.IndexOf(pageGuidString, StringComparison.OrdinalIgnoreCase);

      //   if (index != -1)
      //   {
      //      // Guids are separated by ';', so we need to ensure we remove the ';' 
      //      // when removing the last guid in the list.
      //      int index2 = index + pageGuidString.Length + 1;
      //      if (index2 >= pageList.Length)
      //      {
      //         pageList = pageList.Substring(0, index).TrimEnd(';');
      //      }
      //      else
      //      {
      //         pageList = pageList.Substring(0, index) + pageList.Substring(index2);
      //      }
      //   }
      //   else
      //   {
      //      throw new ArgumentException(
      //          string.Format("Cannot find the Page {0} in the Page List {1}",
      //          pageGuidString, pageList));
      //   }
      //}

      private void AddToCLSIDList(ref string pageList, string[] pageGuids)
      {
         if (pageGuids != null && pageGuids.Length > 0)
         {
            // Create a StringBuilder with a pre-allocated buffer big enough for the
            // final string. 39 is the length of a GUID in the "B" form plus the final ';'
            StringBuilder stringList = new StringBuilder(pageList.Length + 39 * pageGuids.Length);
            for (int i = 0; i < pageGuids.Length; i++)
            {
               if (stringList.Length > 0)
                  stringList.Append(";");
               stringList.Append(pageGuids[i]);
            }
            pageList = stringList.ToString();
         }
      }
   }
}
