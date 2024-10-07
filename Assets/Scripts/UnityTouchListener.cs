using System;
using UnityEngine;

public class UnityTouchListener : AndroidJavaProxy
{
     public delegate void TouchEventCallback(int x, int y, double timestamp, int phase);
     public TouchEventCallback OnTouchEvent;

     public UnityTouchListener(TouchEventCallback callback) : base("android.view.View$OnTouchListener")
     {
          OnTouchEvent = callback;
     }

     public bool onTouch(AndroidJavaObject view, AndroidJavaObject motionEvent)
     {
          // Log entry with timestamp
          Debug.Log($"[UnityTouchListener] onTouch called at {GetCurrentDateTimeAsString()}");

          // Get action
          int action = motionEvent.Call<int>("getActionMasked");
          int pointerIndex = motionEvent.Call<int>("getActionIndex");
          float x = motionEvent.Call<float>("getX", pointerIndex);
          float y = motionEvent.Call<float>("getY", pointerIndex);
          long eventTime = motionEvent.Call<long>("getEventTime");

          // Convert action to phase
          int phase = GetPhaseFromAction(action);

          // Convert eventTime to milliseconds (eventTime is in milliseconds since boot, including sleep)
          double timestamp = eventTime;

          // Log the touch event details with timestamp
          Debug.Log($"[UnityTouchListener] Touch at position: ({x}, {y}) | Phase: {phase} | Event Time: {timestamp} | Time: {GetCurrentDateTimeAsString()}");

          // Call the callback
          OnTouchEvent?.Invoke((int)x, (int)y, timestamp, phase);

          // Return true to indicate the event is handled
          return true;
     }

     private int GetPhaseFromAction(int action)
     {
          const int ACTION_DOWN = 0; // MotionEvent.ACTION_DOWN
          const int ACTION_UP = 1; // MotionEvent.ACTION_UP
          const int ACTION_MOVE = 2; // MotionEvent.ACTION_MOVE
          const int ACTION_CANCEL = 3; // MotionEvent.ACTION_CANCEL

          switch (action)
          {
               case ACTION_DOWN:
                    return 0; // Began
               case ACTION_MOVE:
                    return 1; // Moved
               case ACTION_UP:
                    return 3; // Ended
               case ACTION_CANCEL:
                    return 4; // Cancelled
               default:
                    return -1; // Unknown
          }
     }
     private string GetCurrentDateTimeAsString()
     {
          return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
     }
}
