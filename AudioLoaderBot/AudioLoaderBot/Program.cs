using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using YoutubeExtractorCore;

namespace AudioLoaderBot
{
    class Program
    {
		public static TelegramBotClient Bot = new TelegramBotClient("517779099:AAEyOrpWv0Y5_QB3n7wPDoovhkEzNXXBg-k");
		static void Main(string[] args)
        {
			Bot.OnMessage += Bot_OnMessage;
			Bot.OnMessageEdited += Bot_OnMessageEdited;

			Bot.StartReceiving();
			Console.ReadKey();
			Bot.StopReceiving();
		}

		private static void Bot_OnMessageEdited(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			
		}

		private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
			{
				if (e.Message.Text == "/start")
				{
					Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Send me link to youtube video and I'll give you an audio... Files over 50 MB are not processed yet... :)");
				}
				else if (e.Message.Text.Contains("www.youtube.com"))
				{
					RunExample(e.Message.Text, e.Message.Chat.Id).GetAwaiter().GetResult();
				}
				else
				{
					Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Command wrong..");
				}
			}
		}
		private static async Task RunExample(string URL, long chatId)
		{
			var id = URL.Split("=")[1];
			try
			{
				var videoInfos = await DownloadUrlResolver.GetVideoUrlsAsync(id, video => video != null, false);
				var videoWithAudio =
					videoInfos.FirstOrDefault(video => video.AudioBitrate <= 256 && video.VideoType == VideoType.Mp4);

				if (videoWithAudio != null)
				{
					using (var client = new WebClient())
					{
						byte[] contents = await client.DownloadDataTaskAsync(new Uri(videoWithAudio.DownloadUrl, UriKind.Absolute));
						await Bot.SendAudioAsync(chatId, new Telegram.Bot.Types.FileToSend(videoWithAudio.Title, new MemoryStream(contents)), videoWithAudio.Title, 0, string.Empty, string.Empty);
					}
				}
				else
				{
					Console.WriteLine("Video with audio not found");
				}
			}
			catch (YoutubeVideoNotAvailableException)
			{
				Console.WriteLine("Video is not available");
			}
			catch (YoutubeParseException)
			{
				Console.WriteLine("Error while trying to parse youtube data");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
