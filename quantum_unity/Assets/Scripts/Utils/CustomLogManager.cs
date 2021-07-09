using System.Collections;
using System.Collections.Generic;
using Quantum.Utils;
using UnityEngine;

public class CustomLogManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      DontDestroyOnLoad(this);
      UnitySystemConsoleRedirector.OnDebug += OnDebugFromDLL;
    }
    
    void OnDebugFromDLL(string message) {
      Debug.Log(message);
    }
}
