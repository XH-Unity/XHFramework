using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 寻路管理器
    /// 提供地图加载和A*寻路的统一接口
    /// </summary>
    public class PathFindingManager : ManagerBase
    {
        public override int Priority => 120;
        
        #region 字段
        private Dictionary<string, PathfindingMap> _maps = new Dictionary<string, PathfindingMap>();
        private Dictionary<string, AStarPathfinding> _pathfinders = new Dictionary<string, AStarPathfinding>();
        private string _currentMapName;
        #endregion

        #region 属性
        /// <summary>
        /// 当前地图名称
        /// </summary>
        public string CurrentMapName => _currentMapName;

        /// <summary>
        /// 当前地图
        /// </summary>
        public PathfindingMap CurrentMap => GetMap(_currentMapName);

        /// <summary>
        /// 是否允许对角线移动
        /// </summary>
        public bool AllowDiagonal { get; set; } = true;
        #endregion

        #region 生命周期
        public override void Init()
        {
            _maps = new Dictionary<string, PathfindingMap>();
            _pathfinders = new Dictionary<string, AStarPathfinding>();
            _currentMapName = null;
            Log.Info("[PathFindingManager] 寻路管理器初始化完成");
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 寻路管理器不需要Update
        }

        public override void Shutdown()
        {
            UnloadAllMaps();
            _maps = null;
            _pathfinders = null;
            Log.Info("[PathFindingManager] 寻路管理器已关闭");
        }
        #endregion

        #region 地图加载（异步）
        /// <summary>
        /// 异步加载地图（通过地图名称）
        /// 自动从 ResourceManager 加载 JSON 文件
        /// 如果地图已加载，则直接设置为当前地图，不重复加载
        /// </summary>
        /// <param name="assetPath">地图名称（不含扩展名）</param>
        /// <param name="setAsCurrent">是否设为当前地图</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>是否加载成功</returns>
        public async UniTask<bool> LoadMapAsync(string assetPath, bool setAsCurrent = true, uint priority = 0)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Log.Error("[PathFindingManager] 地图名称为空");
                return false;
            }

            // 检查地图是否已加载
            if (_maps.ContainsKey(assetPath))
            {
                if (setAsCurrent)
                {
                    _currentMapName = assetPath;
                }
                Log.Info("[PathFindingManager] 地图已存在，直接使用: {0}", assetPath);
                return true;
            }

            var textAsset = await FW.ResourceManager.LoadAssetAndReleaseAsync<TextAsset>(assetPath, priority);
            if (textAsset == null)
            {
                Log.Error("[PathFindingManager] 加载地图配置失败: {0}", assetPath);
                return false;
            }

            string jsonData = textAsset.text;
            return LoadMap(jsonData, setAsCurrent);
        }
        #endregion

        #region 地图管理
        /// <summary>
        /// 从JSON字符串加载地图
        /// </summary>
        /// <param name="jsonData">JSON数据</param>
        /// <param name="setAsCurrent">是否设为当前地图</param>
        /// <returns>是否加载成功</returns>
        public bool LoadMap(string jsonData, bool setAsCurrent = true)
        {
            PathfindingMap map = new PathfindingMap(jsonData);
            return RegisterMap(map, setAsCurrent);
        }

        /// <summary>
        /// 从MapData对象加载地图
        /// </summary>
        /// <param name="mapData">地图数据</param>
        /// <param name="setAsCurrent">是否设为当前地图</param>
        /// <returns>是否加载成功</returns>
        public bool LoadMap(MapData mapData, bool setAsCurrent = true)
        {
            PathfindingMap map = new PathfindingMap(mapData);
            return RegisterMap(map, setAsCurrent);
        }

        /// <summary>
        /// 注册地图
        /// </summary>
        private bool RegisterMap(PathfindingMap map, bool setAsCurrent)
        {
            string mapName = map.MapName;

            if (string.IsNullOrEmpty(mapName))
            {
                Log.Error("[PathFindingManager] 地图名称为空，无法注册");
                return false;
            }

            // 移除旧的同名地图
            if (_maps.ContainsKey(mapName))
            {
                _maps.Remove(mapName);
                _pathfinders.Remove(mapName);
            }

            _maps[mapName] = map;
            _pathfinders[mapName] = new AStarPathfinding(map, AllowDiagonal);

            if (setAsCurrent)
            {
                _currentMapName = mapName;
            }

            Log.Info("[PathFindingManager] 地图已加载: {0}", mapName);
            return true;
        }

        /// <summary>
        /// 卸载地图
        /// </summary>
        /// <param name="mapName">地图名称</param>
        public void UnloadMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return;

            if (_maps.Remove(mapName))
            {
                _pathfinders.Remove(mapName);

                if (_currentMapName == mapName)
                {
                    _currentMapName = null;
                }

                Log.Info("[PathFindingManager] 地图已卸载: {0}", mapName);
            }
        }

        /// <summary>
        /// 卸载所有地图
        /// </summary>
        public void UnloadAllMaps()
        {
            _maps.Clear();
            _pathfinders.Clear();
            _currentMapName = null;
            Log.Info("[PathFindingManager] 所有地图已卸载");
        }

        /// <summary>
        /// 设置当前地图
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <returns>是否成功</returns>
        public bool SetCurrentMap(string mapName)
        {
            if (!string.IsNullOrEmpty(mapName) && _maps.ContainsKey(mapName))
            {
                _currentMapName = mapName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取地图
        /// </summary>
        /// <param name="mapName">地图名称，传入null则返回当前地图</param>
        public PathfindingMap GetMap(string mapName = null)
        {
            string name = string.IsNullOrEmpty(mapName) ? _currentMapName : mapName;
            if (string.IsNullOrEmpty(name))
                return null;

            _maps.TryGetValue(name, out PathfindingMap map);
            return map;
        }

        /// <summary>
        /// 检查地图是否已加载
        /// </summary>
        public bool HasMap(string mapName)
        {
            return !string.IsNullOrEmpty(mapName) && _maps.ContainsKey(mapName);
        }
        #endregion

        #region 寻路接口
        /// <summary>
        /// 寻找路径（格子坐标）
        /// </summary>
        /// <param name="startX">起点X</param>
        /// <param name="startY">起点Y</param>
        /// <param name="endX">终点X</param>
        /// <param name="endY">终点Y</param>
        /// <param name="mapName">地图名称，传入null则使用当前地图</param>
        /// <returns>路径点列表（格子坐标），找不到返回null</returns>
        public List<Vector2Int> FindPath(int startX, int startY, int endX, int endY, string mapName = null)
        {
            AStarPathfinding pathfinder = GetPathfinder(mapName);
            if (pathfinder == null)
            {
                Log.Error("[PathFindingManager] 未找到寻路器，请先加载地图");
                return null;
            }

            return pathfinder.FindPath(startX, startY, endX, endY);
        }

        /// <summary>
        /// 寻找路径（世界坐标）
        /// </summary>
        /// <param name="startWorld">起点世界坐标</param>
        /// <param name="endWorld">终点世界坐标</param>
        /// <param name="mapName">地图名称，传入null则使用当前地图</param>
        /// <returns>路径点列表（世界坐标），找不到返回null</returns>
        public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld, string mapName = null)
        {
            AStarPathfinding pathfinder = GetPathfinder(mapName);
            if (pathfinder == null)
            {
                Log.Error("[PathFindingManager] 未找到寻路器，请先加载地图");
                return null;
            }

            return pathfinder.FindPathWorld(startWorld, endWorld);
        }

        /// <summary>
        /// 检查指定位置是否可行走
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="mapName">地图名称，传入null则使用当前地图</param>
        public bool IsWalkable(Vector3 worldPosition, string mapName = null)
        {
            PathfindingMap map = GetMap(mapName);
            if (map == null)
                return false;

            Vector2Int gridPos = map.WorldToGrid(worldPosition);
            return map.IsWalkable(gridPos.x, gridPos.y);
        }

        /// <summary>
        /// 检查指定格子是否可行走
        /// </summary>
        public bool IsWalkable(int x, int y, string mapName = null)
        {
            PathfindingMap map = GetMap(mapName);
            if (map == null)
                return false;

            return map.IsWalkable(x, y);
        }

        /// <summary>
        /// 获取最近的可行走位置
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="mapName">地图名称，传入null则使用当前地图</param>
        /// <returns>最近可行走位置的世界坐标</returns>
        public Vector3 GetNearestWalkablePosition(Vector3 worldPosition, string mapName = null)
        {
            PathfindingMap map = GetMap(mapName);
            if (map == null)
                return worldPosition;

            Vector2Int nearestGrid = map.GetNearestWalkableGrid(worldPosition);
            return map.GridToWorld(nearestGrid);
        }

        /// <summary>
        /// 世界坐标转格子坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition, string mapName = null)
        {
            PathfindingMap map = GetMap(mapName);
            if (map == null)
                return Vector2Int.zero;

            return map.WorldToGrid(worldPosition);
        }

        /// <summary>
        /// 格子坐标转世界坐标
        /// </summary>
        public Vector3 GridToWorld(int x, int y, string mapName = null)
        {
            PathfindingMap map = GetMap(mapName);
            if (map == null)
                return Vector3.zero;

            return map.GridToWorld(x, y);
        }

        /// <summary>
        /// 格子坐标转世界坐标
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition, string mapName = null)
        {
            return GridToWorld(gridPosition.x, gridPosition.y, mapName);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取寻路器
        /// </summary>
        private AStarPathfinding GetPathfinder(string mapName)
        {
            string name = string.IsNullOrEmpty(mapName) ? _currentMapName : mapName;
            if (string.IsNullOrEmpty(name))
                return null;

            _pathfinders.TryGetValue(name, out AStarPathfinding pathfinder);
            return pathfinder;
        }
        #endregion
    }
}
