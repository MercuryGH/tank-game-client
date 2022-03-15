using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    private GameObject skin; // 坦克模型
    protected Rigidbody rigidBody;

    public float steer = 30; // 转向速度
    public float speed = 6f; // 移动速度
    public float turretSpeed = 30f; // 炮塔旋转速度
    public Transform turret; // 炮塔
    public Transform gun; // 炮管
    public Transform firePoint; // 发射点
    public float fireCd = 0.5f; // 炮弹CD时间
    public float lastFireTime = 0; // 上一次发射炮弹的时间
    public float hp = 100; // 生命值
    public string id = ""; // 属于哪一名玩家
    public int team = 0; // 阵营

    // 炮管旋转
    public float minGunAngle = -20;
    public float maxGunAngle = 20;
    public float gunSpeed = 4f;

    // 轮子和履带
    public Transform wheels;
    public Transform track;

    // Use this for initialization
    public void Start()
    {

    }

    public virtual void Init(string skinPath)
    {
        // 皮肤
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        // 物理
        rigidBody = gameObject.AddComponent<Rigidbody>();
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, 2.5f, 1.47f);
        boxCollider.size = new Vector3(7, 5, 12);
        // 炮塔炮管
        turret = skin.transform.Find("Turret");
        gun = turret.transform.Find("Gun");
        firePoint = gun.transform.Find("FirePoint");
        // 轮子履带
        wheels = skin.transform.Find("Wheels");
        track = skin.transform.Find("Track");
    }

    public bool IsDie()
    {
        return hp <= 0;
    }

    // 受到 dmg 的伤害
    public void Attacked(float dmg)
    {
        if (IsDie())
        {
            return;
        }
        hp -= dmg;

        // 经过这一击就死了
        if (IsDie())
        {
            // 显示焚烧效果
            GameObject obj = ResManager.LoadPrefab("explosion");
            GameObject explosion = Instantiate(obj, transform.position, transform.rotation);
            explosion.transform.SetParent(transform);
        }
    }

    public void Update()
    {

    }

    //轮子旋转，履带滚动
    public void WheelUpdate(float axis)
    {
        //计算速度
        float v = Time.deltaTime * speed * axis * 100;
        //旋转每个轮子
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(new Vector3(v, 0, 0), Space.Self);
        }
        //滚动履带
        MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            return;
        };
        Material mtl = mr.material;
        mtl.mainTextureOffset += new Vector2(0, v / 256);
    }

    protected Bullet Fire()
    {
        if (IsDie())
        {
            return null;
        }

        // 产生炮弹
        GameObject bulletObj = new GameObject("bullet")
        {
            layer = LayerMask.NameToLayer("Bullet")
        };
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init();
        bullet.shooter = this;
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        // 更新CD
        lastFireTime = Time.time;
        return bullet;
    }
}
