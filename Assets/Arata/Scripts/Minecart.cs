using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Waypoint
{
    public Vector2 position; // ワールド位置 World position
    public Vector3 eulerAngles;// 回転角度 Rotation angles
}

public class Minecart : MonoBehaviour
{
    [Header("レールの経路設定 Rail path settings")]
    public List<Waypoint> waypoints = new List<Waypoint>(); // ウェイポイントのリスト List of waypoints
    [SerializeField] private float speed = 5f;  // 移動速度 Movement speed
    [SerializeField] private float rotationSpeed = 180f;    // 回転速度 Rotation speed

    private int currentIndex = 0;   // 現在の目標ウェイポイントのインデックス Current target waypoint index
    private bool isMoving = false;   // 移動中かどうかのフラグ Flag indicating whether the minecart is moving

    void Update()
    {
        // ウェイポイントが存在しない場合や移動が無効な場合は処理をスキップ
        if (!isMoving || waypoints.Count == 0 || currentIndex >= waypoints.Count) return;

        // 現在の目標ポイントを取得
        Waypoint target = waypoints[currentIndex];

        // 常に同じ速度で目標ポイントへ向かって移動（ワールド座標として扱う）
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 目標の角度へ向かってトロッコを回転させる処理
        Quaternion targetRotation = Quaternion.Euler(target.eulerAngles);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

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
            // プレイヤーをトロッコの子オブジェクトにして一緒に動かす
            collision.transform.SetParent(transform);

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
            // 親子関係を解除してプレイヤーを独立させる
            collision.transform.SetParent(null);
        }
    }

    // トロッコを初期位置に戻す処理
    // Reset the minecart to its initial position
    public void ResetMinecart()
    {
        // トロッコを初期位置に戻す
        if (waypoints.Count > 0)
        {
            transform.position = waypoints[0].position;
            transform.rotation = Quaternion.Euler(waypoints[0].eulerAngles);
            currentIndex = 0;
            isMoving = false;
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
    // Called when the Inspector is enabled
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
            route.waypoints.Add(new Waypoint
            {
                position = route.transform.position,
                eulerAngles = route.transform.eulerAngles
            });
        }
        GUI.backgroundColor = Color.white;
    }

    // Sceneビュー上でのGUI描画
    // GUI rendering in Scene view
    protected virtual void OnSceneGUI()
    {
        // ウェイポイントが存在しない場合は何も描画しない
        // Do nothing if there are no waypoints
        if (route.waypoints == null || route.waypoints.Count == 0) return;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.fontSize = 14;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        // 各ウェイポイントに対してハンドルを描画
        for (int i = 0; i < route.waypoints.Count; i++)
        {
            // ワールド座標・回転をそのまま取得（ローカル変換を廃止）
            Vector2 worldPos = route.waypoints[i].position;
            Quaternion worldRot = Quaternion.Euler(route.waypoints[i].eulerAngles);

            // ラベル表示
            // Display label
            Vector2 labelPos = worldPos + Vector2.down * 0.4f;
            Handles.Label(labelPos, $"Point {i}", labelStyle);

            // 向く方向を示す黄色い矢印を描画
            // Draw a yellow arrow indicating the forward direction
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
                // 変更された座標・回転をワールド座標のまま保存
                route.waypoints[i].position = newWorldPosition;
                route.waypoints[i].eulerAngles = newWorldRotation.eulerAngles;
            }
        }
    }
}
#endif