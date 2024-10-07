package com.swiftgamedev.nativetouch;

import android.app.Activity;
import android.os.SystemClock;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

import com.unity3d.player.UnityPlayer;

public class NativeTouchRecognizer {

    private static final String TAG = "NativeTouchRecognizer";

    static {
        Log.d(TAG, "Library loaded from: " + System.mapLibraryName("NativeTouchPlugin"));
        System.loadLibrary("NativeTouchPlugin");
    }

    // Native Methods
    private static native void nativeRegisterTouchCallback(long callbackPtr);

    private static native void nativeOnTouch(int x, int y, double timestamp, int phase);

    private static native void nativeStartTouch();

    private static native void nativeStopTouch();

    private static View unityView;
    private static View.OnTouchListener touchListener;
    private static boolean isTouchListenerActive = false;
    private static int screenHeight;

    // Utility function to get the current time in milliseconds
    public static long GetCurrentTimeInMilli() {
        return System.currentTimeMillis();
    }

    // Utility function to get the current time as a string
    public static String GetCurrentTimeAsString() {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS", Locale.US);
        return sdf.format(new Date());
    }

    public static void StartNativeTouch() {
        Log.d(TAG, "[Java] StartNativeTouch called at " + GetCurrentTimeAsString());

        if (isTouchListenerActive) {
            Log.d(TAG, "[Java] Touch listener is already active. Exiting StartNativeTouch.");
            return;
        }
        final Activity unityActivity = UnityPlayer.currentActivity;

        // Get the Unity View
        unityView = UnityPlayer.currentActivity.findViewById(android.R.id.content);

        if (unityView == null) {
            Log.e(TAG, "[Java] Unity view not found");
            return;
        }

        // Get screen height
        DisplayMetrics displayMetrics = new DisplayMetrics();
        unityActivity.getWindowManager().getDefaultDisplay().getMetrics(displayMetrics);
        screenHeight = displayMetrics.heightPixels;

        touchListener = new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                int action = event.getActionMasked();
                int pointerIndex = event.getActionIndex();
                float x = event.getX(pointerIndex);
                float y = event.getY(pointerIndex);
                float adjustedY = screenHeight - y; // Adjust Y coordinate

                long timestamp = System.currentTimeMillis();
                int phase = getPhaseFromAction(action);

                Log.d(TAG, "[Java] Touch at position: (" + x + ", " + adjustedY + ") | Phase: " + phase + " | Time: "
                        + GetCurrentTimeAsString());

                // // Send data back to Unity
                // String message = xInt + "," + yInt + "," + timestamp + "," + phase;
                // UnityPlayer.UnitySendMessage("TouchLatencyChecker", "NativeTouchCallback",
                // message);

                // Call native method
                nativeOnTouch((int) x, (int) adjustedY, (double) timestamp, phase);

                return true;
            }
        };

        unityView.setOnTouchListener(touchListener);
        isTouchListenerActive = true;

        Log.d(TAG, "[Java] Native touch listener started at " + GetCurrentTimeAsString());
    }

    public static void StopNativeTouch() {
        Log.d(TAG, "[Java] StopNativeTouch called at " + GetCurrentTimeAsString());

        if (unityView != null && touchListener != null) {
            unityView.setOnTouchListener(null);
            isTouchListenerActive = false;
        }
    }

    private static int getPhaseFromAction(int action) {
        switch (action) {
            case MotionEvent.ACTION_DOWN:
                return 0; // Began
            case MotionEvent.ACTION_MOVE:
                return 1; // Moved
            case MotionEvent.ACTION_UP:
                return 3; // Ended
            case MotionEvent.ACTION_CANCEL:
                return 4; // Cancelled
            default:
                return -1; // Unknown
        }
    }

}
