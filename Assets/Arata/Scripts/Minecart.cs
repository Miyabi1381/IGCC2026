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
    [SerializeField] private float speed = 5f;  // 移動速度 Movement speed
    [SerializeField] private float rotationSpeed = 180f;    // 回転速度 Rotation speed

    private int currentIndex = 0;   // 現在の目標ウェイポイントのインデックス Current target waypoint index
    private bool isMoving = false;   // 移動中かどうかのフラグ Flag indicating whether the minecart is moving
    private Transform passenger = null;    // 乗っているプレイヤーを記憶する変数 Passenger memory variable

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
    }

    void Update()
    {
        // ウェイポイントが存在しない場合や移動が無効な場合はスキップ
        if (!isMoving || worldWaypoints.Count == 0 || currentIndex >= worldWaypoints.Count) return;

        // 移動する前の位置を記憶
        Vector3 previousPosition = transform.position;

        // 現在の目標ポイントを取得
        Waypoint target = worldWaypoints[currentIndex];

        // 常に同じ速度で目標ポイントへ向かって移動
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 目標の角度へ向かってトロッコを回転させる処理
        Quaternion targetRotation = Quaternion.Euler(target.eulerAngles);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // トロッコが動いただけプレイヤーに足す
        if (passenger != null)
        {
            Vector3 deltaPosition = transform.position - previousPosition;
            passenger.position += deltaPosition;
        }

        // 目標ポイントにほぼ到達したら、次のポイントへ切り替え
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
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
            // プレイヤーが離れたら記憶を消す
            if (passenger == collision.transform)
            {
                passenger = null;
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

            // リセット時に乗客の記憶もクリア
            passenger = null;
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
            // Position handle
            Vector3 newWorldPosition = Handles.PositionHandle(worldPos, worldRot);
            // 回転ハンドル
            // Rotation handle
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