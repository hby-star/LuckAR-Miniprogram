using System.Collections;
using System.Collections.Generic;
using System.IO;
using TTSDK;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image backgroundImage;
    public GameObject addImageButton;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTouchAddImage()
    {
        TT.ChooseImage(new TTChooseImageParam
        {
            count = 1,
            sourceType = new[] { "album" },
            success = (result) =>
            {
                string resultOutput = "Choose Message Success, message: " + result.errMsg;
                Debug.Log(resultOutput);
                addImageButton.SetActive(false);
                SetBackground(result.tempFilePaths[0]);
            },
            fail = (msg) =>
            {
                Debug.Log("Choose Message Fail, message: " + msg);
            },
            complete = () =>
            {
                Debug.Log("Choose Message Complete");
            }
        });
    }
    
    public void SetBackground(string filePath)
    {
        var fileSystemManager = TT.GetFileSystemManager();
        
        if (fileSystemManager == null || !fileSystemManager.AccessSync(filePath))
        {
            Debug.LogError("文件系统管理器不可用或文件不存在");
            return;
        }
        
        // 读取文件字节
        byte[] fileData = fileSystemManager.ReadFileSync(filePath);
        
        // 创建Texture2D
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false); 
        tex.LoadImage(fileData);
        
        // 转Sprite
        Sprite newSprite = Sprite.Create(
            tex, 
            new Rect(0, 0, tex.width, tex.height), 
            new Vector2(0.5f, 0.5f)
        );
        
        // 设置到UI Image上
        backgroundImage.sprite = newSprite;
        
        // 拉伸填充
        backgroundImage.preserveAspect = true;
    }
}
