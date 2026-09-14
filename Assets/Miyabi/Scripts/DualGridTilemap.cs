// ======================================================================================
// File: DualGridTilemap.cs
//
// Brief: デュアルグリッドタイルマップシステムクラス
//
// Author: Banno Miyabi
// Date: 2026/09/11
// ======================================================================================
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MiyaLib.TileType;


namespace MiyaLib
{
	/// <summary>
	/// タイルマップの種類
	/// </summary>
	public enum TileType
	{
		None,	///< なし
		Ground,	///< 足場
		Empty,	///< 空白
	}

	public class DualGridTilemap : MonoBehaviour
	{
        // メンバ変数 -------------------------------------------------------------------
        [Tooltip("Four Adjacent Patterns / 隣接する4つのパターン")]
        private static Vector3Int[] neighbours = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0)
        };
        [Tooltip("Dictionary for Linking Patterns and Combinations / パターンと組み合わせの紐づけ用辞書")]
        private static Dictionary<Tuple<TileType, TileType, TileType, TileType>, Tile> neighbourTupleToTile;

        [Tooltip("A Simple Map for Evaluation / 判定用の単純なマップ")]
        [SerializeField] private Tilemap placeholderTilemap;
		[Tooltip("The map as seen by the player / 実際にプレイヤーに見えるマップ")]
        [SerializeField] private Tilemap displayTilemap;

        [Tooltip("Placeholder Tiles for Scaffolding / 足場の仮置きタイル")]
        [SerializeField] private Tile groundPlaceholderTile;
        [Tooltip("Empty Placeholder Tile / 空の仮置きタイル")]
        [SerializeField] private Tile emptyPlaceholderTile;

		[Tooltip("16 tiles / 16種類のタイル")]
		[SerializeField] private Tile[] tiles;


        // ライフサイクル関数 -----------------------------------------------------------
		private void Start()
		{
            // 4マスの組み合わせパターンと16種類のタイルを紐付ける
            neighbourTupleToTile = new()
            {
                {new (Ground,  Ground,  Ground,  Ground),  tiles[6]},  
                {new (Empty, Empty, Empty, Ground),  tiles[13]}, ///< 右下外側
                {new (Empty, Empty, Ground,  Empty), tiles[0]},  ///< 左下外側
                {new (Empty, Ground,  Empty, Empty), tiles[8]},  ///< 右上外側
                {new (Ground,  Empty, Empty, Empty), tiles[15]}, ///< 左上外側
                {new (Empty, Ground,  Empty, Ground),  tiles[1]},  ///< 右端
                {new (Ground,  Empty, Ground,  Empty), tiles[11]}, ///< 左端
                {new (Empty, Empty, Ground,  Ground),  tiles[3]},  ///< 下端
                {new (Ground,  Ground,  Empty, Empty), tiles[9]},  ///< 上端
                {new (Empty, Ground,  Ground,  Ground),  tiles[5]},  ///< 右下内側
                {new (Ground,  Empty, Ground,  Ground),  tiles[2]},  ///< 左下内側
                {new (Ground,  Ground,  Empty, Ground),  tiles[10]}, ///< 右上内側
                {new (Ground,  Ground,  Ground,  Empty), tiles[7]},  ///< 左上内側
                {new (Empty, Ground,  Ground,  Empty), tiles[14]}, ///< 斜め右上
                {new (Ground,  Empty, Empty, Ground),  tiles[4]},  ///< 斜め右下
                {new (Empty, Empty, Empty, Empty), tiles[12]},
            };
            RefreshDisplayTilemap();
        }


        // 公開メンバ関数 ---------------------------------------------------------------

        /// <summary>
        /// マップの変更を反映する関数
        /// </summary>
        /// <param name="coords">座標</param>
        /// <param name="tile">タイル</param>
        public void SetCell(Vector3Int coords, Tile tile)
        {
            placeholderTilemap.SetTile(coords, tile);
            SetDisplayTile(coords);
        }

        /// <summary>
        /// タイルタイプを取得する関数
        /// </summary>
        /// <param name="coords">座標</param>
		/// <returns>指定された座標にあるタイルの種類</returns>
        private TileType GetPlaceholderTileType(Vector3Int coords)
        {
            if (placeholderTilemap.GetTile(coords) == groundPlaceholderTile)
                return TileType.Ground;
            else
                return TileType.Empty;
        }

        /// <summary>
        /// タイルを判定・決定する計算関数
        /// </summary>
        /// <param name="coords">座標</param>
		/// <returns>指定された座標の見た目用タイル</returns>
        private Tile CalculateDisplayTile(Vector3Int coords)
        {
            // 各ハーフタイルを取得
            TileType topRight    = GetPlaceholderTileType(coords - neighbours[0]);
            TileType topLeft     = GetPlaceholderTileType(coords - neighbours[1]);
            TileType bottomRight = GetPlaceholderTileType(coords - neighbours[2]);
            TileType bottomLeft  = GetPlaceholderTileType(coords - neighbours[3]);

            // 1タイル内にある4つのハーフタイルの種類を取得
            Tuple<TileType, TileType, TileType, TileType> neighbourTuple = new(topLeft, topRight, bottomLeft, bottomRight);
            
            return neighbourTupleToTile[neighbourTuple];
        }

        /// <summary>
        /// 判定用タイルの変化に伴い、影響を受ける表示用タイル更新する関数
        /// </summary>
        /// <param name="pos">座標</param>
        private void SetDisplayTile(Vector3Int pos)
        {
            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector3Int newPos = pos + neighbours[i];
                displayTilemap.SetTile(newPos, CalculateDisplayTile(newPos));
            }
        }

        /// <summary>
        /// マップ全体の表示用タイルを一括で再計算・更新する関数
        /// </summary>
        public void RefreshDisplayTilemap()
        {
            for(int i = -50; i < 50; i++)
            {
                for (int j = -50; j < 50; j++)
                {
                    SetDisplayTile(new Vector3Int(i, j, 0));
                }
            }
        }
	}
}