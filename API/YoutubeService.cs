using Spectre.Console;
using Swan;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Spotiloader.API;

public enum DownloadStatus
{
    Downloaded,
    NotFound,
    Failed
}

public class YoutubeService
{
    private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory + "SpotiloaderDownloads\\";
    private readonly YoutubeClient _client = new();
    
    public string GetDownloadPath => BasePath;

    public YoutubeService()
    {
        if (!Directory.Exists(BasePath))
        {
            Directory.CreateDirectory(BasePath);
        }
    }
    
    public async Task<DownloadStatus> DownloadSongByName(string name)
    {
        var batchNumber = 0;
        while (batchNumber == 0)
        {
            await foreach (var batch in _client.Search.GetResultBatchesAsync(name))
            {
                foreach (var result in batch.Items)
                {
                    switch (result)
                    {
                        case VideoSearchResult video:
                        {
                            try
                            {
                                AnsiConsole.WriteLine();
                                AnsiConsole.MarkupLineInterpolated($"[green bold]Downloading song: {video.Title}[/]");
                                AnsiConsole.WriteLine();

                                var manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);
            
                                var streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                                var sanitizedTitle = SanitizeTitle(video.Title);
                                
                                await _client.Videos.Streams.DownloadAsync(streamInfo, $@"{BasePath}{sanitizedTitle}.mp3");

                                return DownloadStatus.Downloaded;
                            }
                            catch (Exception e)
                            {
                                AnsiConsole.MarkupLine($"[red bold]{e.Humanize()}[/]");
                                AnsiConsole.MarkupLine(e.Message);
                                return DownloadStatus.Failed;
                            }
                        }
                        default:
                            return DownloadStatus.NotFound;
                    }
                
                }

                batchNumber++;
            }
        }
        return DownloadStatus.NotFound;
    }

    private static string SanitizeTitle(string videoTitle)
    {
        var charsToEvade = new[]
        {
            '#', '<', '>', '$', '+', '%', '!', '`', '&', '*', '\'', '|', '{', '}', '?', '"', '=', 
            '/', ':', '\\', '@'
        };

        return charsToEvade.Aggregate(videoTitle, (current, character) => current.Replace(character.ToString(), string.Empty));
    }
}