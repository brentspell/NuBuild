//===========================================================================
// MODULE:  GacApi.cs
// PURPOSE: Query GAC
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
using System.Text;
using System.Runtime.InteropServices;
// Project References

namespace NuBuild.MSBuild
{
   public static class GacApi
   {
      // GAC Interfaces - IAssemblyCache. As a sample, non used vtable entries 
      [ComImport]
      [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
      [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
      private interface IAssemblyCache
      {
         int Dummy1();
         [PreserveSig()]
         IntPtr QueryAssemblyInfo(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)]
            string assemblyName,
            ref AssemblyInfo assemblyInfo);

         int Dummy2();
         int Dummy3();
         int Dummy4();
      }

      [StructLayout(LayoutKind.Sequential)]
      private struct AssemblyInfo
      {
         public int cbAssemblyInfo;
         public int assemblyFlags;
         public long assemblySizeInKB;

         [MarshalAs(UnmanagedType.LPWStr)]
         public string currentAssemblyPath;

         public int cchBuf;
      }

      [DllImport("fusion.dll")]
      private static extern IntPtr CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);

      /// <summary>
      /// Determines whether the specified assembly exists.
      /// </summary>
      /// <param name="assemblyName">Name of the assembly.</param>
      /// <returns></returns>
      public static bool AssemblyExist(string assemblyName)
      {
         try
         {
            QueryAssemblyInfo(assemblyName);
            return true;
         }
         catch (System.IO.FileNotFoundException)
         {
            return false;
         }
      }

      /// <summary>
      /// Queries info about the specified assembly.
      /// </summary>
      /// <param name="assemblyName">Name of the assembly</param>
      /// <returns></returns>
      public static string QueryAssemblyInfo(string assemblyName)
      {
         var assembyInfo = new AssemblyInfo { cchBuf = 512 };
         assembyInfo.currentAssemblyPath = new String(' ', assembyInfo.cchBuf);
         IAssemblyCache assemblyCache;

         // If assemblyName is not fully qualified, a random matching may be 
         var hr = GacApi.CreateAssemblyCache(out assemblyCache, 0);
         if (hr == IntPtr.Zero)
         {
            hr = assemblyCache.QueryAssemblyInfo(1, assemblyName, ref assembyInfo);
            if (hr != IntPtr.Zero)
               Marshal.ThrowExceptionForHR(hr.ToInt32());
         }
         else
            Marshal.ThrowExceptionForHR(hr.ToInt32());
         return assembyInfo.currentAssemblyPath;
      }
   }
}