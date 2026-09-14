using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public Vector2 position; // ローカル位置 Local position
    public Vector3 eulerAngles;// ローカル回転角度 Local rotation angles
}

public class Minecart : MonoBehaviour
{
    [Header("レールの経路設定 Rail path settings")]
    public List<Waypoint> waypoints = new List<Waypoint>(); // ウェイポイントのリスト List of waypoints

    [Header("移動設定 Movement settings")]
    [SerializeField] private float maxSpeed = 10f;   // 最高速度 Max speed
    [SerializeField] private float acceleration = 5f; // 加速度 Acceleration

    private int currentIndex = 0;   // 現在の目標ウェイポイントのインデックス Current target waypoint index
    private bool isMoving = false;   // 移動中かどうかのフラグ Flag indicating whether the minecart is moving
    private float currentSpeed = 0f;       // 現在の速度 Current speed

    // 乗っているプレイヤーを記憶する変数 Passenger memory variables
    private Transform passenger = null;
    private Rigidbody2D passengerRb = null;
    private Collider2D passengerCollider = null;

    // 摩擦を管理する変数 Friction management variables
    private PhysicsMaterial2D originalMaterial = null;
    private PhysicsMaterial2D gripMaterial = null;

    // ゲーム中に実際に使用するワールド座標のリスト List of absolute world coordinates used during gameplay
    private List<Waypoint> worldWaypoints = new List<Waypoint>();

    void Start()
    {
        // ゲーム開始の瞬間に、ローカル座標をワールド座標に変換して固定する
        foreach (var wp in waypoints)
        {
            Waypoint absoluteWp = new Waypoint();
            absoluteWp.position = transform.TransformPoint(wp.position);
            absoluteWp.eulerAngles = (transform.rotation * Quaternion.Euler(wp.eulerAngles)).eulerAngles;
            worldWaypoints.Add(absoluteWp);
        }

        // コード上で強力な滑り止めマテリアルを作成しておく
        gripMaterial = new PhysicsMaterial2D("MinecartGrip");
        gripMaterial.friction = 10f;
        gripMaterial.bounciness = 0f;
    }

    void FixedUpdate()
    {
        // 移動が存在しない場合は処理をスキップ
        if (!isMoving || worldWaypoints.Count == 0 || currentIndex >= worldWaypoints.Count) return;

        // 移動する「前」の位置を記憶
        Vector3 previousPosition = transform.position;

        // 現在の目標ポイントを取得
        // Get the current target waypoint
        Waypoint target = worldWaypoints[currentIndex];

        // トロッコの加速処理
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
            if (currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }
        }

        // 現在の速度(currentSpeed)で目標ポイントへ向かって移動
        transform.position = Vector3.MoveTowards(transform.position, target.position, currentSpeed * Time.fixedDeltaTime);

        // トロッコが動いた差分を計算し、プレイヤーに足す
        if (passenger != null)
        {
            Vector2 deltaPosition = transform.position - previousPosition;

            // プレイヤーがRigidbody2Dを持っている場合は、物理演算の座標に直接足すことでガタつきを無くす
            if (passengerRb != null)
            {
                passengerRb.position += deltaPosition;
            }
            else
            {
                passenger.position += (Vector3)deltaPosition;
            }
        }

        // 目標ポイントに到達したら、次のポイントへ切り替えつつ角度を変える
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            // 到達した瞬間に位置をぴったり合わせ、そのポイントに設定された角度へ変更する
            transform.position = target.position;
            transform.rotation = Quaternion.Euler(target.eulerAngles);

