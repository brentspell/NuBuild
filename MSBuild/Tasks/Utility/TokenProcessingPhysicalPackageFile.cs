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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public class TokenProcessingPhysicalPackageFile : PhysicalPackageFile, IPackageFile
   {
      private static readonly Regex _tokenRegex = new Regex(@"\$\((?<propertyName>\w+)\)\$");
      private IPropertyProvider propertyProvider;

      public TokenProcessingPhysicalPackageFile(IPropertyProvider propertyProvider)
      {
         this.propertyProvider = propertyProvider;
      }

      #region IPackageFile Implementation

      public new Stream GetStream()
      {
         return _tokenRegex.Replace(propertyProvider.Process(base.GetStream().ReadToEnd()),
             match => "$" + match.Groups["propertyName"].Value + "$").AsStream();
      }

      #endregion

      public override bool Equals(object obj)
      {
         var file = obj as TokenProcessingPhysicalPackageFile;

         return file != null && String.Equals(SourcePath, file.SourcePath, StringComparison.OrdinalIgnoreCase) &&
                                String.Equals(TargetPath, file.TargetPath, StringComparison.OrdinalIgnoreCase);
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
   }
}
