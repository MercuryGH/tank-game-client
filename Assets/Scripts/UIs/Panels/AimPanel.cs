using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 自动瞄准控件
public class AimPanel : BasePanel
{
    CtrlTank tank;
    private Image aimImage;

    public override void OnInit()
    {
        skinPath = "AimPanel";
        layer = PanelManager.Layer.CommonPanel;
    }
    public override void OnShow(params object[] args)
    {
        // 准星UI
        aimImage = skin.transform.Find("Image").GetComponent<Image>();

        tank = (CtrlTank)BattleManager.GetCtrlTank();
    }

    public override void OnClose()
    {

    }

    public void Update()
    {
        if (tank == null)
        {
            return;
        }

        // 3D坐标
        Vector3 point = tank.getAimedDirection();
        // 屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(point);
        // UI坐标
        aimImage.transform.position = screenPoint;
    }
}
