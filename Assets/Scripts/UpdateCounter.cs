using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
     void Start()
     {
          if (rateSlider != null)
          {
               // Set an initial value for the slider (optional)
               rateSlider.value = 50f; // For example, set it to 50 FPS
                                       // Attach the listener to the slider
               rateSlider.onValueChanged.AddListener(SetFixedUpdateRate);
          }
          else
          {
               Debug.LogError("Rate Slider not assigned!");
          }
          // Start the counting coroutine
          StartCoroutine(CountUpdatesForOneSecond());
     }

     void FixedUpdate()
     {
          fixedUpdateCount++;
          if (logEachFrame)
               Debug.Log("FixedUpdate count: " + fixedUpdateCount + " Frame: " + Time.frameCount);
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
          yield return new WaitForSeconds(1f);

                    // Output the counts
          Debug.LogWarning("FixedUpdate count: " + fixedUpdateCount);
          Debug.LogWarning("Update count: " + updateCount);
          Debug.LogWarning("LateUpdate count: " + lateUpdateCount);
          Debug.LogWarning("OnGUI count: " + onGUICount);

          //Set Maximums
          if(maxUpdateCount < updateCount)
          {
               maxUpdateCount = updateCount;
               maxUpdateText.text = maxUpdateCount.ToString();
          }
          if (maxLateUpdateCount < lateUpdateCount)
          {
               maxLateUpdateCount = lateUpdateCount;
               maxLateUpdateText.text = maxLateUpdateCount.ToString();
          }
          if (maxGUICount < onGUICount)
          {
               maxGUICount = onGUICount;
               maxGUIText.text = maxGUICount.ToString();
          }

          fixedUpdateText.text = fixedUpdateCount.ToString();
          UpdateText.text = updateCount.ToString();
          lateUpdateText.text = lateUpdateCount.ToString();
          onGUIText.text = onGUICount.ToString();

          // Optionally, you can reset the counts and start counting again
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
}
