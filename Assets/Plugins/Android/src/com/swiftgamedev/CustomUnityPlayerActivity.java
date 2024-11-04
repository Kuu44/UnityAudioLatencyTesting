package com.swiftgamedev;

import android.util.Log;
import android.view.MotionEvent;
import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayer;
import org.json.JSONArray;
import org.json.JSONObject;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {

    @Override
    public boolean dispatchTouchEvent(MotionEvent event) {
        try {
            int pointerCount = event.getPointerCount();
            JSONArray touchesArray = new JSONArray();

            for (int i = 0; i < pointerCount; i++) {
                int pointerId = event.getPointerId(i);
                float x = event.getX(i);
                float y = event.getY(i);
                long timestamp = event.getEventTime();
                int actionMasked = event.getActionMasked();
                int actionIndex = event.getActionIndex();
                int phase = -1;

                switch (actionMasked) {
                    case MotionEvent.ACTION_DOWN:
                    case MotionEvent.ACTION_POINTER_DOWN:
                        if (i == actionIndex) {
                            phase = 0; // Began
                        } else {
                            phase = 2; // Stationary
                        }
                        break;
                    case MotionEvent.ACTION_MOVE:
                        phase = 1; // Moved
                        break;
                    case MotionEvent.ACTION_UP:
                    case MotionEvent.ACTION_POINTER_UP:
                        if (i == actionIndex) {
                            phase = 3; // Ended
                        } else {
                            phase = 2; // Stationary
                        }
                        break;
                    case MotionEvent.ACTION_CANCEL:
                        phase = 4; // Canceled
                        break;
                    default:
                        phase = 2; // Stationary
                        break;
                }

                JSONObject touchData = new JSONObject();
                touchData.put("fingerId", pointerId);
                touchData.put("x", x);
                touchData.put("y", y);
                touchData.put("timestamp", timestamp);
                touchData.put("phase", phase);

                touchesArray.put(touchData);
            }

            JSONObject wrapper = new JSONObject();
            wrapper.put("touches", touchesArray);
            String touchDataString = wrapper.toString();

            // Log the touch data
            Log.d("[Native]", "Touch Data: " + touchDataString);

            // Send touch data to Unity
            UnityPlayer.UnitySendMessage("TouchAudioHandler", "AndroidTouchCallback", touchDataString);

        } catch (Exception e) {
            Log.e("[Native]", "Error processing touch event: " + e.getMessage());
        }

        // Proceed with the usual dispatch
        return super.dispatchTouchEvent(event);
    }
}
