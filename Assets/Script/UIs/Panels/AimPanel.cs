using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimPanel : BasePanel
{
    CtrlTank tank;
    private Image aimImage;
    //初始化
    public override void OnInit()
    {
        skinPath = "AimPanel";
        layer = PanelManager.Layer.CommonPanel;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        //寻找组件
        aimImage = skin.transform.Find("Image").GetComponent<Image>();

        tank = (CtrlTank)BattleManager.GetCtrlTank();
    }

    //关闭
    public override void OnClose()
    {

    }

    //当按下确定按钮
    public void Update()
    {
        if (tank == null)
        {
            return;
        }
        //3D坐标
        Vector3 point = tank.ForecastExplodePoint();
        //屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(point);
        //UI坐标
        aimImage.transform.position = screenPoint;
    }
}
