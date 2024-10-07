using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.InputSystem;
using E7.Native;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TouchLatencyChecker : MonoBehaviour
{
    private double touchTimestamp;
    private Vector2 touchPosition;

    private static TouchLatencyChecker instance;

     //iOS Touch Code
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _StartNativeTouch(NativeTouchDelegate callback);

    [DllImport("__Internal")]
    private static extern void _StopNativeTouch();

    public delegate void NativeTouchDelegate(int x, int y, double iosTimeInMilliseconds, int state);

    [MonoPInvokeCallback(typeof(NativeTouchDelegate))]
    public static void NativeTouchCallback(int x, int y, double iosTimeInMilliseconds, int state)
    {
        //Start playing tone
        Debug.Log($"[Unity - Native Callback] Called playTone() from NativeTouchCallback | Time: {instance.GetCurrentDateTimeAsString()}");
        instance.playTone();
        //Update the touch position and timestamp with value passed from iOS
        instance.touchTimestamp = iosTimeInMilliseconds;
        instance.touchPosition = new Vector2(x, y);

        double unityTimestamp = instance.GetCurrentTimeInMilliseconds();
        double touchLatencyMs = unityTimestamp - instance.touchTimestamp;
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
#elif UNITY_ANDROID
     // Declare the delegate for the touch callback
     [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
     public delegate void NativeTouchDelegate(int x, int y, double timestamp, int phase);

     // Keep a reference to the delegate to prevent it from being garbage collected
     private static NativeTouchDelegate nativeTouchCallbackDelegate;

     // This method will be called from the native code
     [AOT.MonoPInvokeCallback(typeof(NativeTouchDelegate))]
     private static void NativeTouchCallback(int x, int y, double timestamp, int phase)
     {
          //Start playing tone
          Debug.Log($"[Unity - Native Callback] Called playTone() from NativeTouchCallback | Time: {instance.GetCurrentDateTimeAsString()}");
          instance.playTone();

          //Update the touch position and timestamp with value passed from Native
          instance.touchTimestamp = timestamp;
          instance.touchPosition = new Vector2(x, y);

          double unityTimestamp = instance.GetCurrentTimeInMilliseconds();
          double touchLatencyMs = unityTimestamp - instance.touchTimestamp;
          Debug.Log($"[Unity - Native Callback] Touch Latency: {touchLatencyMs:F3} ms | Phase {phase} | Time: {instance.GetCurrentDateTimeAsString()}");
     }

     // The function pointer to the delegate
     private static IntPtr nativeTouchCallbackPointer;

     // Native methods to register the callback
     [DllImport("NativeTouchPlugin")]
     private static extern void nativeRegisterTouchCallback(IntPtr callback);

     [DllImport("NativeTouchPlugin")]
     private static extern void nativeStartTouch();

     [DllImport("NativeTouchPlugin")]
     private static extern void nativeStopTouch();

     private AndroidJavaClass nativeTouchRecognizer;

     // Method called from Java via UnitySendMessage (UNUSED)
     //public void NativeTouchCallback(string message)
     //{
     //     // Parse the message received from Java
     //     string[] parts = message.Split(',');
     //     if (parts.Length == 4)
     //     {
     //          int x = int.Parse(parts[0]);
     //          int y = int.Parse(parts[1]);
     //          double timestamp = double.Parse(parts[2]);
     //          int phase = int.Parse(parts[3]);

     //          Debug.Log($"[Unity - Native Callback] Touch at position: ({x}, {y}) | Phase: {phase} | Time: {GetCurrentDateTimeAsString()}");

     //          // Handle touch event (e.g., play tone)
     //          playTone();
     //     }
     //     else
     //     {
     //          Debug.LogError("Invalid message received from NativeTouchRecognizer: " + message);
     //     }
     //}
#endif

    //Audio Code
    int[] channelIds; // 2 per string for AndriodNativeAudio in andriod devices and AndriodNativeAudio in adriod devices.
    public int channelIndex = 0; // which audio source to play rn (LRU: least recently used)
    public int[] fadeFlags;
    /*   0: not fading/playing (turn off coroutine)
         1: fading normally
         2: fading rapidly (basically clear this shit cuz all channels occupied)
    */
    float baseVolume = 1;
    int[] fadeCounters;
    float[] fadeVolumes;

    //Fade settings
    public int maxFadeCounter = 25; // How many loops to make for the fade counter

    public float fadeMultiplier = 0.75f; // What to multiply fade with

    //bool fadingOut = false;
    public float[] fadeMultipliers = new float[] { 1, 0.8f, .05f }; // 0: normal, 1: rapid

    bool noteInProgress = false;
    private void Start()
    {
        instance = this;

        Debug.Log($"[Unity] TouchLatencyChecker: Start | TimeStamp: {instance.GetCurrentDateTimeAsString()}");

        // Set the GameObject name so UnitySendMessage can find it
        this.gameObject.name = "TouchLatencyChecker";

#if UNITY_IOS
        // Call the iOS function to print its timestamp
        _PrintIOSTimeStamp(NativeTimestampCallback);

        _StartNativeTouch(NativeTouchCallback);
#elif UNITY_ANDROID
          // Initialize the delegate
          nativeTouchCallbackDelegate = NativeTouchCallback;

          // Get the function pointer
          nativeTouchCallbackPointer = Marshal.GetFunctionPointerForDelegate(nativeTouchCallbackDelegate);

          // Register the callback with the native plugin
          nativeRegisterTouchCallback(nativeTouchCallbackPointer);
          Debug.Log($"[Unity] Registering touch callback with native plugin at {instance.GetCurrentDateTimeAsString()}");

          Debug.Log($"[Unity] Calling StartNativeTouch in Java at {instance.GetCurrentDateTimeAsString()}");
          // Call the Java method to start touch detection
          nativeTouchRecognizer = new AndroidJavaClass("com.swiftgamedev.nativetouch.NativeTouchRecognizer");
          nativeTouchRecognizer.CallStatic("StartNativeTouch");
#endif
        // Please ensure the length of sources is equal to no of channels
        int channelCount = 2;
        fadeVolumes = new float[channelCount];  // to track channel volume when fading

        channelIds = new int[channelCount]; // Tracks what channel is being played for Andriod platform
        fadeFlags = new int[channelCount]; // tracks if channels are fading or not
        fadeCounters = new int[channelCount]; // how much fading round has passed

        for (int i = 0; i < channelCount; i++)
        {
            // choosing channel for IOS.
            channelIds[i] = NativeAudio.GetNativeSource(i).Index;
            Debug.Log(" Channel ID: " + channelIds[i]);
            // nativeChannels[i] = NativeAudio.GetNativeSourceAuto(); 
            // This is not possible for andriodNativeAudio, only way to get ID is to via .Play() thingy it seems.
            fadeVolumes[i] = baseVolume;
        }

        baseVolume = 1.0f;

        StartCoroutine(LoadIOSAudio());
    }

    private void OnDestroy()
    {
        Debug.Log("TouchLatencyChecker: OnDestroy");
#if UNITY_IOS
        _StopNativeTouch();
#elif UNITY_ANDROID
          if (nativeTouchRecognizer != null)
          {
               nativeTouchRecognizer.CallStatic("StopNativeTouch");
          }
#endif
        NativeAudio.Dispose();
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

                double touchLatencyMs = unityEventTimestamp - touchTimestamp;
                Vector2 positionDifference = unityTouchPosition - touchPosition;

                Debug.Log($"[Unity InputSystem] Touch Latency: {touchLatencyMs:F3} ms");
                Debug.Log($"[Unity InputSystem] iOS Touch Position: {touchPosition}, Unity Touch Position: {unityTouchPosition}, Difference: {positionDifference}, Frame Number: {Time.frameCount}");
            }
        }
    }
    private void OnDisable()
    {
#if UNITY_ANDROID
                nativeTouchCallbackDelegate = null;
#endif
    }
    private string GetCurrentDateTimeAsString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
    private double GetCurrentTimeInMilliseconds()
    {
        return DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
        //return DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds - 20700000.0d;
    }

    private NativeAudioPointer stringNativeAudioPointer;
    public IEnumerator LoadIOSAudio()
    {
        if (NativeAudio.OnSupportedPlatform == false)
        {
            Debug.Log("[Unity] Not supported platform");
            yield break;
        }
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

        stringNativeAudioPointer = NativeAudio.Load("A1.wav");

        Debug.Log($"[Unity - LoadIOSAudio] Loaded audio file: A1.wav at pointer: {stringNativeAudioPointer}");
    }
    public void PlayStringChannelIOS(int channelIndex)
    {
        if (NativeAudio.Initialized == false) return;

        NativeAudio.GetNativeSource(channelIndex).Play(stringNativeAudioPointer);

        Debug.Log($"[Unity - PlayStringIOS] Playing audio for channel: {channelIndex} at pointer {stringNativeAudioPointer} | Time: {instance.GetCurrentDateTimeAsString()}");
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

    public void playTone()
    {
        int LRUIndex = channelIndex; // fadeFlags.Length == sources.Length
        if (fadeFlags[LRUIndex] == 1)
            fadeFlags[LRUIndex] = 2;
        stopToneFadeOut();
        channelIndex++;
        channelIndex %= fadeFlags.Length;
        fadeFlags[channelIndex] = 0;

        // iOS specific code goes here


        SetVolumeStringChannelIOS(channelIds[channelIndex], baseVolume);
        PlayStringChannelIOS(channelIds[channelIndex]);

        Debug.Log("[Unity] Played tone for Channel ID: " + channelIds[channelIndex] + " attempted channel index: " + channelIndex + " | Time: " + instance.GetCurrentDateTimeAsString());
        //setPitch();
    }

    public void stopToneFadeOut()
    {
        // Debug.Log(channelIndex);
        // nisan edit
        if (fadeFlags[channelIndex] == 0)
        {

            fadeFlags[channelIndex] = 1;
            fadeCounters[channelIndex] = 0;
            fadeVolumes[channelIndex] = baseVolume;
        }
        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(fadeOutNote());
        }
        Debug.Log("[Unity] Stopped tone for Channel ID: " + channelIds[channelIndex] + " attempted channel index: " + channelIndex + " | Time: " + instance.GetCurrentDateTimeAsString());
        // Debug.Log("Fader end flags:" + string.Join(", ", fadeFlags));
    }

    Coroutine fadeCoroutine = null;
    ////This holds the fade coroutine so if fading is occuring when a note needs to be played, it can be ~~stopped~~ faded rapildy.
    // fadeCoroutine now holds the coroutine so that it can fade channels according to the fadeFlags so that oldest channels can be faded rapidly.
    IEnumerator fadeOutNote()
    {
        while (fadeFlags.Any(flag => flag != 0))
        {
            for (int i = 0; i < fadeFlags.Length; i++)
            {
                if (fadeFlags[i] == 0) continue;
                if (fadeCounters[i] >= maxFadeCounter || fadeVolumes[i] < 0.0001f)
                {
                    // iOS specific code goes here

                    StopStringChannelIOS(channelIds[i]);

                    /// reseting flags and stuff
                    fadeFlags[i] = 0;
                    fadeCounters[i] = 0;
                    fadeVolumes[i] = baseVolume;
                    continue;
                }

                fadeCounters[i]++;
                fadeVolumes[i] *= fadeMultipliers[fadeFlags[i]];


                SetVolumeStringChannelIOS(channelIds[i], fadeVolumes[i]);

            }

            yield return new WaitForSecondsRealtime(0.01f);
        }
        // inavlaidate the corotine.
        fadeCoroutine = null;
    }

}