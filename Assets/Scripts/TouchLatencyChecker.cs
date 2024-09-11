using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.InputSystem;
using E7.Native;
using System.Collections;
using FMODUnity;
using System.Collections.Generic;

public class TouchLatencyChecker : MonoBehaviour
{
    //Touch Code
    [DllImport("__Internal")]
    private static extern void _StartNativeTouch(NativeTouchDelegate callback);

    [DllImport("__Internal")]
    private static extern void _StopNativeTouch();

    public delegate void NativeTouchDelegate(int x, int y, double iosTimeInMilliseconds, int state);

    private static TouchLatencyChecker instance;
    private double iosTouchTimestamp;
    private Vector2 iosTouchPosition;

    [MonoPInvokeCallback(typeof(NativeTouchDelegate))]
    public static void NativeTouchCallback(int x, int y, double iosTimeInMilliseconds, int state)
    {
        //Update the touch position and timestamp with value passed from iOS
        instance.iosTouchTimestamp = iosTimeInMilliseconds;
        instance.iosTouchPosition = new Vector2(x, y);

        double unityTimestamp = instance.GetCurrentTimeInMilliseconds();
        double touchLatencyMs = unityTimestamp - instance.iosTouchTimestamp;
        Debug.Log($"[Unity - Native Callback] Touch Latency: {touchLatencyMs:F3} ms | Phase {state} | Time: {instance.GetCurrentDateTimeAsString()}");
    }

    //Sync Time
    [DllImport("__Internal")]
    private static extern void _PrintIOSTimeStamp(NativeTimestampDelegate callback);
    public delegate void NativeTimestampDelegate(string timestamp);

    [MonoPInvokeCallback(typeof(NativeTimestampDelegate))]
    public static void NativeTimestampCallback(string timestamp)
    {
        Debug.Log($"[Unity - Native Callback] Received iOS Time Stamp: {timestamp}");
        Debug.Log($"[Unity - Native Callback] Unity TimeStamp: {instance.GetCurrentDateTimeAsString()}");
    }

    private void Start()
    {
        instance = this;

        Debug.Log($"[Unity] TouchLatencyChecker: Start | TimeStamp: {instance.GetCurrentDateTimeAsString()}");

        // Call the iOS function to print its timestamp
        _PrintIOSTimeStamp(NativeTimestampCallback);

        _StartNativeTouch(NativeTouchCallback);

          //Audio Code
          LoadIOSAudio();
     }

    private void OnDestroy()
    {
        Debug.Log("TouchLatencyChecker: OnDestroy");
        _StopNativeTouch();
    }

    private void Update()
    {
        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                double unityEventTimestamp = GetCurrentTimeInMilliseconds();
                Vector2 unityTouchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

                // Log the Unity timestamp in hh:mm:ss.mmm format
                Debug.Log($"[Unity InputSystem] Unity Touch Timestamp: {instance.GetCurrentDateTimeAsString()} with Phase: {Touchscreen.current.primaryTouch.phase.ReadValue()}");

                double touchLatencyMs = unityEventTimestamp - iosTouchTimestamp;
                Vector2 positionDifference = unityTouchPosition - iosTouchPosition;

                Debug.Log($"[Unity InputSystem] Touch Latency: {touchLatencyMs:F3} ms");
                Debug.Log($"[Unity InputSystem] iOS Touch Position: {iosTouchPosition}, Unity Touch Position: {unityTouchPosition}, Difference: {positionDifference}, Frame Number: {Time.frameCount}");
            }
        }
    }
    private string GetCurrentDateTimeAsString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
    private double GetCurrentTimeInMilliseconds()
    {
        return DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds - 20700000.0d;
    }

     private NativeAudioPointer stringNativeAudioPointer;
     public IEnumerator LoadIOSAudio()
     {
          if (NativeAudio.OnSupportedPlatform == false) yield break;
          NativeAudio.Dispose();
          var option = NativeAudio.InitializationOptions.defaultOptions;

          option.androidAudioTrackCount = 6 * 2;

          //The dispose above followed by this initialize could cause problem on some phones.
          //You will get non-fast track if you previously holding too many fast ones, because
          //it need more time to make the fast track available again after release. The easiest
          //way to reproduce is to press Next Scene until it came back to this scene again.

          //So! This little wait will help that.

          yield return new WaitForSeconds(0.5f);

          NativeAudio.Initialize(option);
          stringNativeAudioPointer=NativeAudio.Load("A1.wav");
     }
     public void PlayStringChannelIOS(int channelIndex)
     {
          if (NativeAudio.Initialized == false) return;

          NativeAudio.GetNativeSource(channelIndex).Play(stringNativeAudioPointer);
     }
     public void SetVolumeStringChannelIOS(int channelIndex, float volume01)
     {
          if (NativeAudio.Initialized == false) return;

          NativeAudio.GetNativeSource(channelIndex).SetVolume(volume01);
     }

     public void StopStringChannelIOS(int channelIndex)
     {
          if (NativeAudio.Initialized == false) return;

          NativeAudio.GetNativeSource(channelIndex).Stop();
     }

     public void SetPitchStringChannelIOS(int channelIndex, float pitch)
     {
          if (NativeAudio.Initialized == false) return;

          NativeAudio.GetNativeSource(channelIndex).SetPitch(pitch);
     }
}
