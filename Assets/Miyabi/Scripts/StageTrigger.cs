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
		// メンバ変数 -------------------------------------------------------------------
		[Tooltip("main camera is here")]
		[SerializeField] private CinemachineConfiner2D cameraConfiner;
		
		// ライフサイクル関数 -----------------------------------------------------------
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Collider2D>(out Collider2D roomCollider))
            {
                // カメラの制限エリアを、今入ったステージのコライダーに上書きする
                cameraConfiner.BoundingShape2D = roomCollider;

				// 過去のキャッシュをクリアして、新しい部屋への移動を開始させる
				cameraConfiner.InvalidateBoundingShapeCache();
            }

        }

        // 公開メンバ関数 ---------------------------------------------------------------
    }
}