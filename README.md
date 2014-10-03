![NuBuild](https://raw.githubusercontent.com/bspell1/NuBuild/master/NuBuild.png) NuBuild
=========================================================================
A NuGet project system for Visual Studio

Download the latest [version](http://brentspell.com/download/NuBuild.msi) and check it out on the [gallery](http://visualstudiogallery.msdn.microsoft.com/3efbfdea-7d51-4d45-a954-74a2df51c5d0).

##Features##
* Creates a new project type (.nuproj) in Visual Studio, so you can manage your NuGet packages right along with your other projects
* Adds project reference DLLs automatically to the NuGet package, so you don't have to specify them explicitly in the .nuspec file
* Builds incrementally, so the NuGet package is generated only if one of its dependencies changes
* Generates package version numbers automatically from one of several sources

##Getting Started##
![New Project](https://raw.githubusercontent.com/bspell1/NuBuild/master/NewProj.png)

Once you have installed NuGet and NuBuild, simply click **File**->**New Project**. Under **Installed Templates**, choose the **NuGet** category and the **NuGet Package** project type. This will add a NuBuild project to your solution.

![Project Properties](https://raw.githubusercontent.com/bspell1/NuBuild/master/ProjProp.png)

Right-click the project and select **Properties** to change the build configuration for your NuBuild project. Then, either edit the .nuspec file directly or use the excellent [NuGet Package Explorer](http://npe.codeplex.com/) to configure your package's properties.

##Versioning##
NuBuild supports the following options for generating NuGet package version numbers.

* **Manual:** In this mode, NuBuild will use the version number specified in the .nuspec file for the package version. It will not attempt to generate it from any other source. Use this option if you manage your version numbers by hand or via a text replacement mechanism (as in TeamCity).
* **Library:** (default) NuGet will assign the package version from the first library it finds that contains a version resource. The library must either be a project reference or be specified explicitly within the .nuspec &lt;files&gt; section. Use this option if you automatically version the DLLs in your solution.
* **Auto:** In this mode, the major/minor version numbers are specified manually in the .nuspec file, but NuBuild generates the build number. If the project includes a **$(BuildNumber)** property (as in TFS builds), it will be used as the build number for the package version. Otherwise, NuBuild will create a build.number file and automatically increment it each time the project builds.

##Replacement Tokens##
NuBuild also supports the NuGet replacement tokens defined at http://docs.nuget.org/docs/reference/nuspec-reference#Replacement_Tokens. If used, the **version** token behaves according to specification regardless of the versioning mode of the project.

##Samples##
The project includes samples for all of the valid project configurations.

* **Simple:** the simplest configuration - a single library project reference and a manually-maintained version number
* **ReferenceVersion:** in this sample, the NuBuild project references a library project, automatically including its targets in the NuGet package and generating the version number from the DLL
* **LibraryVersion:** this package references a single library within the &lt;files&gt; section of the .nuspec file and generates its version number from the DLL
* **TfsVersion:** generates the version number from the TFS $(BuildNumber) property
* **AutoVersion:** generates the version number automatically from an auto-incremented build.number file
* **MultiPackage:** this sample generates multiple NuGet packages from a single NuBuild project

##Miscellaneous##
* If you want to compile/extend NuBuild yourself, you will need to install the Visual Studio 2010 SP1 SDK and WiX 3.6+. The project consists of a Visual Studio package (Vsix\Package), a MSBuild task library (MSBuild\Tasks), and a WiX installer (Install).
