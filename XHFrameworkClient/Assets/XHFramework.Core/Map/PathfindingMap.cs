using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 寻路地图数据
    /// 从JSON加载的地图数据，用于A*寻路计算
    /// </summary>
    public class PathfindingMap
    {
        #region 属性
        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName { get; private set; }

        /// <summary>
        /// 地图宽度（格子数）
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// 地图高度（格子数）
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 单元格大小
        /// </summary>
        public Vector3 CellSize { get; private set; }

        /// <summary>
        /// 地图中心位置
        /// </summary>
        public Vector3 CenterPosition { get; private set; }

        /// <summary>
        /// 玩家出生点
        /// </summary>
        public Vector3 SpawnPoint { get; private set; }

        /// <summary>
        /// 格子数据（true=可行走，false=不可行走）
        /// </summary>
        private bool[,] _walkableGrid;
        #endregion

        #region 构造函数
        /// <summary>
        /// 从JSON数据创建地图
        /// </summary>
        public PathfindingMap(string jsonData)
        {
            ParseJsonData(jsonData);
        }

        /// <summary>
        /// 从MapData对象创建地图
        /// </summary>
        public PathfindingMap(MapData mapData)
        {
            LoadFromMapData(mapData);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 检查指定格子是否可行走
        /// </summary>
        /// <param name="x">格子X坐标</param>
        /// <param name="y">格子Y坐标</param>
        /// <returns>是否可行走</returns>
        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }
            return _walkableGrid[x, y];
        }

        /// <summary>
        /// 检查指定格子是否在地图范围内
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// 将世界坐标转换为格子坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // 计算相对于地图中心的偏移
            Vector3 offset = worldPosition - CenterPosition;

            // 转换为格子坐标（地图中心对应格子中心）
            int x = Mathf.FloorToInt(offset.x / CellSize.x + Width / 2f);
            int y = Mathf.FloorToInt(offset.y / CellSize.y + Height / 2f);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 将格子坐标转换为世界坐标（返回格子中心点）
        /// </summary>
        public Vector3 GridToWorld(int x, int y)
        {
            // 从格子坐标计算相对于地图中心的偏移
            float worldX = (x - Width / 2f + 0.5f) * CellSize.x;
            float worldY = (y - Height / 2f + 0.5f) * CellSize.y;

            return CenterPosition + new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// 将格子坐标转换为世界坐标
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return GridToWorld(gridPosition.x, gridPosition.y);
        }

        /// <summary>
        /// 获取指定位置最近的可行走格子
        /// </summary>
        public Vector2Int GetNearestWalkableGrid(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);

            if (IsWalkable(gridPos.x, gridPos.y))
            {
                return gridPos;
            }

            // 螺旋搜索最近的可行走格子
            for (int radius = 1; radius < Mathf.Max(Width, Height); radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                            continue;

                        int nx = gridPos.x + dx;
                        int ny = gridPos.y + dy;

                        if (IsWalkable(nx, ny))
                        {
                            return new Vector2Int(nx, ny);
                        }
                    }
                }
            }

            return gridPos;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 解析JSON数据
        /// </summary>
        private void ParseJsonData(string jsonData)
        {
            var mapData = JsonConvert.DeserializeObject<MapData>(jsonData);
            LoadFromMapData(mapData);
        }

        /// <summary>
        /// 从MapData加载数据
        /// </summary>
        private void LoadFromMapData(MapData mapData)
        {
            MapName = mapData.mapName;
            Width = mapData.width;
            Height = mapData.height;
            CellSize = mapData.cellSize.ToVector3();
            CenterPosition = mapData.centerPosition.ToVector3();
            SpawnPoint = mapData.spawnPoint.ToVector3();

            // 解析格子数据
            _walkableGrid = new bool[Width, Height];
            string gridData = mapData.gridData;

            // gridData是从上到下、从左到右排列的
            // 需要转换为二维数组[x, y]，其中y=0是底部
            for (int i = 0; i < gridData.Length && i < Width * Height; i++)
            {
                // 计算在gridData中的行列（从上到下）
                int row = i / Width;  // 从上往下的行号
                int col = i % Width;  // 列号

                // 转换为数组坐标（y=0在底部）
                int x = col;
                int y = Height - 1 - row;

                // '0'表示可行走，'1'表示不可行走
                _walkableGrid[x, y] = gridData[i] == '0';
            }

            Log.Info("[PathfindingMap] 地图加载完成: {0}, {1}x{2}", MapName, Width, Height);
        }
        #endregion

        #region 调试方法
        /// <summary>
        /// 在Scene视图中绘制地图网格（仅编辑器）
        /// </summary>
        public void DrawGizmos()
        {
#if UNITY_EDITOR
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 worldPos = GridToWorld(x, y);
                    Gizmos.color = IsWalkable(x, y)
                        ? new Color(0, 1, 0, 0.3f)
                        : new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawCube(worldPos, CellSize * 0.9f);
                }
            }
#endif
        }
        #endregion
    }
}
