using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Helpers;

namespace TrelloClient {
    class Program {
        static void Main(string[] args) {
            try {
                bool showHelp = false;
                string appKey = null;
                string userToken = null;
                string commandName = null;
                var commandParameters = new List<string>();
                var argsParser = new ArgsParser(Path.GetFileName(Environment.GetCommandLineArgs()[0]), "<command> <command parameters>");
                argsParser.AddOption(new ArgOption("?", true, v => showHelp = true, "print this help"));
                argsParser.AddOption(new ArgOption("k|appkey=", false, v => appKey = v, "application key"));
                argsParser.AddOption(new ArgOption("u|usertoken=", false, v => userToken = v, "user token"));
                argsParser.AddTarget(new ArgTarget(false, s => { commandName = s; return true; }));
                argsParser.AddTarget(new ArgTarget(false, s => { commandParameters.Add(s); return false; }));
                bool showError = !argsParser.Parse(args, true);
                TrelloCommand? command = null;
                if(!showError && !showHelp) {
                    command = ParseCommand(commandName, commandParameters);
                    if(command == null)
                        showError = true;
                }
                if(showError || showHelp || command == null) {
                    if(showError)
                        Console.Error.WriteLine("The syntax of the command is incorrect.");
                    Console.WriteLine(argsParser.GetUsage());
                    Console.WriteLine("Known commands:");
                    Console.WriteLine("\tgbd <board name> - get burndown chart data");
                    Console.WriteLine("\tpbc <board name> [<image file>] - put burndown chart picture from specified file or standard input");
                    if(showError)
                        Environment.Exit(1);
                } else {
                    TrelloTool.Run(appKey, userToken, command.Value, commandParameters);
                }
            } catch(Exception e) {
                Console.Error.Write(e.ToStringEx());
                Environment.Exit(1);
            }
        }
        static TrelloCommand? ParseCommand(string command, IEnumerable<string> commandParameters) {
            if(string.Equals(command, "gbd", StringComparison.OrdinalIgnoreCase)) {
                if(!commandParameters.Take(1).Any() || commandParameters.Skip(1).Any()) return null;
                return TrelloCommand.GetBurndownChartData;
            }
            if(string.Equals(command, "pbc", StringComparison.OrdinalIgnoreCase)) {
                if(!commandParameters.Take(1).Any() || commandParameters.Skip(2).Any()) return null;
                return TrelloCommand.PutBurndownChart;
            }
            return null;
        }
    }
}
