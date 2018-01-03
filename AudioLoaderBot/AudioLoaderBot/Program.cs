using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
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
				string message = e.Message.Text;
				if (e.Message.Text == "/start")
				{
					Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Send me link to youtube video and I'll give you an audio... Files over 50 MB are not processed yet... :)");
				}
				else if (e.Message.Text.Contains("www.youtube.com") || e.Message.Text.Contains("youtu.be"))
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
			string videoid = string.Empty;
			if (URL.Contains("youtube.com"))
			{
				videoid = URL.Substring(URL.LastIndexOf("=") + 1);
			}
			else if (URL.Contains("youtu.be"))
			{
				videoid = URL.Substring(URL.LastIndexOf("/") + 1);
			}
			try
			{
				var videoInfos = await DownloadUrlResolver.GetVideoUrlsAsync(videoid, video => video != null, false);
				var videoWithAudio =
					videoInfos.FirstOrDefault(video => video.AudioBitrate <= 256 && video.VideoType != VideoType.WebM);

				if (videoWithAudio != null)
				{
					using (var client = new WebClient())
					{
						await client.DownloadFileTaskAsync(new Uri(videoWithAudio.DownloadUrl), $"./audio/{chatId}.mp3");
						FileToSend file = new FileToSend(videoWithAudio.Title, new MemoryStream(System.IO.File.ReadAllBytes($"./audio/{chatId}.mp3")));
						await Bot.SendAudioAsync(chatId, file, videoWithAudio.Title, 0, string.Empty, string.Empty);
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
