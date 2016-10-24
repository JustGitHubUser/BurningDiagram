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
                string sourceFile = null;
                string targetFile = null;
                string targetXml = null;
                var argsParser = new ArgsParser(Path.GetFileName(Environment.GetCommandLineArgs()[0]), "<source file>");
                argsParser.AddOption(new ArgOption("?", true, v => showHelp = true, "print this help"));
                argsParser.AddOption(new ArgOption("o|output=", false, v => targetFile = v, "image output path"));
                argsParser.AddOption(new ArgOption("x|xml=", false, v => targetXml = v, "xml output path"));
                argsParser.AddTarget(new ArgTarget(false, s => { sourceFile = s; return true; }));
                bool showError = !argsParser.Parse(args, true);
                if(showError || showHelp) {
                    if(showError)
                        Console.WriteLine("The syntax of the command is incorrect.");
                    Console.Write(argsParser.GetUsage());
                    if(showError)
                        Environment.Exit(1);
                } else {
                    BurningDiagramTool.Run(sourceFile, targetXml, targetFile);
                }
            } catch(Exception e) {
                Console.Write(e.ToStringEx());
                Environment.Exit(1);
            }
        }
    }
}
