using UnityEngine;

public class camera_move : MonoBehaviour
{
    public Transform target;  // カメラが回る中心点
    public float speed = 30f; // 角速度（度/秒）
    public float angleLimit = 90f; // 移動する角度の範囲

    private float radius; // 円弧の半径
    private float angle = 0f;
    private int direction = 1; // 移動方向（1:時計回り, -1:反時計回り）
    private float camera_y;

    void Start()
    {
        if (target != null)
        {
            // カメラの初期Y座標を保持
            camera_y = transform.position.y;

            // ターゲットとカメラの水平距離（XZ 平面上）を計算して半径とする
            Vector3 targetXZ = new Vector3(target.position.x, 0, target.position.z);
            Vector3 cameraXZ = new Vector3(transform.position.x, 0, transform.position.z);
            radius = Vector3.Distance(targetXZ, cameraXZ);

            // 初期角度を計算（カメラの現在位置から）
            Vector3 offset = cameraXZ - targetXZ;
            angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // 角度を更新
            angle += direction * speed * Time.deltaTime;
            
            // 指定した角度範囲を超えたら方向を反転
            if (Mathf.Abs(angle) > angleLimit)
            {
                direction *= -1;
            }

            // 新しい位置を計算
            float radian = angle * Mathf.Deg2Rad;
            Vector3 newPosition = target.position + new Vector3(-1.0f*Mathf.Cos(radian) * radius, 0.3f, -1.0f*Mathf.Sin(radian) * radius);

            // カメラの位置を更新
            transform.position = newPosition;

            // ターゲットを向くように回転
            transform.LookAt(target);
        }
    }
}
