using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Helpers;

namespace BurningDiagram {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            try {
                bool showHelp = false;
                string targetXml = null;
                var argsParser = new ArgsParser(Path.GetFileName(Environment.GetCommandLineArgs()[0]), "");
                argsParser.AddOption(new ArgOption("?", true, v => showHelp = true, "print this help"));
                argsParser.AddOption(new ArgOption("x|xml=", false, v => targetXml = v, "save xml to specified file"));
                bool showError = !argsParser.Parse(args, true);
                if(showError || showHelp) {
                    if(showError)
                        Console.Error.WriteLine("The syntax of the command is incorrect.");
                    Console.Write(argsParser.GetUsage());
                    if(showError)
                        Environment.Exit(1);
                } else {
                    BurningDiagramTool.Run(targetXml);
                }
            } catch(Exception e) {
                Console.Error.Write(e.ToStringEx());
                Environment.Exit(1);
            }
        }
    }
}
