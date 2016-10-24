using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manatee.Trello;
using Manatee.Trello.ManateeJson;
using Manatee.Trello.WebApi;

namespace TrelloClient {
    static class TrelloTool {
        public static void Run(string appKey, string userToken, TrelloCommand command, IList<string> commandParameters) {
            var serializer = new ManateeSerializer();
            TrelloConfiguration.Serializer = serializer;
            TrelloConfiguration.Deserializer = serializer;
            TrelloConfiguration.JsonFactory = new ManateeFactory();
            TrelloConfiguration.RestClientProvider = new WebApiClientProvider();
            TrelloAuthorization.Default.AppKey = appKey;
            TrelloAuthorization.Default.UserToken = userToken;
            var boardName = commandParameters[0];
            var board = Member.Me.Boards.First(x => string.Equals(x.Name, boardName, StringComparison.Ordinal));
            var card = board.Cards.First(x => string.Equals(x.Name, "BURNDOWN CHART", StringComparison.Ordinal));
            switch(command) {
            case TrelloCommand.GetBurndownChartData:
                var text = card.Comments.First().Data.Text;
                Console.Write(text);
                break;
            case TrelloCommand.PutBurndownChart:
                var file = commandParameters.Count > 1 ? commandParameters[1] : null;
                byte[] fileData;
                using(var stream = file == null ? Console.OpenStandardInput() : new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    if(stream.CanSeek) {
                        fileData = new byte[stream.Length];
                        stream.Read(fileData, 0, fileData.Length);
                    } else {
                        fileData = new byte[0];
                        var buffer = new byte[2048];
                        while(true) {
                            var readed = stream.Read(buffer, 0, buffer.Length);
                            if(readed == 0) break;
                            var newData = new byte[fileData.Length + readed];
                            fileData.CopyTo(newData, 0);
                            Array.Copy(buffer, 0, newData, fileData.Length, readed);
                            fileData = newData;
                        }
                    }
                }
                foreach(var a in card.Attachments.ToArray())
                    a.Delete();
                card.Attachments.Add(fileData, "burndown.png");
                break;
            default: throw new InvalidOperationException();
            }
        }
    }
    enum TrelloCommand {
        GetBurndownChartData,
        PutBurndownChart
    }
}
