using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace SwiftGameDev.Touch
{
    public static partial class SwiftTouch
    {
        private static double touchTimestamp;
        private static Vector2 touchPosition;


        // Define the TouchData structure
        public struct TouchData
        {
            public int fingerId;
            public Vector2 position;
            public double timestamp;
            public UnityEngine.TouchPhase phase;
        }

        // Publicly accessible array of touches
        public static TouchData[] CurrentTouches;

        // Define an Action that can be subscribed to
        public static Action<TouchData[]> OnTouchesReceived;

        public static void Start()
        {
            Debug.Log($"[Unity] SwiftTouch: Start | TimeStamp: {GetCurrentDateTimeAsString()}");

#if UNITY_IOS
            StartIOS();
#elif UNITY_ANDROID
            StartAndroid();
#endif
        }

        public static void OnDestroy()
        {
            Debug.Log("SwiftTouch: OnDestroy");
#if UNITY_IOS
            OnDestroyIOS();
#elif UNITY_ANDROID
            OnDestroyAndroid();
#endif
        }

        public static void TestOldInputSystem()
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    double unityEventTimestamp = GetCurrentTimeInMilliseconds();

                    Vector2 unityTouchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

                    Debug.Log($"[Unity InputSystem] Unity Touch Timestamp: {GetCurrentDateTimeAsString()} with Phase: {Touchscreen.current.primaryTouch.phase.ReadValue()}");

                    double touchLatencyMs = unityEventTimestamp - touchTimestamp;
                    Vector2 positionDifference = unityTouchPosition - touchPosition;

                    Debug.Log($"[Unity InputSystem] Touch Latency: {touchLatencyMs:F3} ms");
                    Debug.Log($"[Unity InputSystem] Native Touch Position: {touchPosition}, Unity Touch Position: {unityTouchPosition}, Difference: {positionDifference}, Frame Number: {Time.frameCount}");
                }
            }
        }

        public static void OnDisable()
        {
            // Cleanup if necessary
        }

        public static string GetCurrentDateTimeAsString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static double GetCurrentTimeInMilliseconds()
        {
            return DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
        }
    }
}
