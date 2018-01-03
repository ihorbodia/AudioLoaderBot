using System;
using Telegram.Bot;

namespace AudioLoaderBot
{
    class Program
    {
		public static TelegramBotClient Bot = new TelegramBotClient("517779099:AAEO5Rv31rz-79Nt1Pqy3i1D_LgSVXEg0oI");
		static void Main(string[] args)
        {
			Bot.OnMessage += Bot_OnMessage;

			Bot.StartReceiving();
			Console.ReadKey();
			Bot.StopReceiving();
		}

		private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			AudioSender audioSender = new AudioSender(Bot);
			audioSender.BotOnMessage(sender, e);
		}
	}
}
