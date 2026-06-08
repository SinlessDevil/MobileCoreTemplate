using System;
using Code.Services.AudioVibrationFX.Vibration;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace Code.StaticData.AudioVibration
{
    [Serializable]
    public class VibrationData
    {
        public string Name;
        public VibrationMode Mode = VibrationMode.Preset;

        public HapticTypes HapticType;

        public float ConstantIntensity = 1f;
        public float ConstantDuration  = 0.3f;

        public float EmphasisIntensity  = 1f;
        public float EmphasisSharpness  = 1f;

        public AnimationCurve Curve         = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float          CurveDuration = 1f;

        [HideInInspector]
        public VibrationType VibrationType = VibrationType.Unknown;
    }
}
