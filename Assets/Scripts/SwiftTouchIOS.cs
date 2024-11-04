using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SwiftGameDev.Touch
{
    public static partial class SwiftTouch
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _StartNativeTouch(NativeTouchesDelegate callback);

        [DllImport("__Internal")]
        private static extern void _StopNativeTouch();

        public delegate void NativeTouchesDelegate(string touchDataJson);

        [MonoPInvokeCallback(typeof(NativeTouchesDelegate))]
        public static void NativeTouchCallback(string touchDataJson)
        {
            if (string.IsNullOrEmpty(touchDataJson))
            {
                Debug.LogError("[Unity - Native Callback] Received empty touch data JSON");
                return;
            }

            try
            {
                var touchesData = JArray.Parse(touchDataJson);
                List<TouchData> touches = new List<TouchData>();

                foreach (var touchObj in touchesData)
                {
                    int id = touchObj["id"].Value<int>();
                    float x = touchObj["x"].Value<float>();
                    float y = touchObj["y"].Value<float>();
                    double timestamp = touchObj["timestamp"].Value<double>();
                    int phaseInt = touchObj["phase"].Value<int>();

                    TouchData touchData = new TouchData
                    {
                        fingerId = id,
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

        // Convert iOS UITouchPhase to UnityEngine.TouchPhase
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


        // Sync Time
        [DllImport("__Internal")]
        private static extern void _PrintIOSTimeStamp(NativeTimestampDelegate callback);
        public delegate void NativeTimestampDelegate(string timestamp);

        [MonoPInvokeCallback(typeof(NativeTimestampDelegate))]
        public static void NativeTimestampCallback(string timestamp)
        {
            Debug.Log($"[Unity - Native Callback] Received iOS Time Stamp: {timestamp}");
            Debug.Log($"[Unity - Native Callback] Unity TimeStamp: {GetCurrentDateTimeAsString()}");
        }

        public static void StartIOS()
        {
            _PrintIOSTimeStamp(NativeTimestampCallback);
            _StartNativeTouch(NativeTouchCallback);
        }

        public static void OnDestroyIOS()
        {
            _StopNativeTouch();
        }
#endif
    }
}
