using UnityEngine;
using System.Collections;

public abstract class BasePanel : MonoBehaviour
{
    public string skinPath;
    public GameObject skin;
    public PanelManager.Layer layer = PanelManager.Layer.CommonPanel;

    public void LoadSkin()
    {
        GameObject skinPrefab = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinPrefab);
    }

    public void Close()
    {
        string name = this.GetType().ToString();
        PanelManager.RemovePanel(name);
    }

    public abstract void OnInit();

    public abstract void OnShow(params object[] args);

    public abstract void OnClose();

}
