using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Disable_SetACL {
    class Program {
        static void Main(string[] args) {
            bool debugMode = false, showHelp = false;
            string projFilename = "";

            // the debug arg sets the debug mode, any other arg is the proj filename 
            // if there is no proj filename then we process ALL csproj files in the current dir
            foreach (string arg in args) {
                switch (arg.ToLower()) {
                    case "debug": debugMode = true; break;
                    case "help":
                    case "-h":
                    case "?": showHelp = true; break;
                    default: projFilename = arg; break;
                }
            }

            if (showHelp) {
                Console.WriteLine("DisableSetACL.exe processes Visual Studio .csproj files in order to add the IncludeSetACLProviderOnDestination=false node to every appropriate PropertyGroup node.\r\n");
                Console.WriteLine("Usage: DisableSetACL.exe [yourproject.csproj]\r\n");
                Console.WriteLine("If there are no command line params then every .csproj file in the current directory is processed.");
                Console.WriteLine("Any other param will be assumed to be a file path to a specific .csproj file to be processed.");
                if (debugMode) Console.ReadKey();
                return;
            }

            if (!string.IsNullOrWhiteSpace(projFilename)) {
                Process(new FileInfo(projFilename));

            } else {
                FileInfo[] projFiles = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.csproj");
                if (projFiles.Count() == 0) {
                    Console.WriteLine($"No .csproj files found in {Directory.GetCurrentDirectory()}");
                }

                foreach (FileInfo csProj2 in projFiles) {
                    Process(csProj2);
                }
            }

            if (debugMode) Console.ReadKey();
        }

        static void Process(FileInfo csProj) {
            if (!csProj.Exists) {
                Console.WriteLine($"File [{csProj.FullName}] not found.\r\n");
                return;
            }

            XElement csProjXml = XElement.Load(csProj.FullName);
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";

            Console.WriteLine($"Processing {csProj.Name}");

            int n = 0;
            foreach (XElement propertyGroup in csProjXml.Elements(xmlns + "PropertyGroup")) {
                XAttribute condition = propertyGroup.Attribute("Condition");

                //Console.WriteLine(propertyGroup.Name);
                //Console.WriteLine(propertyGroup.Attribute("Condition"));

                if (condition != null && condition.Value.Contains("AnyCPU")) {
                    //Console.WriteLine("OK");

                    // look for exsisting <IncludeSetACLProviderOnDestination> node
                    XElement includeSetACL = propertyGroup.Element(xmlns + "IncludeSetACLProviderOnDestination");

                    if (includeSetACL == null) {
                        includeSetACL = new XElement(xmlns + "IncludeSetACLProviderOnDestination");
                        includeSetACL.Value = "False";
                        propertyGroup.Add(includeSetACL);

                    } else {
                        includeSetACL.Value = "False";
                    }

                    n++;
                }
            }

            csProjXml.Save(csProj.FullName);

            Console.WriteLine($"Updated {n} PropertyGroup nodes.\r\n");
        }
    }
}
