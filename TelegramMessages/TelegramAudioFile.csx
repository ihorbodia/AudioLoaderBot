using Telegram.Bot.Types;

public class TelegramAudioFile
{
	public FileToSend TelegramFile { get; set; }
	public string Caption { get; set; }
	public string Performer{ get; set; }
	public string Title { get; set; }
	public double Duration { get; set; }
}
