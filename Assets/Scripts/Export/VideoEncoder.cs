﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoEncoder : MonoBehaviour
    {
        const string FfmpegMissingMessage = "Couldn't execute ffmpeg. Please install it from https://ffmpeg.org/download.html";

        public static async Task<string> ExtractAudio()
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return null;
            }

            var videoPlayer = FindObjectOfType<VideoPlayer>();
            if (!videoPlayer)
            {
                Debug.Log("Couldn't find Video Player.");
                return null;
            }

            var clip = videoPlayer.clip;
            if (!clip)
            {
                Debug.Log("No video assigned.");
                return null;
            }

            // TODO source:URL にも対応
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, clip.originalPath);
            var extension = await GetSuitableAudioExtension(path);
            var destination = Path.Combine(PathProvider.WorkDir, $"audio.{extension}");

            var startInfo = new ProcessStartInfo
            {
                Arguments = $"-i \"{path}\" -vn -acodec copy \"{destination}\"",
                FileName = "ffmpeg",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };
            var extractor = new Process { StartInfo = startInfo };
            extractor.Start();
            extractor.WaitForExit();
            return destination;
        }

        static async Task<string> GetSuitableAudioExtension(string videoPath)
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return null;
            }

            var startInfo = new ProcessStartInfo
            {
                Arguments = $"-i \"{videoPath}\"",
                FileName = "ffmpeg",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };
            var infoReader = new Process { StartInfo = startInfo };
            infoReader.Start();
            var result = await infoReader.StandardError.ReadToEndAsync();
            var audioType = Regex.Match(result, "Audio: (?<type>.+?) ").Groups["type"].Value;

            var extension = "";
            switch (audioType)
            {
                case "aac":
                    extension = "aac";
                    break;

                case "vorbis":
                    extension = "ogg";
                    break;
            }

            if (string.IsNullOrEmpty(extension)) throw new Exception($"Couldn't specify suitable extension. AudioType is {audioType}");

            return extension;
        }

        public static void EncodeToVideo(VideoClip clip, Codec codec, string fileName, int crf, string audioPath)
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return;
            }

            string codecStr;
            switch (codec)
            {
                case Codec.H264:
                    codecStr = "libx264";
                    break;

                case Codec.H265:
                    codecStr = "hevc";
                    break;

                case Codec.H264_NVENC:
                    codecStr = "h264_nvenc";
                    break;

                case Codec.H265_NVENC:
                    codecStr = "hevc_nvenc";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var destination = GetValidFilePath(fileName);
            var startInfo = new ProcessStartInfo
            {
                Arguments = $"-r {clip.frameRate.ToString()} -i image_%07d.png -i \"{audioPath}\" -vcodec {codecStr} -crf {crf.ToString()} -pix_fmt yuv420p \"{destination}\"",
                FileName = "ffmpeg",
                WorkingDirectory = PathProvider.WorkDir
            };
            var process = new Process { StartInfo = startInfo };
            process.Start();
            Debug.Log($"Creating video in {destination}");
        }

        static string GetValidFilePath(string fileName)
        {
            var path = Path.Combine(PathProvider.DestinationDir, $"{fileName}.mp4");
            if (!File.Exists(path)) return path;

            for (var i = 2; i < int.MaxValue; i++)
            {
                path = Path.Combine(PathProvider.DestinationDir, $"{fileName} {i.ToString()}.mp4");
                if (!File.Exists(path)) return path;
            }

            throw new Exception("Couldn't create valid file path.");
        }

        static bool FfmpegIsAvailable
        {
            get
            {
                try
                {
                    var startInfo = new ProcessStartInfo { FileName = "ffmpeg" };
                    var p = new Process { StartInfo = startInfo };
                    p.Start();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}