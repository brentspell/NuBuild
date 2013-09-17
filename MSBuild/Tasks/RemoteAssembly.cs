//===========================================================================
// MODULE:  NuPackage.cs
// PURPOSE: NuBuild package MSBuild task
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
using System.IO;
using System.Reflection;
// Project References
using NuGet.Runtime;

namespace NuBuild.MSBuild
{
   internal class RemoteAssembly : MarshalByRefObject
   {
      private Assembly assembly;

      internal void ReflectionOnlyLoadFrom(String assemblyFile)
      {
         assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
      }

      internal T GetPropertyValue<T>(Func<Assembly, T> getter)
      {
         return getter(assembly);
      }

      internal T GetCustomAttribute<T>(Func<CustomAttributeData, Boolean> predicate, Func<CustomAttributeData, T> selector)
      {
         return CustomAttributeData.GetCustomAttributes(assembly).Where(predicate).Select(selector).FirstOrDefault();
      }

      internal IList<Object> GetCustomAttributes(Func<CustomAttributeData, Boolean> predicate, Func<CustomAttributeData, Object> selector)
      {
         return CustomAttributeData.GetCustomAttributes(assembly).Where(predicate).Select(selector).ToList();
      }
   }

   public class RemoteAssemblyProxy
   {
      private AppDomain remoteDomain;
      private RemoteAssembly remoteAssembly;

      public RemoteAssemblyProxy(AppDomain remoteDomain)
      {
         this.remoteDomain = remoteDomain;
         this.remoteAssembly = remoteDomain.CreateInstance<RemoteAssembly>();
      }

      public static AppDomain CreateDomain(String friendlyName)
      {
         var setup = new AppDomainSetup()
         {
            ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            LoaderOptimization = LoaderOptimization.MultiDomainHost,
         };
         return AppDomain.CreateDomain(friendlyName, null, setup);
      }

      public static void UnloadDomain(AppDomain remoteDomain)
      {
         AppDomain.Unload(remoteDomain);
      }

      public static T ExecuteGetter<T>(String friendlyName, String assemblyFile, Func<RemoteAssemblyProxy, T> getter)
      {
         var appDomain = (AppDomain)null;
         try
         {
            appDomain = RemoteAssemblyProxy.CreateDomain(friendlyName);
            var assembly = new RemoteAssemblyProxy(appDomain);
            assembly.ReflectionOnlyLoadFrom(assemblyFile);
            return getter(assembly);
         }
         finally
         {
            if (appDomain != null)
               RemoteAssemblyProxy.UnloadDomain(appDomain);
         }
      }

      public T GetPropertyValue<T>(Func<Assembly, T> getter)
      {
         return remoteAssembly.GetPropertyValue<T>(getter);
      }
      
      public void ReflectionOnlyLoadFrom(String assemblyFile)
      {
         remoteAssembly.ReflectionOnlyLoadFrom(assemblyFile);
      }

      public T GetCustomAttribute<T>(Func<CustomAttributeData, Boolean> predicate, Func<CustomAttributeData, T> selector)
      {
         return remoteAssembly.GetCustomAttribute<T>(predicate, selector);
      }

      public IList<Object> GetCustomAttributes(Func<CustomAttributeData, Boolean> predicate, Func<CustomAttributeData, Object> selector)
      {
         return remoteAssembly.GetCustomAttributes(predicate, selector);
      }
   }
}
