using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PanelManager
{
    public enum Layer
    {
        CommonPanel, // 底层，Hiearchy常存
        TipPanel, // 顶层，Hiearchy常存
    }

    // {自定义层级: 层级位置} 映射，保存面板之间的覆盖关系，覆盖效果由Unity Hiearchy实现
    private static Dictionary<Layer, Transform> layers = new Dictionary<Layer, Transform>();

    // {面板类型的字符串: 面板} 映射，保证每个类型只有一个面板，应该用 HashSet 改写
    public static Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();

    // Hierarchy
    public static Transform root;
    public static Transform canvas;

    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        Transform commonPanel = canvas.Find("CommonPanel");
        Transform tipPanel = canvas.Find("TipPanel");
        layers.Add(Layer.CommonPanel, commonPanel);
        layers.Add(Layer.TipPanel, tipPanel);
    }

    public static void CreatePanel<T>(params object[] args) where T : BasePanel
    {
        // 不允许重复打开同一面板
        string name = typeof(T).ToString();
        if (panels.ContainsKey(name))
        {
            return;
        }

        BasePanel panel = root.gameObject.AddComponent<T>(); // 挂载T的脚本到Root
        panel.OnInit();
        panel.LoadSkin();

        // 设置 panel 的 Hierarchy Parent 为 layerTransform 
        Transform layerTranform = layers[panel.layer];
        panel.skin.transform.SetParent(layerTranform, false);

        panels.Add(name, panel);

        panel.OnShow(args);
    }

    public static void RemovePanel(string name)
    {
        if (!panels.ContainsKey(name))
        {
            return;
        }

        BasePanel panel = panels[name];
        panel.OnClose();

        panels.Remove(name);

        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }
}
