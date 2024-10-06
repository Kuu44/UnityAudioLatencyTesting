#if UNITY_ANDROID
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;

namespace SwiftGameDev.Touch
{
    public static partial class SwiftTouch
    {
        // Declare the delegate for the touch callback
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NativeTouchDelegate(int x, int y, double timestamp, int phase);

        // Keep a reference to the delegate to prevent it from being garbage collected
        private static NativeTouchDelegate nativeTouchCallbackDelegate;

        // The function pointer to the delegate
        private static IntPtr nativeTouchCallbackPointer;

        // Native methods to register the callback
        [DllImport("NativeTouchPlugin")]
        private static extern void nativeRegisterTouchCallback(IntPtr callback);

        [DllImport("NativeTouchPlugin")]
        private static extern void nativeStartTouch();

        [DllImport("NativeTouchPlugin")]
        private static extern void nativeStopTouch();

        private static AndroidJavaClass nativeTouchRecognizer;

        [AOT.MonoPInvokeCallback(typeof(NativeTouchDelegate))]
        public static void NativeTouchCallback(int x, int y, double timestamp, int phase)
        {
            Debug.Log($"[Unity - Native Callback] NativeTouchCallback called | Time: {GetCurrentDateTimeAsString()}");

            touchTimestamp = timestamp;
            touchPosition = new Vector2(x, y);

            double unityTimestamp = GetCurrentTimeInMilliseconds();
            double touchLatencyMs = unityTimestamp - touchTimestamp;
            Debug.Log($"[Unity - Native Callback] Touch Latency: {touchLatencyMs:F3} ms | Phase {phase} | Time: {GetCurrentDateTimeAsString()}");

            // Invoke the action
            OnTouchReceived?.Invoke();
        }

        public static void StartAndroid()
        {
            nativeTouchCallbackDelegate = NativeTouchCallback;

            nativeTouchCallbackPointer = Marshal.GetFunctionPointerForDelegate(nativeTouchCallbackDelegate);

            nativeRegisterTouchCallback(nativeTouchCallbackPointer);
            Debug.Log($"[Unity] Registering touch callback with native plugin at {GetCurrentDateTimeAsString()}");

            Debug.Log($"[Unity] Calling StartNativeTouch in Java at {GetCurrentDateTimeAsString()}");
            nativeTouchRecognizer = new AndroidJavaClass("com.swiftgamedev.nativetouch.NativeTouchRecognizer");
            nativeTouchRecognizer.CallStatic("StartNativeTouch");
        }

        public static void OnDestroyAndroid()
        {
            if (nativeTouchRecognizer != null)
            {
                nativeTouchRecognizer.CallStatic("StopNativeTouch");
            }
        }
    }
}
#endif
