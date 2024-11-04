using UnityEngine;
using SwiftGameDev.Touch;
using E7.Native;
using System.Collections;
using System.Linq;
public class TouchAudioHandler : MonoBehaviour
{
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

#if UNITY_ANDROID
     int FileID, SoundID;
#endif

     private void Awake()
    {
        this.gameObject.name = "TouchAudioHandler";

        // Set up the SwiftTouch class
        SwiftTouch.Start();

        // Subscribe to the OnTouchReceived action
        SwiftTouch.OnTouchesReceived += HandleTouchReceived;

#if UNITY_IOS
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
#elif UNITY_ANDROID

          LoadAndroidAudio();
#endif
     }

    private void OnDestroy()
    {
          UnloadAndroidAudio();
        // Unsubscribe from the OnTouchReceived action
        SwiftTouch.OnTouchesReceived -= HandleTouchReceived;

        // Clean up the SwiftTouch class
        SwiftTouch.OnDestroy();
    }

    private void Update()
    {
        // Test the old Update method
        // SwiftTouch.TestOldInputSystem();
    }

     public void AndroidTouchCallback(string touchDataJson)
     {
          SwiftTouch.NativeTouchCallback(touchDataJson);
     }
    // Example method subscribed to the OnTouchReceived action, will be called whenever a touch is received
    private void HandleTouchReceived()
    {
          if (SwiftTouch.CurrentTouches!= null)
          {
               foreach (var touch in SwiftTouch.CurrentTouches)
               {
                    Debug.Log($"[Unity] Touch Received: ID: {touch.fingerId} | Pos: {touch.position} | Time: {SwiftTouch.GetCurrentDateTimeAsString()} | Phase: {touch.phase}");
                    if (touch.phase == UnityEngine.TouchPhase.Began)
                    {
                         playTone();
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                         // Handle touch moved
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                         // Handle touch ended
                    }
               } 
          }
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

     public void LoadAndroidAudio()
     {
          AndroidNativeAudio.makePool();

          FileID = AndroidNativeAudio.load("A1.wav");
     }
     public void UnloadAndroidAudio()
     {
          AndroidNativeAudio.unload(FileID);
          AndroidNativeAudio.releasePool();
     }
     public void PlayStringChannelIOS(int channelIndex)
    {
        if (NativeAudio.Initialized == false) return;

        NativeAudio.GetNativeSource(channelIndex).Play(stringNativeAudioPointer);

        Debug.Log($"[Unity - PlayStringIOS] Playing audio for channel: {channelIndex} at pointer {stringNativeAudioPointer} | Time: {SwiftTouch.GetCurrentDateTimeAsString()}");
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
#if UNITY_IOS
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
#elif UNITY_ANDROID
          AndroidNativeAudio.play(FileID);
#endif
          Debug.Log("[Unity] Played tone for Channel ID: " + channelIds[channelIndex] + " attempted channel index: " + channelIndex + " | Time: " + SwiftTouch.GetCurrentDateTimeAsString());
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
        Debug.Log("[Unity] Stopped tone for Channel ID: " + channelIds[channelIndex] + " attempted channel index: " + channelIndex + " | Time: " + SwiftTouch.GetCurrentDateTimeAsString());
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
