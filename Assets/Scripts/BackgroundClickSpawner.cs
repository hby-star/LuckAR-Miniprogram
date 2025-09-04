using Lean.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using Unity.VisualScripting;

public class BackgroundClickSpawner : MonoBehaviour, IPointerClickHandler
{
    [Header("要生成的3D模型 Prefab")]
    public GameObject prefab;

    [Header("生成的父物体")]
    public Transform parent; // 一般设为一个空物体，比如 "SpawnRoot"

    [Header("主摄像机")]
    public Camera mainCamera;

    [Header("射线检测层 (模型层)")]
    public LayerMask selectableLayer = -1;

    private LeanSelectable currentSelection;

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 screenPos = eventData.position;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, selectableLayer))
        {
            // 点击到了已有物体
            var selectable = hit.collider.GetComponentInParent<LeanSelectable>();
            if (selectable != null)
            {
                SelectObject(selectable);
                return;
            }
        }

        // 没点到 → 在背景生成新物体
        SpawnNew(screenPos);
    }

    private void SpawnNew(Vector2 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 5f) // 距相机 5 单位
        );

        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, parent);

        // 添加 LeanSelectable 组件
        var selectable = obj.GetComponent<LeanSelectable>();
        if (selectable == null) selectable = obj.AddComponent<LeanSelectable>();

        // 给物体自动加 Lean Touch 控件
        if (obj.GetComponent<LeanDragTranslate>() == null) obj.AddComponent<LeanDragTranslate>();
        if (obj.GetComponent<LeanPinchScale>() == null) obj.AddComponent<LeanPinchScale>();
        if (obj.GetComponent<LeanTwistRotate>() == null) obj.AddComponent<LeanTwistRotate>();
        
        // 给物体自动加 Outline 控件
        if (obj.GetComponent<Outline>() == null)
        {
            var outline = obj.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.cyan;
            outline.OutlineWidth = 10f;
        }
        

        // 自动选中新生成的物体
        SelectObject(selectable);
    }

    private void SelectObject(LeanSelectable selectable)
    {
        if (currentSelection != null)
        {
            currentSelection.Deselect();
            
            // 移除高亮效果
            currentSelection.GetComponent<Outline>().enabled = false;
        }

        currentSelection = selectable;
        currentSelection.SelfSelected = true;
        
        // 添加高亮效果
        currentSelection.GetComponent<Outline>().enabled = true;
        

        Debug.Log("选中了: " + selectable.name);
    }
}
