using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NuBuild.MSBuild
{
    public sealed class NuProjectResultResolver : Task
    {
        private List<ITaskItem> lstOutputs = new List<ITaskItem>();

        [Output]
        public ITaskItem[] TargetOutputs { get; set; }

        [Required]
        public ITaskItem[] Projects { get; set; }

        [Required]
        public string Configuration { get; set; }
        [Required]
        public string Platform { get; set; }

        public override bool Execute()
        {
            if (Projects == null)
            {
                TargetOutputs = new ITaskItem[0];
                return false;
            }

            foreach (ITaskItem project in Projects)
            {
                string compiledFile = GetCompiledFile(project.ItemSpec, Configuration, Platform);
                lstOutputs.Add(new TaskItem(compiledFile));
            }

            TargetOutputs = lstOutputs.ToArray();
            return true;
        }

        private string GetCompiledFile(string prjFile, string configuration, string platform)
        {
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";
            XDocument doc = XDocument.Load(prjFile);
            //OutputType            
            string outputType = doc.Root.Descendants(ns + "OutputType").First().Value;
            //AssemblyName
            string assemblyName = doc.Root.Descendants(ns + "AssemblyName").First().Value;
            //OutputPath
            string outputPath = string.Empty;
            foreach (var item in doc.Root.Elements(ns + "PropertyGroup"))
            {
                if (item.HasAttributes && !string.IsNullOrWhiteSpace(item.Attribute("Condition").Value))
                {
                    string conditionAttribute = item.Attribute("Condition").Value;
                    if (conditionAttribute.Contains(configuration + @"|" + platform))
                    {
                        outputPath = item.Element(ns + "OutputPath").Value;
                        break;
                    }
                }
            }

            string fileExt = GetOutputTypeExtension(outputType);

            string compiledProject = Path.Combine(Path.GetDirectoryName(prjFile), outputPath, assemblyName + fileExt);


            return compiledProject;
        }

        private string GetOutputTypeExtension(string outputType)
        {
            if (outputType.ToUpper() == @"Library".ToUpper())
                return ".dll";
            if (outputType.ToUpper() == @"Exe".ToUpper())
                return ".exe";
            if (outputType.ToUpper() == @"WinExe".ToUpper())
                return ".exe";

            return string.Empty;
        }
    }
}
