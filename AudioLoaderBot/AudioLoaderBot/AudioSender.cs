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
    public class AudioSender
    {
		TelegramBotClient _bot;
		public AudioSender(TelegramBotClient bot)
		{
			_bot = bot;
		}

		public void BotOnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
			{
				string message = e.Message.Text;
				if (e.Message.Text == "/start")
				{
					_bot.SendTextMessageAsync(e.Message.Chat.Id, $"Send me link to youtube video and I'll give you an audio... Files over 50 MB are not processed yet... :)");
				}
				else if (e.Message.Text.Contains("www.youtube.com") || e.Message.Text.Contains("youtu.be"))
				{
					GetAudio(e.Message.Text, e.Message.Chat.Id).GetAwaiter().GetResult();
				}
				else
				{
					_bot.SendTextMessageAsync(e.Message.Chat.Id, $"Command wrong...");
				}
			}
		}

		private async Task GetAudio(string URL, long chatId)
		{
			try
			{
				var videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync(URL, false);
				var videoWithAudio =
					videoInfos.FirstOrDefault(video => video.AudioBitrate <= 256 && video.VideoType != VideoType.WebM && video.Resolution <= 480);
					
				if (videoWithAudio != null)
				{				
					if (videoWithAudio.RequiresDecryption)
					{
						await DownloadUrlResolver.DecryptDownloadUrl(videoWithAudio);
					}
					await _bot.SendTextMessageAsync(chatId, "It seems OK, processing...");
					using (var client = new WebClient())
					{
						byte[] data = await client.DownloadDataTaskAsync(new Uri(videoWithAudio.DownloadUrl));
						MemoryStream stream = new MemoryStream(data);
						FileToSend file = new FileToSend("Audiofile", new MemoryStream(data));
						if (data.Count() > 50000000)
						{
							await _bot.SendTextMessageAsync(chatId, "File is too large...");
							stream.Close();
							data = null;
						}
						else
						{
							await _bot.SendAudioAsync(chatId, file, videoWithAudio.Title, 0, GetAutor(videoWithAudio.Title), GetTitle(videoWithAudio.Title));
						}
					}

					GC.Collect();
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

		private string GetAutor(string caption)
		{
			string result = string.Empty;
			int length = caption.IndexOf("-");
			if (length > 0)
			{
				result = caption.Substring(0, length);
			}
			else
			{
				result = caption;
			}
			return result;
		}

		private string GetTitle(string caption)
		{
			string result = string.Empty;
			int length = caption.IndexOf("-");
			if (length > 0)
			{
				result = caption.Substring(caption.IndexOf("-") + 2);
			}
			else
			{
				result = string.Empty;
			}
			return result;
		}
	}
}
