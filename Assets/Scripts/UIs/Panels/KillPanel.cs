using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillPanel : BasePanel
{
    private float startTime = 0;

    public override void OnInit()
    {
        skinPath = "KillPanel";
        layer = PanelManager.Layer.TipPanel;
    }

    public override void OnShow(params object[] args)
    {
        startTime = Time.time;
    }

    public override void OnClose()
    {

    }

    public void Update()
    {
        if (Time.time - startTime > 2f)
        {
            Close();
        }
    }
}
