// ======================================================================================
// File: CinemachineTagFollower.cs
//
// Brief: Cinemachineで追尾対象のタグを検索するクラス
//
// Author: Banno Miyabi
// Date: 2026/09/10
// ======================================================================================
using UnityEngine;
using Unity.Cinemachine;

namespace MiyaLib
{
	public class CinemachineTagFollower : MonoBehaviour
	{
		// メンバ変数 -------------------------------------------------------------------
		[Tooltip("Cinemachine Camera is here")]
		private CinemachineCamera cCamera;
		
		// ライフサイクル関数 -----------------------------------------------------------
		private void Start()
		{
			cCamera= GetComponent<CinemachineCamera>();
			FindPlayer();
		}
		
		private void Update()
		{
			if (cCamera != null && cCamera.Target.TrackingTarget == null)
				FindPlayer();
		}
		
		
		// 公開メンバ関数 ---------------------------------------------------------------
		
		/// <summary>
		/// プレイヤータグを探す関数
		/// </summary>
		/// <param name="paramName">[引数の説明]</param>
		/// <returns>[戻り値の説明]</returns>	
		private void FindPlayer()
		{
			GameObject player = GameObject.FindWithTag("Player");
			cCamera.Target.TrackingTarget = player.transform;
		}
	}
}