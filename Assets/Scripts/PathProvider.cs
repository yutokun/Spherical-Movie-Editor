﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public static class PathProvider
    {
        public static string WorkDir => Path.Combine(Directory.GetCurrentDirectory(), "VideoRecorder");
        public static string ProxyDir => Path.Combine(Application.dataPath, "Proxy");
        public static string GetProxyPath(VideoClip clip) => Path.Combine(ProxyDir, $"proxy-{clip.name}.mp4");
        public static string GetProxyPathRelative(VideoClip clip) => Path.Combine("Assets", "Proxy", $"proxy-{clip.name}.mp4");
        public static string DestinationDir => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string InternalOptionPath = "Assets/RecorderInternalOptions/Internal.asset";
        public static string InternalOptionDir = "Assets/RecorderInternalOptions/";
        public static string DefaultOptionPath = "Assets/RecorderOptions/Default.asset";
        public static string OptionDir = "Assets/RecorderOptions/";
    }
}
