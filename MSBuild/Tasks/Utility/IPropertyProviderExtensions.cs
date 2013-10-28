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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public static class IPropertyProviderExtensions
   {
      private static readonly Regex _tokenRegex = new Regex(@"\$(?<propertyName>\w+)\$");

      /// <summary>
      /// Replaces tokens in parameter text
      /// </summary>
      /// <param name="text">
      /// Text with tokens to replace
      /// </param>
      /// <param name="throwIfNotFound">
      /// Whether to throw if token not found
      /// </param>
      /// <returns>
      /// Text with replaced tokens
      /// </returns>
      public static string Process(this NuGet.IPropertyProvider propertyProvider, string text, bool throwIfNotFound = true)
      {
         return _tokenRegex.Replace(text, match => ReplaceToken(propertyProvider, match, throwIfNotFound));
      }

      /// <summary>
      /// Replaces tokens in parameter text
      /// </summary>
      /// <param name="text">
      /// Text with tokens to replace
      /// </param>
      /// <param name="throwIfNotFound">
      /// Whether to throw if token not found
      /// </param>
      /// <returns>
      /// Text with replaced tokens
      /// </returns>
      public static Stream Process(this NuGet.IPropertyProvider propertyProvider, Stream stream, bool throwIfNotFound = true)
      {
         return _tokenRegex.Replace(stream.ReadToEnd(), match => ReplaceToken(propertyProvider, match, throwIfNotFound)).AsStream();
      }

      private static string ReplaceToken(this NuGet.IPropertyProvider propertyProvider, Match match, bool throwIfNotFound)
      {
         string propertyName = match.Groups["propertyName"].Value;
         var value = propertyProvider.GetPropertyValue(propertyName);
         if (value == null && throwIfNotFound)
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The replacement token '{0}' has no value.", propertyName));
         return value;
      }
   }
}
