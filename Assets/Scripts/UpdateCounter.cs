using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class UpdateCounter : MonoBehaviour
{
     private int fixedUpdateCount = 0;
     private int updateCount = 0;
     private int lateUpdateCount = 0;
     private int onGUICount = 0;
     private int pressedCount = 0;

     private int maxGUICount = 0;
     private int maxUpdateCount = 0;
     private int maxLateUpdateCount = 0;
     float standardDeviation = 0f;

     public float[] deltaTimes;
     //Calculating S.D
     private float[] fixedUpdateTimes;  // Array to store time values of each FixedUpdate
     public float[] lastFrameUpdateTimes;
     private int maxFixedUpdateSamples = 260;  // Default size of the array

     bool fixedUpdateRun = false;
     bool updateRun = false;
     bool lateUpdateRun = false;

     [SerializeField]
     bool logEachFrame = false;

     [SerializeField]
     Slider rateSlider;

     //input 4 TextMeshPro - Text
     public TextMeshPro fixedUpdateText;
     public TextMeshPro UpdateText;
     public TextMeshPro lateUpdateText;
     public TextMeshPro onGUIText;

     public TextMeshPro maxGUIText;
     public TextMeshPro maxUpdateText;
     public TextMeshPro maxLateUpdateText;

     //Touch
     private TouchInputActions touchInputActions;
     private Vector2 touchPosition;
     private bool isTouching;

     private void Awake()
     {
          touchInputActions = new TouchInputActions();
          // Set the Input System to manual update mode
          InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
     }
     void Start()
     {

          if (rateSlider != null)
          {
               // Set an initial value for the slider (optional)
               rateSlider.value = 240f; // For example, set it to 50 FPS
                                        // Attach the listener to the slider
               SetFixedUpdateRate(rateSlider.value);
               rateSlider.onValueChanged.AddListener(SetFixedUpdateRate);
          }
          else
          {
               Debug.LogError("Rate Slider not assigned!");
          }

          // Initialize the array for storing FixedUpdate times
          fixedUpdateTimes = new float[maxFixedUpdateSamples];

          // Start the counting coroutine
          StartCoroutine(CountUpdatesForOneSecond());
     }

     void FixedUpdate()
     {
          // Manually process input events right before the physics update
          InputSystem.Update();

          if (isTouching)
          {
               // Handle the touch position here
               Debug.Log($"Touching at: {touchPosition}");
          }

          if (fixedUpdateCount < maxFixedUpdateSamples)
               fixedUpdateTimes[fixedUpdateCount] = Time.realtimeSinceStartup;

          fixedUpdateCount++;
          if (logEachFrame)
               Debug.Log("FixedUpdate count: " + fixedUpdateCount + " Frame: " + Time.frameCount);
     }

     private void OnEnable()
     {
          touchInputActions.Enable();
          touchInputActions.TouchControls.TouchPosition.performed += ctx => OnTouch(ctx);
          touchInputActions.TouchControls.TouchPosition.canceled += ctx => OnTouchCanceled(ctx);
     }

     private void OnDisable()
     {
          touchInputActions.Disable();
          touchInputActions.TouchControls.TouchPosition.performed -= ctx => OnTouch(ctx);
          touchInputActions.TouchControls.TouchPosition.canceled -= ctx => OnTouchCanceled(ctx);
     }

     private void OnTouch(InputAction.CallbackContext context)
     {
          PrintCurrentTimeStamp("Touched on:");
          isTouching = true;
          touchPosition = context.ReadValue<Vector2>();
          Debug.Log($"Touching at: {touchPosition}");

     }

     private void OnTouchCanceled(InputAction.CallbackContext context)
     {
          isTouching = false;
     }

     void Update()
     {
          updateCount++;
          if (logEachFrame)
               Debug.Log("Update count: " + updateCount + " Frame: " + Time.frameCount);
     }

     void LateUpdate()
     {
          lateUpdateCount++;
          if (logEachFrame)
               Debug.Log("LateUpdate count: " + lateUpdateCount + " Frame: " + Time.frameCount);
     }

     void OnGUI()
     {
          onGUICount++;
          if (logEachFrame)
               Debug.Log("OnGUI count: " + onGUICount + " Frame: " + Time.frameCount);
     }

     private IEnumerator CountUpdatesForOneSecond()
     {
          // Wait for 1 second
          yield return new WaitForSecondsRealtime(1f);

          // Output the counts
          Debug.LogWarning("FixedUpdate count: " + fixedUpdateCount);
          Debug.LogWarning("Update count: " + updateCount);
          Debug.LogWarning("LateUpdate count: " + lateUpdateCount);
          Debug.LogWarning("OnGUI count: " + onGUICount);

          //Set Maximums
          if (maxUpdateCount < updateCount)
          {
               maxUpdateCount = updateCount;
               maxUpdateText.text = maxUpdateCount.ToString();
          }
          if (maxLateUpdateCount < lateUpdateCount)
          {
               maxLateUpdateCount = lateUpdateCount;
               maxLateUpdateText.text = maxLateUpdateCount.ToString();
          }
          //if (maxGUICount < onGUICount)
          //{
          //     maxGUICount = onGUICount;
          //     maxGUIText.text = maxGUICount.ToString();
          //}

          fixedUpdateText.text = fixedUpdateCount.ToString();
          UpdateText.text = updateCount.ToString();
          lateUpdateText.text = lateUpdateCount.ToString();
          onGUIText.text = onGUICount.ToString();


          // Calculate delta times and standard deviation
          deltaTimes = new float[maxFixedUpdateSamples - 1];
          float sum = 0f;
          int length = 0;
          for (int i = 1; i < (maxFixedUpdateSamples - 1); i++)
          {
               deltaTimes[i - 1] = fixedUpdateTimes[i] - fixedUpdateTimes[i - 1];
               if (deltaTimes[i - 1] >= 0.0f)
               {
                    sum += deltaTimes[i - 1];
                    length++;
               }
          }

          float mean = sum / length;

          Debug.LogWarning("FixedUpdate Sum:" + sum * 1000.0f + "ms, Mean: " + mean * 1000.0f + "ms");
          float sumOfSquares = 0f;
          foreach (float delta in deltaTimes)
          {
               if (delta >= 0.0f)
                    sumOfSquares += Mathf.Pow(delta - mean, 2);
          }

          standardDeviation = Mathf.Sqrt(sumOfSquares / length);
          Debug.LogWarning("FixedUpdate Standard Deviation: " + standardDeviation);

          maxGUIText.text = standardDeviation.ToString("F5");

          // Reset the array size according to the current slider value
          maxFixedUpdateSamples = (int)rateSlider.value + 10;
          lastFrameUpdateTimes = fixedUpdateTimes;
          fixedUpdateTimes = new float[maxFixedUpdateSamples];
          ResetCounts();

          StartCoroutine(CountUpdatesForOneSecond());
     }

     private void ResetCounts()
     {
          fixedUpdateCount = 0;
          updateCount = 0;
          lateUpdateCount = 0;
          onGUICount = 0;
     }
     public void SetFixedUpdateRate(float rate)
     {
          if (rate > 0)
          {
               Time.fixedDeltaTime = 1.0f / rate;
               Debug.Log("FixedUpdate rate set to: " + rate + " calls per second");
          }
     }
     private void PrintCurrentTimeStamp(string preText="")
     {
          // Get the current system time with millisecond accuracy
          DateTime now = DateTime.Now;

          // Format the time string with millisecond precision
          string timeStamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");

          // Print the time stamp
          Debug.Log($"{preText} Current Time Stamp: {timeStamp}");
     }
}
