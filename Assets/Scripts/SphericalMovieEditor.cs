﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class SphericalMovieEditor : MonoBehaviour
    {
        public static SphericalMovieEditor Current { get; private set; }

        [SerializeField]
        VideoClip clip;

        [SerializeField]
        bool useProxy;

        bool prevUseProxy;
        VideoClip ProxyClip
        {
            get
            {
                var proxyPath = PathProvider.GetProxyPathRelative(clip);
                return AssetDatabase.LoadAssetAtPath<VideoClip>(proxyPath);
            }
        }

        VideoPlayer player;

        void Awake()
        {
            Current = this;
        }

        void OnValidate()
        {
            player = GetComponentInChildren<VideoPlayer>();
            if (!player) return;

            SetClip(player);
        }

        async void SetClip(VideoPlayer player)
        {
            if (clip == null)
            {
                player.clip = clip;
                return;
            }

            if (useProxy != prevUseProxy)
            {
                if (useProxy && !ProxyClip)
                {
                    await VideoEncoder.EncodeProxy(clip);
                    AssetDatabase.Refresh();
                }

                prevUseProxy = useProxy;
            }

            player.clip = useProxy ? ProxyClip : clip;
        }

        public void UseOriginalClip()
        {
            player.clip = clip;
        }
    }
}