using System.Collections;
using UnityEngine;
using TMPro;

public class UpdateCounter : MonoBehaviour
{
     private int fixedUpdateCount = 0;
     private int updateCount = 0;
     private int lateUpdateCount = 0;
     private int onGUICount = 0;
     private int pressedCount = 0;

     bool fixedUpdateRun = false;
     bool updateRun = false;
     bool lateUpdateRun = false;

     [SerializeField]
     bool logEachFrame = false;

     //input 4 TextMeshPro - Text
     public TextMeshPro fixedUpdateText;
     public TextMeshPro UpdateText;
     public TextMeshPro lateUpdateText;
     public TextMeshPro onGUIText;
     void Start()
     {
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
}
