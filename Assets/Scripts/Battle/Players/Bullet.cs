using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BaseTank shooter; // 发射者
    private GameObject skin;//炮弹模型
    Rigidbody rigidBody;

    public float speed = 220f;

    //初始化
    public void Init()
    {
        // 皮肤
        GameObject skinRes = ResManager.LoadPrefab("bulletPrefab");
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        // 物理
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
    }

    void Update()
    {
        // 向前移动
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        // 获取碰撞对象
        GameObject collObj = collisionInfo.gameObject;
        BaseTank hitTank = collObj.GetComponent<BaseTank>();
        // 不能打自己
        if (hitTank == shooter)
        {
            return;
        }

        if (hitTank != null) // 打中的是坦克
        {
            SendHitMsg(shooter, hitTank);
        }

        // 无论是否打中坦克，均显示爆炸效果，并摧毁自身
        // 爆炸效果不会做
        //GameObject explode = ResManager.LoadPrefab("fire");
        //Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // 发送伤害协议
    void SendHitMsg(BaseTank tank, BaseTank hitTank)
    {
        if (hitTank == null || tank == null)
        {
            return;
        }
        // 不是自己发出的炮弹
        if (tank.id != GameMain.id)
        {
            return;
        }

        MsgHit msg = new MsgHit();
        msg.targetId = hitTank.id;
        msg.id = tank.id;
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        NetManager.Send(msg);
    }
}
