﻿using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class RecorderWindow : EditorWindow
    {
        [MenuItem("Movie/Export...")]
        public static void Create()
        {
            var window = GetWindow<RecorderWindow>("Spherical Movie Editor");
            window.minSize = new Vector2(320, 274);
            window.maxSize = new Vector2(320, 274);
            window.autoRepaintOnSceneChange = true;

            var options = RecorderOptions.Options;

            if (options == null)
            {
                options = CreateInstance<RecorderOptions>();
                AssetDatabase.CreateAsset(options, PathProvider.OptionPath);
            }
        }

        void OnGUI()
        {
            var options = RecorderOptions.Options;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);
            var height = EditorGUILayout.IntField("Height", options.Height);
            var width = EditorGUILayout.IntField("Width", options.Width);
            var mapSize = EditorGUILayout.IntField("Map Size", options.MapSize);
            var renderStereo = EditorGUILayout.Toggle("Render Stereo", options.renderStereo);
            var stereoSeparation = EditorGUILayout.FloatField("Stereo Separation", options.StereoSeparation);
            var intermediateFormat = EditorGUILayout.EnumPopup("Intermediate Format", options.IntermediateFormat);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Encode Settings", EditorStyles.boldLabel);
            var codec = EditorGUILayout.EnumPopup("Codec", options.Codec);
            var crf = EditorGUILayout.IntSlider("CRF", options.Crf, 0, 51);
            var fileName = EditorGUILayout.TextField("File Name", options.FileName);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(options, "Change Recording Option");
                options.Height = height;
                options.Width = width;
                options.MapSize = mapSize;
                options.renderStereo = renderStereo;
                options.StereoSeparation = stereoSeparation;
                options.IntermediateFormat = (ImageRecorderSettings.ImageRecorderOutputFormat)intermediateFormat;
                options.Codec = (Codec)codec;
                options.Crf = crf;
                options.FileName = fileName;
                EditorUtility.SetDirty(options);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Capture and Encode"))
            {
                FrameCapturer.Export();
            }

            if (GUILayout.Button("Encode Only"))
            {
                FrameCapturer.Encode();
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
