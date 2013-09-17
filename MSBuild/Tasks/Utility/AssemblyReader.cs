//===========================================================================
// MODULE:  AssemblyReader.cs
// PURPOSE: Assembly property reader
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
// Project References

namespace NuBuild.MSBuild
{
   /// <summary>
   /// Assembly property reader class
   /// </summary>
   /// <remarks>
   /// This class reads properties from an assembly to include in
   /// the NuGet package. It loads assemblies into a separate AppDomain
   /// in order to avoid holding locks on assembly files (for rebuilds)
   /// and so that the assemblies can be unloaded.
   /// </remarks>
   public class AssemblyReader : MarshalByRefObject
   {
      private static Int32 assemblyIndex = 0;

      /// <summary>
      /// Reads common properties from an assembly file
      /// </summary>
      /// <param name="path">
      /// The file system path to the assembly
      /// </param>
      /// <returns>
      /// The properties of the assembly
      /// </returns>
      public static Properties Read (String path)
      {
         var domain = (AppDomain)null;
         try
         {
            // create a new AppDomain in which to load the assembly,
            // so that it can be unloaded
            domain = AppDomain.CreateDomain(
               String.Format(
                  "NuBuild.MSBuild.AssemblyReader.{0}",
                  Interlocked.Increment(ref assemblyIndex)
               ),
               null,
               new AppDomainSetup()
               {
                  ApplicationBase = Path.GetDirectoryName(path)
               }
            );
            // create an instance of the reader within the new AppDomain 
            // and load the assembly properties from within
            var reader = (AssemblyReader)domain.CreateInstanceFromAndUnwrap(
               Assembly.GetExecutingAssembly().CodeBase,
               typeof(AssemblyReader).FullName
            );
            return reader.ReadAssembly(path);
         }
         finally
         {
            AppDomain.Unload(domain);
         }
      }
      /// <summary>
      /// Retrieves the properties of an assembly from within
      /// the assembly's AppDomain
      /// </summary>
      /// <param name="path">
      /// The path to the assembly to load
      /// </param>
      /// <returns>
      /// The assembly's properties
      /// </returns>
      private Properties ReadAssembly (String path)
      {
         var asm = Assembly.Load(File.ReadAllBytes(path));
         // retrieve the assembly version
         var ver = asm.GetName().Version;
         if (ver == new Version(0, 0, 0, 0))
            ver = null;
         // retrieve the assembly company attribute
         var company = (String)null;
         var companyAttr = (AssemblyCompanyAttribute)asm
            .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
            .FirstOrDefault();
         if (companyAttr != null)
            company = companyAttr.Company;
         // retrieve the assembly description attribute
         var desc = (String)null;
         var descAttr = (AssemblyDescriptionAttribute)asm
            .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
            .FirstOrDefault();
         if (descAttr != null)
            desc = descAttr.Description;
         // return the assembly properties
         return new Properties()
         {
            Name = asm.GetName().Name,
            Version = ver,
            Description = desc,
            Company = company
         };
      }

      /// <summary>
      /// Assembly properties
      /// </summary>
      [Serializable]
      public struct Properties
      {
         public String Name { get; set; }
         public Version Version { get; set; }
         public String Company { get; set; }
         public String Description { get; set; }
      }
   }
}
