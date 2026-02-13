using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace XHFramework.Core
{
    /// <summary>
    /// 地图寻路编辑器 - 用于编辑和导出地图寻路数据
    /// 挂载在地图根节点上，自动创建PathfindingTilemap和SpawnPoint
    ///
    /// 使用方法：
    /// 1. 在PathfindingTilemap中用BoundaryTile画出矩形边界（四个角或完整边框）
    /// 2. 在PathfindingTilemap中用ObstacleTile画出不可行走区域
    /// 3. 调整SpawnPoint位置
    /// 4. 点击生成JSON（会自动居中地图到0,0,0）
    /// </summary>
    [ExecuteInEditMode]
    public class MapPathfindingEditor : MonoBehaviour
    {
        #region 常量
        private const string PATHFINDING_TILEMAP_NAME = "PathfindingTilemap";
        private const string SPAWNPOINT_NAME = "SpawnPoint";
        private const int TILEMAP_ORDER = 10000;
        #endregion

        #region 字段
        [Header("地图信息")]
        [Tooltip("地图唯一标识名称")]
        [SerializeField] private string _mapName;

        [Header("导出设置")]
        [Tooltip("JSON文件导出路径（相对于Assets目录）")]
        [SerializeField] private string _exportPath = "XHFramework.Game/PackageAssets/Config/Map";

        [Header("组件引用（自动创建）")]
        [SerializeField] private Tilemap _pathfindingTilemap;
        [SerializeField] private Transform _spawnPoint;
        #endregion

        #region 属性
        public string MapName => _mapName;
        public Tilemap PathfindingTilemap => _pathfindingTilemap;
        public Transform SpawnPoint => _spawnPoint;
        public string ExportPath => _exportPath;
        public string ExportFileName => _mapName;
        #endregion

        #region Unity生命周期
        private void Awake()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void Reset()
        {
            EnsureComponents();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取地图边界信息（基于Tilemap中所有Tile的范围）
        /// </summary>
        public bool GetMapBounds(out BoundsInt bounds)
        {
            bounds = default;

            if (_pathfindingTilemap == null)
            {
                return false;
            }

            _pathfindingTilemap.CompressBounds();
            bounds = _pathfindingTilemap.cellBounds;

            if (bounds.size.x <= 0 || bounds.size.y <= 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取地图边界的世界坐标
        /// </summary>
        public bool GetWorldBounds(out Bounds worldBounds)
        {
            worldBounds = default;

            if (!GetMapBounds(out BoundsInt cellBounds))
            {
                return false;
            }

            Vector3 min = _pathfindingTilemap.CellToWorld(cellBounds.min);
            Vector3 max = _pathfindingTilemap.CellToWorld(cellBounds.max);

            worldBounds = new Bounds();
            worldBounds.SetMinMax(min, max);

            return true;
        }

        /// <summary>
        /// 检查是否已画出边界（至少有Tile存在）
        /// </summary>
        public bool HasBoundary()
        {
            return GetMapBounds(out _);
        }

        /// <summary>
        /// 检查是否有障碍物（边界内部是否有Tile）
        /// </summary>
        public bool HasObstacles()
        {
            if (!GetMapBounds(out BoundsInt bounds))
            {
                return false;
            }

            // 检查边界内部是否有Tile（排除边框本身）
            for (int y = bounds.yMin + 1; y < bounds.yMax - 1; y++)
            {
                for (int x = bounds.xMin + 1; x < bounds.xMax - 1; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    if (_pathfindingTilemap.GetTile(cellPos) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 检查SpawnPoint是否在可行走区域
        /// </summary>
        public bool ValidateSpawnPoint(out string errorMessage)
        {
            errorMessage = null;

            if (_spawnPoint == null)
            {
                errorMessage = "SpawnPoint不存在";
                return false;
            }

            if (_pathfindingTilemap == null)
            {
                errorMessage = "PathfindingTilemap不存在";
                return false;
            }

            if (!GetMapBounds(out BoundsInt bounds))
            {
                errorMessage = "请先画出地图边界";
                return false;
            }

            // 将SpawnPoint世界坐标转换为格子坐标
            Vector3Int cellPos = _pathfindingTilemap.WorldToCell(_spawnPoint.position);

            // 检查是否在地图范围内
            if (cellPos.x < bounds.xMin || cellPos.x >= bounds.xMax ||
                cellPos.y < bounds.yMin || cellPos.y >= bounds.yMax)
            {
                errorMessage = $"SpawnPoint位置({cellPos.x}, {cellPos.y})超出地图范围";
                return false;
            }

            // 检查是否在有Tile的格子上（有Tile表示不可行走）
            TileBase tile = _pathfindingTilemap.GetTile(cellPos);
            if (tile != null)
            {
                errorMessage = $"SpawnPoint位置({cellPos.x}, {cellPos.y})在障碍物/边界上";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 居中地图并生成JSON数据
        /// 将边界中心移动到(0,0,0)位置，根节点跟随移动
        /// </summary>
        public MapData GenerateMapData()
        {
            if (!GetMapBounds(out BoundsInt bounds))
            {
                return null;
            }

            int width = bounds.size.x;
            int height = bounds.size.y;
            Vector3 cellSize = _pathfindingTilemap.cellSize;

            // 计算边界中心的格子坐标（浮点数，因为可能在格子中间）
            float cellCenterX = (bounds.xMin + bounds.xMax) / 2f;
            float cellCenterY = (bounds.yMin + bounds.yMax) / 2f;

            // 将边界中心的格子坐标转换为世界坐标
            Vector3 boundaryCenterWorld = _pathfindingTilemap.transform.TransformPoint(
                new Vector3(cellCenterX * cellSize.x, cellCenterY * cellSize.y, 0)
            );

            // 计算需要移动的偏移量：让边界中心移动到(0,0,0)
            Vector3 offset = Vector3.zero - boundaryCenterWorld;

            // 移动根节点，边界会跟随移动
            transform.position += new Vector3(offset.x, offset.y, 0);

            // 生成格子数据（字符串格式）
            StringBuilder gridDataBuilder = new StringBuilder(width * height);

            // 从上到下，从左到右遍历
            for (int y = bounds.yMax - 1; y >= bounds.yMin; y--)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    // 检查是否有Tile（有Tile = 不可行走）
                    TileBase tile = _pathfindingTilemap.GetTile(cellPos);

                    // 有Tile为1（不可行走），无Tile为0（可行走）
                    gridDataBuilder.Append(tile != null ? '1' : '0');
                }
            }

            // 创建地图数据
            MapData mapData = new MapData
            {
                mapName = _mapName,
                width = width,
                height = height,
                cellSize = new SerializableVector3(cellSize),
                centerPosition = new SerializableVector3(Vector3.zero),
                spawnPoint = _spawnPoint != null
                    ? new SerializableVector3(_spawnPoint.localPosition)
                    : new SerializableVector3(Vector3.zero),
                gridData = gridDataBuilder.ToString()
            };

            Debug.Log($"[MapPathfindingEditor] 地图数据生成完成: {_mapName}, {width}x{height}, 数据长度:{mapData.gridData.Length}");

            return mapData;
        }

        /// <summary>
        /// 将MapData序列化为JSON字符串
        /// </summary>
        public string SerializeToJson(MapData mapData)
        {
            return JsonConvert.SerializeObject(mapData, Formatting.Indented);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 确保必要组件存在
        /// </summary>
        private void EnsureComponents()
        {
            EnsureGrid();
            EnsurePathfindingTilemap();
            EnsureSpawnPoint();
        }

        /// <summary>
        /// 确保Grid组件存在
        /// </summary>
        private void EnsureGrid()
        {
            if (GetComponent<Grid>() == null)
            {
                gameObject.AddComponent<Grid>();
                Debug.Log("[MapPathfindingEditor] 已添加Grid组件");
            }
        }

        /// <summary>
        /// 确保PathfindingTilemap存在
        /// </summary>
        private void EnsurePathfindingTilemap()
        {
            if (_pathfindingTilemap != null) return;

            Transform existing = transform.Find(PATHFINDING_TILEMAP_NAME);
            if (existing != null)
            {
                _pathfindingTilemap = existing.GetComponent<Tilemap>();
                if (_pathfindingTilemap != null) return;
            }

            GameObject tilemapGO = new GameObject(PATHFINDING_TILEMAP_NAME);
            tilemapGO.transform.SetParent(transform);
            tilemapGO.transform.localPosition = Vector3.zero;
            tilemapGO.transform.localRotation = Quaternion.identity;
            tilemapGO.transform.localScale = Vector3.one;

            _pathfindingTilemap = tilemapGO.AddComponent<Tilemap>();

            TilemapRenderer renderer = tilemapGO.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = TILEMAP_ORDER;

            Debug.Log($"[MapPathfindingEditor] 已创建PathfindingTilemap: {PATHFINDING_TILEMAP_NAME}");
        }

        /// <summary>
        /// 确保SpawnPoint存在
        /// </summary>
        private void EnsureSpawnPoint()
        {
            if (_spawnPoint != null) return;

            Transform existing = transform.Find(SPAWNPOINT_NAME);
            if (existing != null)
            {
                _spawnPoint = existing;
                return;
            }

            GameObject spawnPointGO = new GameObject(SPAWNPOINT_NAME);
            spawnPointGO.transform.SetParent(transform);
            spawnPointGO.transform.localPosition = Vector3.zero;
            spawnPointGO.transform.localRotation = Quaternion.identity;
            spawnPointGO.transform.localScale = Vector3.one;

            _spawnPoint = spawnPointGO.transform;

            Debug.Log($"[MapPathfindingEditor] 已创建SpawnPoint: {SPAWNPOINT_NAME}");
        }
        #endregion
    }

    #region 数据结构
    /// <summary>
    /// 地图数据结构 - 用于JSON序列化
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        /// <summary>
        /// 地图唯一标识名称
        /// </summary>
        public string mapName;

        /// <summary>
        /// 地图宽度（格子数）
        /// </summary>
        public int width;

        /// <summary>
        /// 地图高度（格子数）
        /// </summary>
        public int height;

        /// <summary>
        /// 单元格大小
        /// </summary>
        public SerializableVector3 cellSize;

        /// <summary>
        /// 地图中心位置（通常为0,0,0）
        /// </summary>
        public SerializableVector3 centerPosition;

        /// <summary>
        /// 玩家出生点位置
        /// </summary>
        public SerializableVector3 spawnPoint;

        /// <summary>
        /// 格子数据字符串：'0'=可行走，'1'=不可行走
        /// 从上到下，从左到右排列
        /// </summary>
        public string gridData;
    }

    /// <summary>
    /// 可序列化的Vector3结构
    /// </summary>
    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3() { }

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
    #endregion
}
