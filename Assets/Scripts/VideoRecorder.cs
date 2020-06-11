﻿using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public enum Codec
    {
        H264,
        H265,
        H264_NVENC,
        H265_NVENC
    }

    public class VideoRecorder : EditorWindow
    {
        static RecorderOptions options;
        static VideoPlayer video;
        static ImageRecorderSettings image;
        static RecorderController controller;
        static long frameCount;
        static bool nextFrameExists = true;

        public static void Export()
        {
            RemoveImages();
            PrepareToRecord();
        }

        static void RemoveImages()
        {
            if (Directory.Exists(PathProvider.WorkDir))
            {
                Directory.Delete(PathProvider.WorkDir, true);
                Debug.Log("Removed old images.");
            }
        }

        static void PrepareToRecord()
        {
            LoadPrerequisites();
            options.startRecordingOnEnterPlayMode = true;
            EditorUtility.SetDirty(options);
            AssetDatabase.SaveAssets();
            EditorApplication.EnterPlaymode();
            // ここでシーンに配置した VideoRecorderBridge が StartRecording を叩く。
            // なぜならプレイモードに入るタイミングで、おそらくドメインがリロードされて実行が停止してしまうからだ。
        }

        static void LoadPrerequisites()
        {
            options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
            video = FindObjectOfType<VideoPlayer>();
        }

        public static void StartRecording()
        {
            LoadPrerequisites();
            var settings = CreateInstance<RecorderControllerSettings>();
            settings.SetRecordModeToSingleFrame(0);

            image = CreateInstance<ImageRecorderSettings>();
            image.imageInputSettings = new Camera360InputSettings
            {
                Source = ImageSource.MainCamera,
                MapSize = options.MapSize,
                OutputHeight = options.Height,
                OutputWidth = options.Width,
                RenderStereo = options.renderStereo,
                StereoSeparation = options.StereoSeparation,
            };
            image.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            image.FileNameGenerator.Root = OutputPath.Root.Absolute;
            image.FileNameGenerator.Leaf = PathProvider.WorkDir;
            settings.AddRecorderSettings(image);

            controller = new RecorderController(settings);

            video.isLooping = false;
            frameCount = (long)video.frameCount;
            video.sendFrameReadyEvents = true;
            video.started += VideoOnStarted;
            video.frameReady += VideoOnFrameReady;
            video.Play();
        }

        static void VideoOnStarted(VideoPlayer source)
        {
            source.Pause();
            source.frame = 0;
            Debug.Log("Start Capturing");
        }

        static async void VideoOnFrameReady(VideoPlayer source, long frameidx)
        {
            image.FileNameGenerator.FileName = $"image_{video.frame:0000000}";
            controller.PrepareRecording();
            controller.StartRecording();
            await UniTask.WaitWhile(() => controller.IsRecording());
            if (nextFrameExists)
            {
                nextFrameExists = Next();
            }

            if (!nextFrameExists)
            {
                Debug.Log("Finish Capturing");
                Encode();
                EditorApplication.ExitPlaymode();
            }
        }

        /// <summary>
        /// Show next frame.
        /// </summary>
        /// <returns>Return true when another frame exists.</returns>
        static bool Next()
        {
            video.frame += 1;
            Debug.Log($"Next Frame is {(video.frame + 1).ToString()}/{frameCount.ToString()}");
            return video.frame < (frameCount - 1);
        }

        public static async void Encode()
        {
            // TODO ffmpeg なければ終了
            var path = await VideoEncoder.ExtractAudio();
            LoadPrerequisites();
            VideoEncoder.EncodeToVideo(video.clip, options.Codec, options.FileName, options.Crf, path);
        }

        // TODO エディタ拡張の似たやつ
        void OnApplicationQuit()
        {
            controller?.StopRecording();
        }
    }
}
