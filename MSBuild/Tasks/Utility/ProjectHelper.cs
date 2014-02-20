//===========================================================================
// MODULE:  ProjectHelper.cs
// PURPOSE: ProjectFactory helper
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
// this class is based on http://nuget.codeplex.com/
//===========================================================================
// System References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public static class ProjectHelper
   {
      private static readonly HashSet<string> supportedProjectExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
      {  
         ".csproj",
         ".vbproj",
         ".fsproj",
      };

      private static readonly List<string> csDefaultReferences = new List<string> {
         // TODO get default references for other than basic library CS projects
         "Microsoft.CSharp", 
         "System", 
         "System.Core", 
         "System.Data", 
         "System.Data.DataSetExtensions", 
         "System.Xml", 
         "System.Xml.Linq",
      };

      private static readonly List<string> vbDefaultReferences = new List<string> {
         // TODO get default references for basic and other library VB projects
         "System", 
      };

      private static readonly List<string> fsDefaultReferences = new List<string> {
         // TODO get default references for other than basic library FS projects
         "FSharp.Core",
         "mscorlib",
         "System", 
         "System.Core", 
         "System.Numerics",
      };

      public static IEnumerable<ProjectItem> GetItems(this Project project, string itemType1, string itemType2)
      {
         return project.GetItems(itemType1).Concat(project.GetItems(itemType2));
      }

      public static IEnumerable<string> FullPath(this IEnumerable<ProjectItem> projectItems)
      {
         return projectItems
            .Select(item => item.GetMetadataValue("FullPath"));
      }
      
      public static IEnumerable<string> FullPath(this IEnumerable<ITaskItem> taskItems)
      {
         return taskItems
            .Select(item => item.GetMetadata("FullPath"));
      }

      public static bool ValidItem(string fullPath)
      {
         return !string.IsNullOrEmpty(fullPath) &&
            ProjectHelper.IsSupportedProject(fullPath) &&
            !ProjectHelper.HasNuspecFile(fullPath);
      }

      public static IEnumerable<string> ValidItem(this IEnumerable<string> items)
      {
         return items
            .Where(fullPath => ValidItem(fullPath));
      }

      public static bool NuspecItem(ITaskItem item)
      {
         return NuspecItem(item.GetMetadata("FullPath"));
      }

      public static bool NuspecItem(string fullPath)
      {
         return HasNuspecFile(fullPath);
      }

      public static IEnumerable<string> NuspecItem(this IEnumerable<string> items)
      {
         return items
            .Where(fullPath => NuspecItem(fullPath));
      }

      public static IEnumerable<string> UnappliedItem(this IEnumerable<string> items, HashSet<string> alreadyAppliedProjects)
      {
         return items
            .Where(fullPath => !alreadyAppliedProjects.Contains(fullPath));
      }
      
      public static bool IsSupportedProject(string projectFile)
      {
         var extension = Path.GetExtension(projectFile);
         return supportedProjectExtensions.Contains(extension);
      }

      public static IEnumerable<string> GetDefaultReferences(string projectFile)
      {
         if (projectFile.EndsWith(".csproj"))
            return csDefaultReferences;
         else if (projectFile.EndsWith(".vbproj"))
            return vbDefaultReferences;
         else if (projectFile.EndsWith(".fsproj"))
            return fsDefaultReferences;
         else
            return Enumerable.Empty<string>();
      }

      public static string GetSolutionDir(string projectDirectory)
      {
         var path = projectDirectory;

         // Only look 4 folders up to find the solution directory
         const int maxDepth = 5;
         int depth = 0;
         do
         {
            if (SolutionFileExists(path))
               return path;

            path = Path.GetDirectoryName(path);

            depth++;
            //When you get to c:\, the parent path is null.
         } while (depth < maxDepth && path != null);

         return null;
      }

      private static bool SolutionFileExists(string path)
      {
         return Directory.GetFiles(path, "*.sln").Any();
      }

      private static bool HasNuspecFile(string projectFile)
      {
         return Directory
            .EnumerateFiles(
               Path.GetDirectoryName(projectFile),
               "*" + Constants.ManifestExtension,
               SearchOption.AllDirectories)
            .Any();
      }
   }
}
