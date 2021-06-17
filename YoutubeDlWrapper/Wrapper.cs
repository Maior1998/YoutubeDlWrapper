using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace YoutubeDlWrapper
{
    public static class Wrapper
    {
        private static string UnixYoutubeDlFileName = "youtube-dl";
        private static string WindowsYoutubeDlFileName = UnixYoutubeDlFileName + ".exe";

        private static string CurrentOsFileName;


        private static string UnixYoutubeDlDownloadLink = @"https://yt-dl.org/downloads/latest/youtube-dl";
        private static string WindowsYoutubeDlDownloadLink = @"https://yt-dl.org/latest/youtube-dl.exe";

        private static string CurrentOsYoutubeDlDownloadLink;

        static Wrapper()
        {
            PlatformID version = Environment.OSVersion.Platform;
            if (version == PlatformID.Unix)
            {
                CurrentOsFileName = UnixYoutubeDlFileName;
                CurrentOsYoutubeDlDownloadLink = UnixYoutubeDlDownloadLink;
            }
            else if (version == PlatformID.Win32NT)
            {
                CurrentOsFileName = WindowsYoutubeDlFileName;
                CurrentOsYoutubeDlDownloadLink = WindowsYoutubeDlDownloadLink;
            }
            else
            {
                throw new("Unsupported platform");
            }
        }

        public static async void EnsureYouTubeDlInstalledAndUpdated()
        {

            await Update();
        }

        public class OnUpdateCheckingEventArgs
        {
            public bool Cancel;
        }


        private static ProgramStartHelper getHelper()
        {
            ProgramStartHelper updater = new(CurrentOsFileName);
            updater.Args.Add(new("--encoding", "utf-8"));
            updater.Args.Add(new("-4"));
            return updater;
        }

        public record YoutubeDlVideo
        {
            public YoutubeDlVideo(string url, string name, IEnumerable<YoutubeDlVideoQuality> qualities)
            {
                Url = url;
                Name = name;
                Qualities = qualities;
            }
            public string Url { get; init; }
            public string Name { get; init; }
            public IEnumerable<YoutubeDlVideoQuality> Qualities { get; init; }
        }

        public static async Task<YoutubeDlVideo> GetVideoInfoAsync(string url)
        {
            Task<IEnumerable<YoutubeDlVideoQuality>> qualitiesTask = GetVideoQualities(url);
            Task<string> videoNameTask = GetVideoNameAsync(url);

            IEnumerable<YoutubeDlVideoQuality> qualities = await qualitiesTask;
            string videoName = await videoNameTask;

            return new YoutubeDlVideo(url, videoName, qualities);
        }

        private static readonly Regex QualityRegex = new(@"^(?<number>\d+)\s+(?<extension>[a-z0-9]+)\s+(?<resolution>audio only|\d+x\d+)\s+(?<resolutionname>tiny|\d+p(\d*))\s+.+?(?<size>\(best\)|\d+\.\d+\w+)$");
        private static async Task<IEnumerable<YoutubeDlVideoQuality>> GetVideoQualities(string url)
        {
            ProgramStartHelper updater = getHelper();
            updater.Args.Add(new("-F"));
            updater.Args.Add(new(url));
            IEnumerable<string> output = await updater.StartAndGetOutputAsync();
            List<YoutubeDlVideoQuality> result = new();
            foreach (string row in output)
            {
                Match match = QualityRegex.Match(row);
                if (!match.Success) continue;
                YoutubeDlVideoQuality quality = new()
                {
                    Id = match.Groups["number"].Value,
                    Extension = match.Groups["extension"].Value,
                    Resolution = match.Groups["resolution"].Value,
                    DimensionName = match.Groups["resolutionname"].Value,
                    Size = match.Groups["size"].Value,

                };
                result.Add(quality);
            }
            Console.WriteLine();
            return result;
        }
        public static async Task SaveVideo(string formatCode, string filePath, string url)
        {
            ProgramStartHelper helper = getHelper();
            helper.Args.Add(new("-f", formatCode));
            helper.Args.Add(new("-v"));
            helper.Args.Add(new("-o", filePath) { IsNeedSurroundWithQuotes = true });
            helper.Args.Add(new(url));
            helper.DataReceivedEventHandler = (arg1, arg2) =>
            {
                Debug.WriteLine(arg2.Data);
            };
            ProgramStartHelper.useSpecial = true;
            await helper.Start();
            ProgramStartHelper.useSpecial = false;
            helper = new("explorer.exe");
            helper.Args.Add(new("/select,", filePath) { IsNeedSurroundWithQuotes = true });
            await helper.Start();
        }

        private static async Task<string> GetVideoNameAsync(string url)
        {
            ProgramStartHelper updater = getHelper();
            updater.Args.Add(new("-e"));
            updater.Args.Add(new(url));
            IEnumerable<string> output = await updater.StartAndGetOutputAsync();
            return output.FirstOrDefault();
        }

        public static async Task Update()
        {
            if (File.Exists(CurrentOsFileName))
            {
                ProgramStartHelper updater = new(CurrentOsFileName);
                updater.Args.Add(new("-U", ""));
                await updater.Start();
            }
            else
            {
                WebClient client = new WebClient();
                await client.DownloadFileTaskAsync(new Uri(CurrentOsYoutubeDlDownloadLink), CurrentOsFileName);
            }
        }


    }
    public record YoutubeDlVideoQuality
    {
        public string Id { get; set; }
        public string Extension { get; set; }
        public string Resolution { get; set; }
        public string DimensionName { get; set; }
        public string Size { get; set; }
    }
}
