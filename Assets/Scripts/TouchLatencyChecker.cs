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
    private void Start()
    {
        instance = this;

        Debug.Log($"[Unity] TouchLatencyChecker: Start | TimeStamp: {instance.GetCurrentDateTimeAsString()}");

        // Call the iOS function to print its timestamp
        _PrintIOSTimeStamp(NativeTimestampCallback);

        _StartNativeTouch(NativeTouchCallback);

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

        LoadIOSAudio();
    }

    private void OnDestroy()
    {
        Debug.Log("TouchLatencyChecker: OnDestroy");
        _StopNativeTouch();
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

        stringNativeAudioPointer = NativeAudio.Load("A1.wav");
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

    public void playTone()
    {
        int LRUIndex = channelIndex; // fadeFlags.Length == sources.Length
        if (fadeFlags[LRUIndex] == 1)
            fadeFlags[LRUIndex] = 2;
        stopToneFadeOut();
        channelIndex++;
        channelIndex %= fadeFlags.Length;
        fadeFlags[channelIndex] = 0;


        if (soundIndex >= 0 && soundIndex <= 5)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
               // Marks note: Ignoreing e7 for andriod rn
               // //if (StringSystemUI.instance.testE7NativeAudioOnAndroid) // Not being used
               // //{
               //     // StringSystemUI.instance.SetVolumeStringChannelIOS(index, baseVolume);
               //     // StringSystemUI.instance.PlayStringChannelIOS(index);
               // //}
               // //else
               // //{
                    acousticVolume = baseVolume;
                    //  StringSystemUI.instance.SoundIDs[soundIndex] = AndroidNativeAudio.play(StringSystemUI.instance.FileIDs[soundIndex], acousticVolume, acousticVolume);

                    // !Note : Cant be sure which channel will be used/returned so decided to stop them if a valid channel was being used previously in channelindex we are attempting to use.
                    if (channelIds[channelIndex] != -1)
                    {
                         AndroidNativeAudio.stop(channelIds[channelIndex]);
                    }
                    channelIds[channelIndex] = AndroidNativeAudio.play(StringSystemUI.instance.FileIDs[soundIndex], acousticVolume, acousticVolume);
               // //}

#elif UNITY_IOS && !UNITY_EDITOR
               // iOS specific code goes here
               // Example:
               if (StringSystemUI.instance.testE7NativeAudioOnIOS)
               {
                    Debug.Log("Played tone for String: " +index + " Channel ID: " + channelIds[channelIndex] + " attempted channel index: " + channelIndex);

                    StringSystemUI.instance.SetVolumeStringChannelIOS(channelIds[channelIndex], baseVolume);
                    StringSystemUI.instance.PlayStringChannelIOS(channelIds[channelIndex],index);
// Either set of code works.
                    // StringSystemUI.instance.SetVolumeStringChannelIOS(nativeChannels[channelIndex], baseVolume);
                    // StringSystemUI.instance.PlayStringChannelIOS(nativeChannels[channelIndex], index);
               }
               else // not being used
               {
                    sources[channelIndex].volume = baseVolume;
                    sources[channelIndex].Play();
               }

#else
            sources[channelIndex].volume = baseVolume;
            sources[channelIndex].Play();
#endif

        }
        else
        {
            sources[channelIndex].volume = baseVolume;
            sources[channelIndex].Play();
            // mySound.volume = baseVolume;
            // mySound.Play();

        }
        setPitch();

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
                    // Old stop playing string code
                    if (soundIndex >= 0 && soundIndex <= 5)
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                         //// if (StringSystemUI.instance.testE7NativeAudioOnAndroid)
                         ////      StringSystemUI.instance.StopStringChannelIOS(index);
                         //// else
                         //// {

                                   // AndroidNativeAudio.stop(StringSystemUI.instance.SoundIDs[soundIndex]);
                                   AndroidNativeAudio.stop(channelIds[i]);
                              // }

#elif UNITY_IOS && !UNITY_EDITOR
                              // iOS specific code goes here
                              // Example:

                              if (StringSystemUI.instance.testE7NativeAudioOnIOS)
                              {
                                   StringSystemUI.instance.StopStringChannelIOS(channelIds[i]);
                                   // StringSystemUI.instance.StopStringChannelIOS(nativeChannels[i]);
                              }
                              else
                              {
                                   // mySound.Stop();
                                   sources[i].Stop();
                              }
#else
                        // mySound.Stop();
                        sources[i].Stop();
#endif
                    }
                    else
                    {
                        // mySound.Stop();
                        sources[i].Stop();
                    }

                    // setPitch();
                    /// reseting flags and stuff
                    fadeFlags[i] = 0;
                    fadeCounters[i] = 0;
                    fadeVolumes[i] = baseVolume;
                    continue;
                }


                fadeCounters[i]++;
                fadeVolumes[i] *= fadeMultipliers[fadeFlags[i]];

                // Fading stuff as per old code. 
                if (soundIndex >= 0 && soundIndex <= 5)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                         AndroidNativeAudio.setVolume(channelIds[i], fadeVolumes[i]);
                         // AndroidNativeAudio.setVolume(StringSystemUI.instance.SoundIDs[soundIndex], fadeVolumes[channelIndex], fadeVolumes[channelIndex]);
