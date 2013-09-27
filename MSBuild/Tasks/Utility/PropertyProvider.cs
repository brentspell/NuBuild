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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
// Project References

namespace NuBuild.MSBuild
{
   public class PropertyProvider : NuGet.IPropertyProvider
   {
      private static Dictionary<string, Func<AssemblyReader.Properties, string>> assemblyProperties = new Dictionary<string, Func<AssemblyReader.Properties, string>>()
      {
         { "id", p => p.Name },
         { "version", p => p.Version == null ? null : p.Version.ToString() },
         { "description", p => p.Description },
         { "copyright", p => p.Copyright },
         { "author", p => p.Company },
      };
      private static readonly Regex _tokenRegex = new Regex(@"\$(?<propertyName>\w+)\$");

      private string projectPath;
      private ITaskItem[] referenceLibraries;

      private Project propertyProject = null;

      public PropertyProvider(string projectPath, ITaskItem[] referenceLibraries)
      {
         this.projectPath = projectPath;
         this.referenceLibraries = referenceLibraries;
      }

      /// <summary>
      /// Replaces tokens in paramtere text
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
      public string Process(string text, bool throwIfNotFound = true)
      {
         return _tokenRegex.Replace(text, match => ReplaceToken(match, throwIfNotFound));
      }

      private string ReplaceToken(Match match, bool throwIfNotFound)
      {
         string propertyName = match.Groups["propertyName"].Value;
         var value = GetPropertyValue(propertyName);
         if (value == null && throwIfNotFound)
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The replacement token '{0}' has no value.", propertyName));
         return value;
      }

      /// <summary>
      /// Retrieves a property from the current NuGet project file
      /// </summary>
      /// <param name="propertyName">
      /// The name of the property to retrieve
      /// </param>
      /// <returns>
      /// The specified property value if found
      /// Null otherwise
      /// </returns>
      private String GetProjectProperty(String propertyName)
      {
         // attempt to resolve the requested property
         // from the project properties
         if (this.propertyProject == null)
         {
            // attempt to retrieve the project from the global collection
            // this should always work in Visual Studio
            // if there is no project in the global collection, create a new one
            this.propertyProject = ProjectCollection
               .GlobalProjectCollection
               .LoadedProjects
               .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.FullPath, this.projectPath) == 0)
               .FirstOrDefault()
               ?? new Project(this.projectPath);
         }
         if (this.propertyProject != null)
            return this.propertyProject.AllEvaluatedProperties
               .Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.Name, propertyName) == 0)
               .Select(p => p.EvaluatedValue)
               .FirstOrDefault();
         return null;
      }

      #region IPropertyProvider Members

      /// <summary>
      /// Retrieves nuget replacement values from a referenced
      /// assembly library or MSBuild property, as specified here:
      /// http://docs.nuget.org/docs/reference/nuspec-reference#Replacement_Tokens
      /// </summary>
      /// <param name="property">
      /// The replacement property to retrieve
      /// </param>
      /// <returns>
      /// The replacement property value
      /// </returns>
      public dynamic GetPropertyValue(string propertyName)
      {
         // attempt to resolve the property from the referenced libraries
         var propGet = (Func<AssemblyReader.Properties, string>)null;
         if (assemblyProperties.TryGetValue(propertyName, out propGet))
            foreach (var libItem in this.referenceLibraries)
               try
               {
                  var prop = propGet(AssemblyReader.Read(libItem.GetMetadata("FullPath")));
                  if (!String.IsNullOrWhiteSpace(prop))
                     return prop;
               }
               catch { }
         // if the property was not yet resolved, retrieve it
         // from the project properties
         return GetProjectProperty(propertyName);
      }

      #endregion
   }
}
