using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
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
