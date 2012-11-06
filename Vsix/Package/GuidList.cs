using System;

namespace NuBuild.Vsix
{
   static class GuidList
   {
      public const String guidPackagePkgString = "d2ab0959-d15b-4e44-8a06-c999cc386e34";
      public const String guidPackageCmdSetString = "6789e054-ba2b-4e2c-ba46-2439263fc576";
      public const String guidProjectFactoryString = "e09dd79a-4488-4ab9-8d3f-a7eee78bf432";

      public static readonly Guid guidPackageCmdSet = new Guid(guidPackageCmdSetString);
      public static readonly Guid guidProjectFactory = new Guid(guidProjectFactoryString);
   }
}
