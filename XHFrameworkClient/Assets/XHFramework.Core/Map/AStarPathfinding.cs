using System;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// A*寻路节点
    /// </summary>
    internal class PathfindingNode : IComparable<PathfindingNode>
    {
        /// <summary>
        /// 格子X坐标
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// 格子Y坐标
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 从起点到当前节点的实际代价
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// 从当前节点到终点的估算代价（启发函数）
        /// </summary>
        public float H { get; set; }

        /// <summary>
        /// 总代价 F = G + H
        /// </summary>
        public float F => G + H;

        /// <summary>
        /// 父节点（用于回溯路径）
        /// </summary>
        public PathfindingNode Parent { get; set; }

        /// <summary>
        /// 是否在开放列表中
        /// </summary>
        public bool IsInOpenList { get; set; }

        /// <summary>
        /// 是否在关闭列表中
        /// </summary>
        public bool IsInClosedList { get; set; }

        public PathfindingNode(int x, int y)
        {
            X = x;
            Y = y;
            Reset();
        }

        /// <summary>
        /// 重置节点状态
        /// </summary>
        public void Reset()
        {
            G = 0;
            H = 0;
            Parent = null;
            IsInOpenList = false;
            IsInClosedList = false;
        }

        public int CompareTo(PathfindingNode other)
        {
            int compare = F.CompareTo(other.F);
            if (compare == 0)
            {
                compare = H.CompareTo(other.H);
            }
            return compare;
        }

        public override bool Equals(object obj)
        {
            if (obj is PathfindingNode other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X * 10000 + Y;
        }
    }

    /// <summary>
    /// A*寻路算法
    /// </summary>
    public class AStarPathfinding
    {
        #region 常量
        /// <summary>
        /// 直线移动代价
        /// </summary>
        private const float STRAIGHT_COST = 1f;

        /// <summary>
        /// 对角线移动代价（√2 ≈ 1.414）
        /// </summary>
        private const float DIAGONAL_COST = 1.414f;

        /// <summary>
        /// 八方向偏移（包含对角线）
        /// </summary>
        private static readonly int[,] DIRECTIONS_8 = new int[,]
        {
            { 0, 1 },   // 上
            { 1, 0 },   // 右
            { 0, -1 },  // 下
            { -1, 0 },  // 左
            { 1, 1 },   // 右上
            { 1, -1 },  // 右下
            { -1, 1 },  // 左上
            { -1, -1 }  // 左下
        };

        /// <summary>
        /// 四方向偏移（不包含对角线）
        /// </summary>
        private static readonly int[,] DIRECTIONS_4 = new int[,]
        {
            { 0, 1 },   // 上
            { 1, 0 },   // 右
            { 0, -1 },  // 下
            { -1, 0 }   // 左
        };
        #endregion

        #region 字段
        private PathfindingMap _map;
        private PathfindingNode[,] _nodes;
        private List<PathfindingNode> _openList;
        private bool _allowDiagonal;
        #endregion

        #region 构造函数
        /// <summary>
        /// 创建A*寻路实例
        /// </summary>
        /// <param name="map">寻路地图</param>
        /// <param name="allowDiagonal">是否允许对角线移动</param>
        public AStarPathfinding(PathfindingMap map, bool allowDiagonal = true)
        {
            _map = map;
            _allowDiagonal = allowDiagonal;
            _openList = new List<PathfindingNode>();

            // 初始化节点网格
            _nodes = new PathfindingNode[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    _nodes[x, y] = new PathfindingNode(x, y);
                }
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 寻找从起点到终点的路径
        /// </summary>
        /// <param name="startX">起点X</param>
        /// <param name="startY">起点Y</param>
        /// <param name="endX">终点X</param>
        /// <param name="endY">终点Y</param>
        /// <returns>路径点列表（格子坐标），如果找不到路径返回null</returns>
        public List<Vector2Int> FindPath(int startX, int startY, int endX, int endY)
        {
            // 检查起点和终点是否有效
            if (!_map.IsInBounds(startX, startY) || !_map.IsInBounds(endX, endY))
            {
                Debug.LogWarning($"[AStarPathfinding] 起点或终点超出地图范围");
                return null;
            }

            if (!_map.IsWalkable(startX, startY))
            {
                Debug.LogWarning($"[AStarPathfinding] 起点({startX}, {startY})不可行走");
                return null;
            }

            if (!_map.IsWalkable(endX, endY))
            {
                Debug.LogWarning($"[AStarPathfinding] 终点({endX}, {endY})不可行走");
                return null;
            }

            // 起点和终点相同
            if (startX == endX && startY == endY)
            {
                return new List<Vector2Int> { new Vector2Int(startX, startY) };
            }

            // 重置所有节点
            ResetNodes();

            // 初始化起点
            PathfindingNode startNode = _nodes[startX, startY];
            PathfindingNode endNode = _nodes[endX, endY];

            startNode.G = 0;
            startNode.H = CalculateHeuristic(startX, startY, endX, endY);
            startNode.IsInOpenList = true;

            _openList.Clear();
            _openList.Add(startNode);

            // A*主循环
            while (_openList.Count > 0)
            {
                // 获取F值最小的节点
                PathfindingNode currentNode = GetLowestFNode();
                _openList.Remove(currentNode);
                currentNode.IsInOpenList = false;
                currentNode.IsInClosedList = true;

                // 到达终点
                if (currentNode.X == endX && currentNode.Y == endY)
                {
                    return BuildPath(currentNode);
                }

                // 遍历相邻节点
                ProcessNeighbors(currentNode, endX, endY);
            }

            // 找不到路径
            Debug.LogWarning($"[AStarPathfinding] 找不到从({startX}, {startY})到({endX}, {endY})的路径");
            return null;
        }

        /// <summary>
        /// 寻找从起点到终点的路径（世界坐标版本）
        /// </summary>
        /// <param name="startWorld">起点世界坐标</param>
        /// <param name="endWorld">终点世界坐标</param>
        /// <returns>路径点列表（世界坐标），如果找不到路径返回null</returns>
        public List<Vector3> FindPathWorld(Vector3 startWorld, Vector3 endWorld)
        {
            Vector2Int startGrid = _map.WorldToGrid(startWorld);
            Vector2Int endGrid = _map.WorldToGrid(endWorld);

            List<Vector2Int> gridPath = FindPath(startGrid.x, startGrid.y, endGrid.x, endGrid.y);

            if (gridPath == null)
            {
                return null;
            }

            // 转换为世界坐标
            List<Vector3> worldPath = new List<Vector3>(gridPath.Count);
            foreach (Vector2Int gridPos in gridPath)
            {
                worldPath.Add(_map.GridToWorld(gridPos));
            }

            return worldPath;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 重置所有节点状态
        /// </summary>
        private void ResetNodes()
        {
            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    _nodes[x, y].Reset();
                }
            }
        }

        /// <summary>
        /// 获取开放列表中F值最小的节点
        /// </summary>
        private PathfindingNode GetLowestFNode()
        {
            PathfindingNode lowest = _openList[0];
            for (int i = 1; i < _openList.Count; i++)
            {
                if (_openList[i].CompareTo(lowest) < 0)
                {
                    lowest = _openList[i];
                }
            }
            return lowest;
        }

        /// <summary>
        /// 处理当前节点的相邻节点
        /// </summary>
        private void ProcessNeighbors(PathfindingNode currentNode, int endX, int endY)
        {
            int[,] directions = _allowDiagonal ? DIRECTIONS_8 : DIRECTIONS_4;
            int directionCount = directions.GetLength(0);

            for (int i = 0; i < directionCount; i++)
            {
                int nx = currentNode.X + directions[i, 0];
                int ny = currentNode.Y + directions[i, 1];

                // 检查边界
                if (!_map.IsInBounds(nx, ny))
                    continue;

                // 检查是否可行走
                if (!_map.IsWalkable(nx, ny))
                    continue;

                PathfindingNode neighborNode = _nodes[nx, ny];

                // 已在关闭列表中
                if (neighborNode.IsInClosedList)
                    continue;

                // 对角线移动时，检查是否会穿墙
                if (_allowDiagonal && i >= 4)
                {
                    int dx = directions[i, 0];
                    int dy = directions[i, 1];

                    // 检查对角线两侧是否可行走
                    if (!_map.IsWalkable(currentNode.X + dx, currentNode.Y) ||
                        !_map.IsWalkable(currentNode.X, currentNode.Y + dy))
                    {
                        continue;
                    }
                }

                // 计算移动代价
                float moveCost = (i < 4) ? STRAIGHT_COST : DIAGONAL_COST;
                float newG = currentNode.G + moveCost;

                // 如果不在开放列表中，或者找到更短的路径
                if (!neighborNode.IsInOpenList || newG < neighborNode.G)
                {
                    neighborNode.G = newG;
                    neighborNode.H = CalculateHeuristic(nx, ny, endX, endY);
                    neighborNode.Parent = currentNode;

                    if (!neighborNode.IsInOpenList)
                    {
                        neighborNode.IsInOpenList = true;
                        _openList.Add(neighborNode);
                    }
                }
            }
        }

        /// <summary>
        /// 计算启发函数值（曼哈顿距离或对角线距离）
        /// </summary>
        private float CalculateHeuristic(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x2 - x1);
            int dy = Mathf.Abs(y2 - y1);

            if (_allowDiagonal)
            {
                // 对角线距离（Octile距离）
                return STRAIGHT_COST * (dx + dy) + (DIAGONAL_COST - 2 * STRAIGHT_COST) * Mathf.Min(dx, dy);
            }
            else
            {
                // 曼哈顿距离
                return STRAIGHT_COST * (dx + dy);
            }
        }

        /// <summary>
        /// 从终点回溯构建路径
        /// </summary>
        private List<Vector2Int> BuildPath(PathfindingNode endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            PathfindingNode current = endNode;

            while (current != null)
            {
                path.Add(new Vector2Int(current.X, current.Y));
                current = current.Parent;
            }

            // 反转路径（从起点到终点）
            path.Reverse();
            return path;
        }
        #endregion
    }
}
