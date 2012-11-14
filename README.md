![NuBuild](http://raw.github.com/bspell1/NuBuild/master/NuBuild.png) NuBuild
=========================================================================
A NuGet project system for Visual Studio

Download the latest [version](http://github.com/downloads/bspell1/NuBuild/NuBuild.msi) and check it out on the [gallery](http://visualstudiogallery.msdn.microsoft.com/3efbfdea-7d51-4d45-a954-74a2df51c5d0).

![New Project](https://raw.github.com/wiki/bspell1/nubuild/newproj.png)

##Features:##
* Creates a new project type (.nuproj) within Visual Studio for NuGet packages, so you can manage and build your packages right along with your source projects
* Includes project references automatically within the NuGet package, so you don't have to place them explicitly in the .nuspec file
* Builds incrementally, so the NuGet package is generated only if one of its references changes
* Supports generating package build numbers automatically, from a DLL included in the package, from the TFS build system, or from a build number file
