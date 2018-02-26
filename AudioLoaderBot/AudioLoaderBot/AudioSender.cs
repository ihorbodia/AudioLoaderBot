using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using YoutubeExtractor;

namespace AudioLoaderBot
{
	public class AudioExtractor
	{
		List<VideoInfo> videoInfos;
		VideoInfo videoInfoFile;
		const int TelegramFileMaximumLengthSize = 50000000;

		public string ErrorMessage { get; set; }

		public AudioExtractor(string YoutubeURL)
		{
			videoInfos = DownloadUrlResolver.GetDownloadUrls(YoutubeURL, true).ToList();

		}

		private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			if (e.BytesReceived > TelegramFileMaximumLengthSize)
			{
				(sender as WebClient).CancelAsync();
				ErrorMessage = "File is too large";
			}
		}

		public async Task<TelegramAudioFile> GetAudio()
		{
			TelegramAudioFile result = null;
			try
			{
				videoInfoFile =
				videoInfos.OrderByDescending(info => info.AudioBitrate)
						.FirstOrDefault();

				if (videoInfoFile == null)
				{
					ErrorMessage = "Video with audio not found";
					return null;
				}

				if (videoInfoFile.RequiresDecryption)
				{
					DownloadUrlResolver.DecryptDownloadUrl(videoInfoFile);
				}

				using (var client = new WebClient())
				{
					client.DownloadProgressChanged += WebClient_DownloadProgressChanged;
					byte[] data = await client.DownloadDataTaskAsync(new Uri(videoInfoFile.DownloadUrl));

					result = new TelegramAudioFile()
					{
						TelegramFile = new FileToSend($"{videoInfoFile.Title}.mp3", new MemoryStream(data)),
						Caption = videoInfoFile.Title,
						Performer = ExtractAutor(videoInfoFile.Title),
						Title = ExtractTitle(videoInfoFile.Title)
					};
				}
				return result;
			}
			catch (YoutubeParseException)
			{
				ErrorMessage = "Error while trying to parse youtube data";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		private string ExtractAutor(string caption)
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

		private string ExtractTitle(string caption)
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
