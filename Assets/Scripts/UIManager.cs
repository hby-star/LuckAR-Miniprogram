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

    public GameObject deleteObjectButton;
    public GameObject saveImageButton;
    public BackgroundClickSpawner spawner;
    
    public GameObject saveResultPopup;
    public TMPro.TMP_Text saveResultText;
    private CanvasGroup popupCanvasGroup;
    private Coroutine popupRoutine;
    public float fadeDuration = 0.4f; // 渐变时长
    public float displayTime = 0.1f;   // 停留时长
    
    // Start is called before the first frame update
    void Start()
    {
        popupCanvasGroup = saveResultPopup.GetComponent<CanvasGroup>();
        if (popupCanvasGroup == null)
        {
            popupCanvasGroup = saveResultPopup.AddComponent<CanvasGroup>();
        }
        saveResultPopup.SetActive(false);
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
            sourceType = new[] { "album", "camera" },
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
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.preserveAspect = true;
    }
    
    public void OnTouchSaveImage()
    {
        string imagePath = GetSaveImageFromCamera();
        TT.SaveImageToPhotosAlbum(new TTSaveImageToPhotosAlbumParam
        {
            filePath = imagePath,
            success = (result) =>
            {
                string resultOutput = "Success, message: " + result;
                Debug.Log(resultOutput);
                ShowPopup("图片保存成功");
            },
            fail = (msg) =>
            {
                Debug.Log("Fail, message: " + msg);
                ShowPopup("图片保存失败");
            },
            complete = () =>
            {
                Debug.Log("Complete");
            }
        });
    }

    private string GetSaveImageFromCamera()
    {
        deleteObjectButton.SetActive(false);
        saveImageButton.SetActive(false);

        var mainCamera = Camera.main;
        int width = Screen.width;
        int height = Screen.height;

        // 渲染到RT
        RenderTexture rt = new RenderTexture(width, height, 24);
        mainCamera.targetTexture = rt;
        mainCamera.Render();

        // 计算RectTransform对应的屏幕Rect
        Vector3[] corners = new Vector3[4];
        RectTransform targetRect = backgroundImage.rectTransform;
        targetRect.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[2]);

        Rect screenRect = new Rect(
            min.x,
            min.y,
            max.x - min.x,
            max.y - min.y
        );

        // 创建Texture2D并读取区域
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D((int)screenRect.width, (int)screenRect.height, TextureFormat.RGB24, false);
        tex.ReadPixels(screenRect, 0, 0);
        tex.Apply();

        // 清理
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 保存到TTSDK文件系统
        byte[] pngData = tex.EncodeToPNG();
        string base64Str = System.Convert.ToBase64String(pngData);

        string filePath = $"ttfile://user/capture_{System.DateTime.Now.Ticks}.png";

        var fileSystemManager = TT.GetFileSystemManager();
        fileSystemManager.WriteFileSync(filePath, base64Str, "base64");

        // 恢复按钮
        saveImageButton.SetActive(true);
        if (spawner.currentModelIndex != -1)
        {
            deleteObjectButton.SetActive(true);
        }

        return filePath;
    }

    private void ShowPopup(string message)
    {
        saveResultText.text = message;

        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
        }
        popupRoutine = StartCoroutine(ShowAndHidePopup());
    }

    private IEnumerator ShowAndHidePopup()
    {
        saveResultPopup.SetActive(true);

        // 渐显
        yield return StartCoroutine(FadeCanvasGroup(0f, fadeDuration));
        
        // 停留
        //yield return new WaitForSeconds(displayTime);

        // 渐隐
        //yield return StartCoroutine(FadeCanvasGroup(fadeDuration, 0f));

        saveResultPopup.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            popupCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        popupCanvasGroup.alpha = to;
    }
}
