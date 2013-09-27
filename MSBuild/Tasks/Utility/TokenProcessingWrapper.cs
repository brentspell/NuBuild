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
using System.Text;
using System.Runtime.Versioning;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public class TokenProcessingWrapper : IPackageFile
   {
      private PhysicalPackageFile file;
      private IPropertyProvider propertyProvider;

      public TokenProcessingWrapper(PhysicalPackageFile file, IPropertyProvider propertyProvider)
      {
         this.file = file;
         this.propertyProvider = propertyProvider;
         //SourcePath = file.SourcePath;
         //TargetPath = file.TargetPath;
      }

      /// <summary>
      /// Path on disk
      /// </summary>
      public string SourcePath
      {
         get { return file.SourcePath; }
         set { file.SourcePath = value; }
      }

      /// <summary>
      /// Path in package
      /// </summary>
      public string TargetPath
      {
         get { return file.TargetPath; }
         set { file.TargetPath = value; }
      }

      #region IPackageFile Implementation

      public string Path
      {
         get { return TargetPath; }
      }

      public string EffectivePath
      {
         get;
         private set;
      }

      public FrameworkName TargetFramework
      {
         get { return file.TargetFramework; }
      }

      public IEnumerable<FrameworkName> SupportedFrameworks
      {
         get { return file.SupportedFrameworks; }
      }

      public Stream GetStream()
      {
         return propertyProvider.Process(file.GetStream());
      }

      #endregion

      public override string ToString()
      {
         return TargetPath;
      }

      public override bool Equals(object obj)
      {
         var file = obj as TokenProcessingWrapper;

         return file != null && String.Equals(SourcePath, file.SourcePath, StringComparison.OrdinalIgnoreCase) &&
                                String.Equals(TargetPath, file.TargetPath, StringComparison.OrdinalIgnoreCase);
      }

      public override int GetHashCode()
      {
         return file.GetHashCode();
      }
   }
}
