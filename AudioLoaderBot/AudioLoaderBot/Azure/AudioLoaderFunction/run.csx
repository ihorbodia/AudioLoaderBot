#r "..\bin\Newtonsoft.Json.dll"
#r "..\bin\Telegram.Bot.dll"

#load "AudioSender.csx"

using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;

public static async Task Run(HttpRequestMessage req, TraceWriter log)
{
    TelegramBotClient Bot = new TelegramBotClient("517779099:AAH-Wfp6V2r0o3EQTu5BmSl80ExLt2VBxlw");
    
    var content = req.Content;
    string jsonContent = content.ReadAsStringAsync().Result;
    Update update = JsonConvert.DeserializeObject<Update>(jsonContent);
    
    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.MessageUpdate)
			{
				AudioSender audioSender = new AudioSender(Bot);
				audioSender.BotOnMessage(update.Message);
			}
}
