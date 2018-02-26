using System;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace AudioLoaderBot
{
    class Program
    {
		public static TelegramBotClient TelegramBot = new TelegramBotClient("521386242:AAF2dw49hZyNqME_-aKCT7eUVEHNtscvLhk");
		static void Main(string[] args)
		{
			TelegramBot.OnMessage += Bot_OnMessage;
			TelegramBot.StartReceiving();
			Console.ReadKey();
			TelegramBot.StopReceiving();
		}

		private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			if (e.Message.Type != MessageType.TextMessage)
			{
				await TelegramBot.SendTextMessageAsync(e.Message.Chat.Id, $"Command wrong...");
				return;
			}

			string inputMessageText = e.Message.Text;
			long chatId = e.Message.Chat.Id;

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
	}
}
