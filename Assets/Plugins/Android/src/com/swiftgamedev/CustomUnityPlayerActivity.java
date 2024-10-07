package com.swiftgamedev;

import android.view.MotionEvent;
import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayer;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {

    @Override
    public boolean dispatchTouchEvent(MotionEvent event) {
        // Get touch event details
        int action = event.getAction();
        int x = (int) event.getX();
        int y = (int) event.getY();
        long timestamp = event.getEventTime();
        int phase = action & MotionEvent.ACTION_MASK;

        Log.d("[Native] Touch at position: (" + x + ", " + y + ") | Phase: " + phase + " | Time: " + timestamp);
        // Send touch event to Unity
        UnityPlayer.UnitySendMessage("TouchLatencyChecker", "NativeTouchCallback",
                x + "," + y + "," + timestamp + "," + phase);

        // Proceed with the usual dispatch
        return super.dispatchTouchEvent(event);
    }
}