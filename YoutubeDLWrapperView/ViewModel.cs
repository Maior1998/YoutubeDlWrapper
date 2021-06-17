using DevExpress.Mvvm;

using Microsoft.Win32;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using YoutubeDlWrapper;

using static YoutubeDlWrapper.Wrapper;

namespace YoutubeDLWrapperView
{
    public class ViewModel : ReactiveObject
    {

        public ViewModel()
        {
            Wrapper.EnsureYouTubeDlInstalledAndUpdated();
        }

        [Reactive] public YoutubeDlVideo Video { get; set; }
        [Reactive] public string YoutubeVideoUrl { get; set; }
        [Reactive] public YoutubeDlVideoQuality SelectedQuality { get; set; }


        private AsyncCommand searchUrl;
        public AsyncCommand SearchUrl => searchUrl ??= new(async () =>
        {
            Video = null;
            YoutubeDlVideo bufferVideo = await GetVideoInfoAsync(YoutubeVideoUrl);
            Video = bufferVideo with { Qualities = bufferVideo.Qualities.Where(x => x.Extension != "webm") };
            SelectedQuality = Video.Qualities.FirstOrDefault();
        }, () => !string.IsNullOrWhiteSpace(YoutubeVideoUrl));

        private AsyncCommand downloadVideo;

        private static readonly Regex BannedSymbolsRegex = new("[:\"]");
        public AsyncCommand DownloadVideo => downloadVideo ??= new(async () =>
        {
            SaveFileDialog saveFileDialog = null;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                saveFileDialog = new()
                {
                    AddExtension = true,
                    DefaultExt = SelectedQuality.Extension,
                    FileName = $"{BannedSymbolsRegex.Replace(Video.Name.Truncate(100), "_")}.{SelectedQuality.Extension}",
                    Filter = $"Video with same format|*.{SelectedQuality.Extension}|All files|*.*"
                };
            });

            if (!saveFileDialog.ShowDialog().Value) return;
            await SaveVideo(SelectedQuality.Id, saveFileDialog.FileName, YoutubeVideoUrl);
            Video = null;
        }, () => SelectedQuality != null && !string.IsNullOrWhiteSpace(YoutubeVideoUrl));


    }
}
