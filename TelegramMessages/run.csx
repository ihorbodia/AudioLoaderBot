#load "TelegramAudioFile.csx"
#load "AudioSender.csx"

using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;

public static TelegramBotClient TelegramBot = new TelegramBotClient("");
public static async Task Run(HttpRequestMessage req, TraceWriter log)
{
	var content = req.Content;
    string jsonContent = content.ReadAsStringAsync().Result;
    Update update = JsonConvert.DeserializeObject<Update>(jsonContent);

    if (update.Type != UpdateType.MessageUpdate)
	{
		await TelegramBot.SendTextMessageAsync(update.Message.Chat.Id, $"Command wrong...");
		return;
	}

	string inputMessageText = update.Message.Text;
	long chatId = update.Message.Chat.Id;

	if (inputMessageText == "/start")
	{
		await TelegramBot.SendTextMessageAsync(chatId, $"Send me link to youtube video and I'll give you an audio... Files over 50 MB are not processed yet... :)");
	}
	else if (inputMessageText.Contains("www.youtube.com") || inputMessageText.Contains("youtu.be"))
	{
		AudioExtractor audioExtractor = new AudioExtractor(inputMessageText);
		TelegramAudioFile audioFile = await audioExtractor.GetAudio();
		if (audioFile != null)
		{
			await TelegramBot.SendAudioAsync(chatId, audioFile.TelegramFile, audioFile.Title, 0, audioFile.Performer, audioFile.Title);
		}
		else
		{
			await TelegramBot.SendTextMessageAsync(chatId, audioExtractor.ErrorMessage);
		}
	}
	else
	{
		await TelegramBot.SendTextMessageAsync(chatId, $"Command wrong...");
	}
}
