using System;
using System.Collections.Generic;
using Lean.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using Unity.VisualScripting;

public class BackgroundClickSpawner : MonoBehaviour, IPointerClickHandler
{
    [Header("模型库")]
    public List<GameObject> models;

    public List<GameObject> modelSelectBoxes;

    public int currentModelIndex = -1;
    
    [Header("颜色库-去除")]
    public List<Color> colors;
    
    public List<GameObject> colorSelectBoxes;

    public int currentColorIndex = -1;

    [Header("其他设置")] public GameObject deleteButton;
    
    public Transform spawnRoot; // 一般设为一个空物体，比如 "SpawnRoot"

    public Camera mainCamera;

    public LayerMask selectableLayer = -1;

    private GameObject _currentSelection;

    private void Start()
    {
        deleteButton.SetActive(false);
        
        foreach (var modelSelect in modelSelectBoxes)
        {
            modelSelect.SetActive(false);
        }

        // foreach (var colorSelect in colorSelectBoxes)
        // {
        //     colorSelect.SetActive(false);
        // }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 screenPos = eventData.position;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, selectableLayer))
        {
            // 点击到了已有物体
            var obj = hit.collider.gameObject;
            if (obj != null)
            {
                SelectObject(obj.transform.parent.gameObject);
                return;
            }
        }

        // 没点到 → 在背景生成新物体
        // if(currentModelIndex < 0 || currentColorIndex < 0)
        if(currentModelIndex < 0)
        {
            DeSelectAll();
            Debug.Log("请先选择物体");
            return;
        }
        
        SpawnNew(screenPos);
    }

    private void SpawnNew(Vector2 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 10f) // 距相机 5 单位
        );

        GameObject obj = Instantiate(models[currentModelIndex], worldPos, Quaternion.Euler(20f, 180f, 0f), spawnRoot);
        obj.transform.localScale = Vector3.one * 2f;
        MeshRenderer objRenderer = obj.GetComponent<MeshRenderer>();
        //objRenderer.material.color = colors[currentColorIndex];
        OnSelectModel(-1);
        //OnSelectColor(-1);

        // 给物体自动加 Lean Touch 控件
        if (obj.GetComponent<LeanSelectableByFinger>() == null) obj.AddComponent<LeanSelectableByFinger>();
        if (obj.GetComponent<LeanDragTranslate>() == null) obj.AddComponent<LeanDragTranslate>();
        if (obj.GetComponent<LeanPinchScale>() == null) obj.AddComponent<LeanPinchScale>();
        if (obj.GetComponent<LeanTwistRotateAxis>() == null) obj.AddComponent<LeanTwistRotateAxis>();
        

        // 自动选中新生成的物体
        SelectObject(obj);
    }

    private void SelectObject(GameObject obj)
    {
        deleteButton.SetActive(true);
        var buttonComponent = deleteButton.GetComponent<UnityEngine.UI.Button>();
        buttonComponent.onClick.AddListener(OnClickDelete);
        
        if (_currentSelection != null)
        {
            // 移除之前选中的物体的选中状态
            var selectable = _currentSelection.GetComponent<LeanSelectableByFinger>();
            selectable.Deselect();
            
            // 移除高亮效果
            _currentSelection.GetComponent<Outline>().enabled = false;
        }

        _currentSelection = obj;
        
        // 选中新的物体
        var newSelectable = _currentSelection.GetComponent<LeanSelectableByFinger>();
        newSelectable.SelfSelected = true;
        
        // 添加高亮效果
        _currentSelection.GetComponent<Outline>().enabled = true;
        

        Debug.Log("选中了: " + obj.name);
    }
    
    private void DeSelectAll()
    {
        deleteButton.SetActive(false);
        var buttonComponent = deleteButton.GetComponent<UnityEngine.UI.Button>();
        buttonComponent.onClick.RemoveListener(OnClickDelete);
        
        if (_currentSelection != null)
        {
            var selectable = _currentSelection.GetComponent<LeanSelectableByFinger>();
            selectable.Deselect();
            _currentSelection.GetComponent<Outline>().enabled = false;
            _currentSelection = null;
        }
    }

    public void OnSelectModel(int index)
    {
        currentModelIndex = index;

        for (int i = 0; i < modelSelectBoxes.Count; i++)
        {
            if (i == index)
            {
                modelSelectBoxes[i].SetActive(true);
            }
            else
            {
                modelSelectBoxes[i].SetActive(false);
            }
        }
    }

    public void OnSelectColor(int index)
    {
        currentColorIndex = index;

        for (int i = 0; i < colorSelectBoxes.Count; i++)
        {
            if (i == index)
            {
                colorSelectBoxes[i].SetActive(true);
            }
            else
            {
                colorSelectBoxes[i].SetActive(false);
            }
        }
    }
    
    public void OnClickDelete()
    {
        if (_currentSelection != null)
        {
            Destroy(_currentSelection);
            _currentSelection = null;
        }
        DeSelectAll();
    }
}
