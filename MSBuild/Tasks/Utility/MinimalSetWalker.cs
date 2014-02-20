//===========================================================================
// MODULE:  MinimalSetWalker.cs
// PURPOSE: Determine minimal required dependency
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
using System.Runtime.Versioning;
using NuGet;
// Project References

namespace NuBuild.MSBuild
{
   public class MinimalSetWalker : PackageWalker
   {
      private readonly List<IPackage> packages;
      private readonly IPackageRepository repository;

      private MinimalSetWalker(List<IPackage> packages, FrameworkName targetFramework) :
         base(targetFramework)
      {
         this.packages = packages;
         this.repository = new ReadOnlyPackageRepository(packages.ToList());
      }

      public static IEnumerable<IPackage> GetMinimalSet(List<IPackage> packages, FrameworkName targetFramework)
      {
         return new MinimalSetWalker(packages, targetFramework).GetMinimalSet();
      }

      protected override bool SkipDependencyResolveError
      {
         get
         {
            // For the pack command, when don't need to throw if a dependency is missing 
            // from a nuspec file.
            return true;
         }
      }

      protected override IPackage ResolveDependency(PackageDependency dependency)
      {
         return repository.ResolveDependency(dependency, allowPrereleaseVersions: false, preferListedPackages: false);
      }

      protected override bool OnAfterResolveDependency(IPackage package, IPackage dependency)
      {
         packages.Remove(dependency);
         return base.OnAfterResolveDependency(package, dependency);
      }

      private IEnumerable<IPackage> GetMinimalSet()
      {
         foreach (var package in repository.GetPackages())
            Walk(package);
         return packages;
      }
   }
}