#elif UNITY_IOS && !UNITY_EDITOR
                         if (StringSystemUI.instance.testE7NativeAudioOnIOS)
                         {
                              StringSystemUI.instance.SetVolumeStringChannelIOS(channelIds[i], fadeVolumes[i]);
                              // StringSystemUI.instance.SetVolumeStringChannelIOS(nativeChannels[i], fadeVolumes[i]);

                              // What was this idk
                              // StringSystemUI.instance.PlayStringChannelIOS(index); 
                         }
                         else
                         {
                              sources[i].volume = fadeVolumes[i];
                         }

#else
                    sources[i].volume = fadeVolumes[i];
                    // mySound.volume = fadeVolume;
#endif
                }
                else
                {
                    sources[i].volume = fadeVolumes[i];
                    // mySound.volume = fadeVolume;
                }

            }

            yield return new WaitForSecondsRealtime(0.01f);
        }
        // inavlaidate the corotine.
        fadeCoroutine = null;

        //fadingOut = true;



        /// this code is for stopping the string.


        // fadingOut = false;
    }

    // TODO: figure out if need this and TRASH if not
    IEnumerator fadeInNote(int maxFadeCounterLocal = 10, float fadeMultiplierLocal = 0.3f)
    {

        int fadeCounter = 0;
        float fadeVolume = baseVolume;
        while (fadeCounter < maxFadeCounter)
        {

            if (soundIndex >= 0 && soundIndex <= 5)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                    AndroidNativeAudio.setVolume(StringSystemUI.instance.SoundIDs[soundIndex], baseVolume - fadeVolume, baseVolume - fadeVolume);
#elif UNITY_IOS && !UNITY_EDITOR
                if (StringSystemUI.instance.testE7NativeAudioOnIOS)
                {
                    StringSystemUI.instance.SetVolumeStringChannelIOS(index, baseVolume - fadeVolume);
                    // StringSystemUI.instance.PlayStringChannelIOS(index);
                }else{
                    mySound.volume = fadeVolume;
                }

#else
                mySound.volume = baseVolume - fadeVolume;
#endif
            }
            else
            {
                mySound.volume = baseVolume - fadeVolume;
            }
            yield return new WaitForEndOfFrame();
            fadeVolume *= fadeMultiplier;
            fadeCounter++;
        }

        if (soundIndex >= 0 && soundIndex <= 5)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (StringSystemUI.instance.testE7NativeAudioOnAndroid)
            {
                StringSystemUI.instance.StopStringChannelIOS(index);
            }
            else
            {
            AndroidNativeAudio.stop(StringSystemUI.instance.SoundIDs[soundIndex]);

            }

#elif UNITY_IOS && !UNITY_EDITOR
            // iOS specific code goes here
            // Example:


            if (StringSystemUI.instance.testE7NativeAudioOnIOS)
            {
                StringSystemUI.instance.StopStringChannelIOS(index);
            }
            else
            {
                mySound.Stop();
            }

#else
            mySound.Stop();