            currentIndex++;
        }
    }

    // プレイヤーがトロッコに乗った時の処理
    // When the player gets on the minecart
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーを子オブジェクトにせず、変数として記憶する
            passenger = collision.transform;
            passengerRb = collision.collider.attachedRigidbody;
            passengerCollider = collision.collider;

            // プレイヤーの摩擦を一時的に「強力な滑り止め」に差し替えて、斜面でのずり落ちを防ぐ
            if (passengerCollider != null)
            {
                originalMaterial = passengerCollider.sharedMaterial;
                passengerCollider.sharedMaterial = gripMaterial;
            }

            // トロッコを発車させる
            isMoving = true;
        }
    }

    // プレイヤーがトロッコから降りた時の処理
    // When the player leaves the minecart
    private void OnCollisionExit2D(Collision2D collision)
    {
        // トロッコが非アクティブの場合は処理をスキップ
        if (!this.gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーが離れたら摩擦を元に戻す
            if (passenger == collision.transform)
            {
                if (passengerCollider != null)
                {
                    passengerCollider.sharedMaterial = originalMaterial;
                }

                passenger = null;
                passengerRb = null;
                passengerCollider = null;
            }
        }
    }

    // トロッコを初期位置に戻す処理
    // Reset the minecart to its initial position
    public void ResetMinecart()
    {
        // トロッコを初期位置に戻す
        if (worldWaypoints.Count > 0)
        {
            transform.position = worldWaypoints[0].position;
            transform.rotation = Quaternion.Euler(worldWaypoints[0].eulerAngles);
            currentIndex = 0;
            isMoving = false;

            // 速度と乗客もクリア
            currentSpeed = 0f;

            // リセット時にもプレイヤーの摩擦を元に戻す
            if (passengerCollider != null)
            {
                passengerCollider.sharedMaterial = originalMaterial;
            }

            passenger = null;
            passengerRb = null;
            passengerCollider = null;
        }
    }
}

// エディター側をカスタムする
// Customize the editor side
#if UNITY_EDITOR
[CustomEditor(typeof(Minecart))]
public class MinecartEditor : Editor
{
    private Minecart route;

    // Inspectorが有効になったときに呼ばれる
    private void OnEnable()
    {
        route = (Minecart)target;
    }

    // Inspector上でのGUI描画
    // GUI rendering in Inspector
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Add point", GUILayout.Height(25)))
        {
            Undo.RecordObject(route, "Add Waypoint");
            // 追加時の初期位置を (0,0) にすることで、オブジェクトの中心が原点になる
            route.waypoints.Add(new Waypoint
            {
                position = Vector2.zero,
                eulerAngles = Vector3.zero
            });
        }
        GUI.backgroundColor = Color.white;
    }

    // Sceneビュー上でのGUI描画
    // GUI rendering in Scene view
    protected virtual void OnSceneGUI()
    {
        // ウェイポイントが存在しない場合は何も描画しない
        if (route.waypoints == null || route.waypoints.Count == 0) return;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.fontSize = 14;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        // 各ウェイポイントに対してハンドルを描画
        // Draw handles for each waypoint
        for (int i = 0; i < route.waypoints.Count; i++)
        {
            // ローカル座標をワールド座標に変換してハンドルを表示
            Vector2 worldPos = route.transform.TransformPoint(route.waypoints[i].position);
            Quaternion worldRot = route.transform.rotation * Quaternion.Euler(route.waypoints[i].eulerAngles);

            // ラベル表示
            // Display label
            Vector2 labelPos = worldPos + Vector2.down * 0.4f;
            Handles.Label(labelPos, $"Point {i}", labelStyle);

            // 向く方向を示す黄色い矢印を描画
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, worldPos, worldRot, 1.5f, EventType.Repaint);

            EditorGUI.BeginChangeCheck();

            // 移動ハンドル
            Vector3 newWorldPosition = Handles.PositionHandle(worldPos, worldRot);
            // 回転ハンドル
            Quaternion newWorldRotation = Handles.RotationHandle(worldRot, worldPos);

            // 変更があった場合、Undoを記録してウェイポイントを更新
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(route, "Modify Waypoint");
                // 動かしたワールド座標を、オブジェクト基準のローカル座標に戻して保存
                route.waypoints[i].position = route.transform.InverseTransformPoint(newWorldPosition);
                route.waypoints[i].eulerAngles = (Quaternion.Inverse(route.transform.rotation) * newWorldRotation).eulerAngles;
            }
        }
    }
}
#endif