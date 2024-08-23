using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject cube;
     private void Start()
     {
          StartCoroutine(ToggleCubeEvery3Seconds());
     }
      void ToggleCube()
     {
          cube.SetActive(!cube.activeSelf);
     }
     IEnumerator ToggleCubeEvery3Seconds()
     {
          while (true)
          {
               yield return new WaitForSeconds(1);
               ToggleCube();
          }
     }
}
