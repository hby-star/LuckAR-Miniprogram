using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTSDK;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        TT.EnableTTSDKDebugToast = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        MockSetting.OpenAllMockModule();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
