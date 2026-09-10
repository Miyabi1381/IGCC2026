// ======================================================================================
// File: StageTrigger.cs
//
// Brief: ステージ切り替えのトリガー用クラス
//
// Author: Banno Miyabi
// Date: 2026/09/10
// ======================================================================================
using UnityEngine;
using Unity.Cinemachine;

namespace MiyaLib
{
    public class StageTrigger : MonoBehaviour
    {
        // ライフサイクル関数 -----------------------------------------------------------
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only react to actual room-boundary triggers — ignore pickups, enemies,
            // or anything else with a trigger collider the player might walk into.
            if (!collision.CompareTag("RoomBounds")) return;

            if (collision.TryGetComponent<Collider2D>(out Collider2D roomCollider))
            {
                // Fetch the confiner at runtime instead of a serialized field —
                // prefabs can't hold a direct reference to a scene-only object like the camera.
                CinemachineConfiner2D cameraConfiner = DeathManager.Instance != null
                    ? DeathManager.Instance.GetCameraConfiner()
                    : null;

                if (cameraConfiner == null)
                {
                    Debug.LogWarning("StageTrigger: No camera confiner found via DeathManager.");
                    return;
                }

                // カメラの制限エリアを、今入ったステージのコライダーに上書きする
                cameraConfiner.BoundingShape2D = roomCollider;

                // 過去のキャッシュをクリアして、新しい部屋への移動を開始させる
                cameraConfiner.InvalidateBoundingShapeCache();
            }
        }

        // 公開メンバ関数 ---------------------------------------------------------------
    }
}