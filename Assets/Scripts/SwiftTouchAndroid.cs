
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SwiftGameDev.Touch
{
     public static partial class SwiftTouch
     {
#if UNITY_ANDROID
          //// Declare the delegate for the touch callback
          //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
          //public delegate void NativeTouchDelegate(int x, int y, double timestamp, int phase);

          //// Keep a reference to the delegate to prevent it from being garbage collected
          //private static NativeTouchDelegate nativeTouchCallbackDelegate;

          //// The function pointer to the delegate
          //private static IntPtr nativeTouchCallbackPointer;

          //// Native methods to register the callback
          //[DllImport("NativeTouchPlugin")]
          //private static extern void nativeRegisterTouchCallback(IntPtr callback);

          //[DllImport("NativeTouchPlugin")]
          //private static extern void nativeStartTouch();

          //[DllImport("NativeTouchPlugin")]
          //private static extern void nativeStopTouch();

          //private static AndroidJavaClass nativeTouchRecognizer;

          //[AOT.MonoPInvokeCallback(typeof(NativeTouchDelegate))]
          //public static void NativeTouchCallback(int x, int y, double timestamp, int phase)
          //{
          //     Debug.Log($"[Unity - Native Callback] NativeTouchCallback called | Time: {GetCurrentDateTimeAsString()}");

          //     touchTimestamp = timestamp;
          //     touchPosition = new Vector2(x, y);

          //     double unityTimestamp = GetCurrentTimeInMilliseconds();
          //     double touchLatencyMs = unityTimestamp - touchTimestamp;
          //     Debug.Log($"[Unity - Native Callback] Touch Latency: {touchLatencyMs:F3} ms | Phase {phase} | Time: {GetCurrentDateTimeAsString()}");

          //     // Invoke the action
          //     OnTouchReceived?.Invoke();
          //}
          // Callback method to receive touch events from the custom activity
          public static void NativeTouchCallback(string touchDataJson)
          {
               if (string.IsNullOrEmpty(touchDataJson))
               {
                    Debug.LogError("[Unity - Native Callback] Received empty touch data JSON");
                    return;
               }

               try
               {
                    var wrapper = JObject.Parse(touchDataJson);
                    var touchesArray = wrapper["touches"] as JArray;
                    List<TouchData> touches = new List<TouchData>();

                    foreach (var touchObj in touchesArray)
                    {
                         int fingerId = touchObj["fingerId"].Value<int>();
                         float x = touchObj["x"].Value<float>();
                         float y = touchObj["y"].Value<float>();
                         double timestamp = touchObj["timestamp"].Value<double>();
                         int phaseInt = touchObj["phase"].Value<int>();

                         TouchData touchData = new TouchData
                         {
                              fingerId = fingerId,
                              position = new Vector2(x, y),
                              timestamp = timestamp,
                              phase = ConvertPhase(phaseInt)
                         };

                         Debug.Log($"[Unity - Native Callback] Touch Data: ID: {touchData.fingerId} | Pos: {touchData.position} | Time:{GetCurrentDateTimeAsString()} | Phase: {touchData.phase}");
                         touches.Add(touchData);
                    }

                    // Update the public touches array
                    CurrentTouches = touches.ToArray();

                    // Invoke the action
                    OnTouchesReceived?.Invoke();

               }
               catch (Exception ex)
               {
                    Debug.LogError($"[Unity - Native Callback] Error parsing touch data JSON: {ex.Message}");
               }
          }
          // Convert Android touch phase to UnityEngine.TouchPhase
          private static TouchPhase ConvertPhase(int phase)
          {
               switch (phase)
               {
                    case 0: return TouchPhase.Began;
                    case 1: return TouchPhase.Moved;
                    case 2: return TouchPhase.Stationary;
                    case 3: return TouchPhase.Ended;
                    case 4: return TouchPhase.Canceled;
                    default: return TouchPhase.Canceled;
               }
          }
          public static void StartAndroid()
          {
               //nativeTouchCallbackDelegate = NativeTouchCallback;

               //nativeTouchCallbackPointer = Marshal.GetFunctionPointerForDelegate(nativeTouchCallbackDelegate);

               //nativeRegisterTouchCallback(nativeTouchCallbackPointer);
               //Debug.Log($"[Unity] Registering touch callback with native plugin at {GetCurrentDateTimeAsString()}");

               Debug.Log($"[Unity] Initiating Android Touch in Java at {GetCurrentDateTimeAsString()}");
               //nativeTouchRecognizer = new AndroidJavaClass("com.swiftgamedev.nativetouch.NativeTouchRecognizer");
               //nativeTouchRecognizer.CallStatic("StartNativeTouch");
          }

          public static void OnDestroyAndroid()
          {
               //if (nativeTouchRecognizer != null)
               //{
               //     nativeTouchRecognizer.CallStatic("StopNativeTouch");
               //}
          }
#endif
     }
}