#endif
        }
        else
        {
            mySound.Stop();
        }

        fadeCoroutine = null;
    }


    //PITCH PARAMETERS

    //temporary pitch variables
    float pitch = 0;
    float MaxPitchDelta = 100f; //in %
    float pitchFactor = 0;
    float pitchDelta = 0;


    //pitch setting funcitons
    public void setPitchFactor(float pitch01)
    {
        pitchFactor = Mathf.Lerp(0, MaxPitchDelta, pitch01);
        setPitch();
    }

    public void setPitchDelta(float pitchD)
    {
        pitchDelta = pitchD;
        setPitch();
    }

    public void setPitch()
    {
        //Debug.Log("SET PITCH CALLED String: " + index);
        //Debug.Log("Current channelIndex: " + channelIndex);
        //Debug.Log("pitch was: " + pitchChangeNativeAudioBuffer);
        if (Game.soundController == null)
        {
            Debug.Log("no sound controller :')");
        }
        else
        {

            pitch = Game.soundController.pitchChangeFactors[Mathf.Max(0, currentHighest)];

            if (soundIndex >= 0 && soundIndex <= 5)
            {
                pitchChangeNativeAudioBuffer = pitch * (1 + pitchFactor * 0.01f) + pitchDelta;
#if UNITY_ANDROID && !UNITY_EDITOR
                    // if (StringSystemUI.instance.testE7NativeAudioOnAndroid)
                    // {
                    //      // TODO : NO WAY TO SET PITCH # TODO? TOUGH LUCK BRO
                    // }
                    // else
                    // {
                         AndroidNativeAudio.setRate(channelIds[channelIndex], pitchChangeNativeAudioBuffer);
                         // AndroidNativeAudio.setRate(StringSystemUI.instance.SoundIDs[soundIndex], pitchChangeNativeAudioBuffer);

                    // }
#elif UNITY_IOS && !UNITY_EDITOR
                    // iOS SET PITCH GOES HERE #TEST

                    if (StringSystemUI.instance.testE7NativeAudioOnIOS)
                    {

                         StringSystemUI.instance.SetPitchStringChannelIOS(channelIds[channelIndex], pitchChangeNativeAudioBuffer);
                         // StringSystemUI.instance.SetPitchStringChannelIOS(nativeChannels[channelIndex], pitchChangeNativeAudioBuffer);
                    }
                    else
                    {
                         sources[channelIndex].pitch = pitch * (1 + pitchFactor * 0.01f) + pitchDelta;
                    }

#else
                // mySound.pitch = pitch * (1 + pitchFactor * 0.01f) + pitchDelta;
                sources[channelIndex].pitch = pitch * (1 + pitchFactor * 0.01f) + pitchDelta;
#endif
            }
            else
            {
                sources[channelIndex].pitch = pitch * (1 + pitchFactor * 0.01f) + pitchDelta;
            }
        }
        //Debug.Log("Pitch Set to: " + pitchChangeNativeAudioBuffer);

    }



    //Fret position parameters

    bool[] fretflags = new bool[16];

    int currentHighest = -1;
    void setupFretflags()
    {
        for (int i = 0; i < 15; i++)
        {
            fretflags[i] = false;
        }
    }

    public void setFretFlag(int fretPos, bool newTouch = false)
    {
        fretflags[fretPos] = true;

        if (fretPos > currentHighest)
        {
            if (noteInProgress)
            {
                if (currentHighest != -1)
                {
                    if (newTouch)
                    {
                        recordNoteHammered = true;
                    }
                    else
                    {
                        recordNoteSlid = true;
                    }
                }
            }



            int temp = currentHighest;
            currentHighest = fretPos;



            setPitch();

            if (temp == -1) pluck();
        }
    }



    public void resetFretFlag(int fretPos, bool newTouch = false)
    {



        //Debug.Log("Resetting fret flags from " + fretPos);
        fretflags[fretPos] = false;


        if (fretPos == currentHighest)
        {
            for (int i = fretPos; i >= -1; i--)
            {
                if (i == -1)
                {
                    currentHighest = i;
                    break;
                }

                if (fretflags[i] == true)
                {
                    currentHighest = i;


                    //PULL OFF SYSTEM (Lifting finger plays note if another finger is on the same string, lower fret)
                    if (i < fretPos)
                    {

                        if (newTouch)
                        {
                            // stopPluck(true);

                            stopToneFadeOut();
                            playTone();
                            // setPitch();

                            if (noteInProgress) recordNotePulledoff = true;
                        }
                        else
                        {
                            setPitch();
                            if (noteInProgress) recordNoteSlid = true;
                        }

                    }




                    break;
                }
            }

            if (currentHighest == -1)
            {
                //setPitch();
                stopPluck();
            }
        }
    }

    public void clearAllFlags()
    {
        setupFretflags();
    }
}
