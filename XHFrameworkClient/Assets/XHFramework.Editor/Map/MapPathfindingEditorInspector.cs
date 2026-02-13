using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using XHFramework.Core;

namespace XHFramework.Editor
{
    /// <summary>
    /// MapPathfindingEditor的自定义Inspector
    /// 提供状态检测和生成JSON的功能
    /// </summary>
    [CustomEditor(typeof(MapPathfindingEditor))]
    public class MapPathfindingEditorInspector : UnityEditor.Editor
    {
        #region 字段
        private MapPathfindingEditor _target;

        // 序列化属性
        private SerializedProperty _mapNameProp;
        private SerializedProperty _exportPathProp;
        private SerializedProperty _pathfindingTilemapProp;
        private SerializedProperty _spawnPointProp;

        // 折叠状态
        private bool _showMapInfo = true;
        private bool _showExportSettings = true;
        private bool _showComponents = true;
        #endregion

        #region Unity生命周期
        private void OnEnable()
        {
            _target = (MapPathfindingEditor)target;

            _mapNameProp = serializedObject.FindProperty("_mapName");
            _exportPathProp = serializedObject.FindProperty("_exportPath");
            _pathfindingTilemapProp = serializedObject.FindProperty("_pathfindingTilemap");
            _spawnPointProp = serializedObject.FindProperty("_spawnPoint");

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        #endregion

        #region Inspector绘制
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTitle();

            EditorGUILayout.Space(5);

            // 地图信息
            _showMapInfo = EditorGUILayout.Foldout(_showMapInfo, "地图信息", true, EditorStyles.foldoutHeader);
            if (_showMapInfo)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_mapNameProp, new GUIContent("地图名称"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            // 导出设置
            _showExportSettings = EditorGUILayout.Foldout(_showExportSettings, "导出设置", true, EditorStyles.foldoutHeader);
            if (_showExportSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_exportPathProp, new GUIContent("导出路径"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            // 组件引用（只读显示）
            _showComponents = EditorGUILayout.Foldout(_showComponents, "组件引用（自动创建）", true, EditorStyles.foldoutHeader);
            if (_showComponents)
            {
                EditorGUI.indentLevel++;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_pathfindingTilemapProp, new GUIContent("PathfindingTilemap"));
                EditorGUILayout.PropertyField(_spawnPointProp, new GUIContent("SpawnPoint"));
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // 状态检测
            DrawStatusInfo();

            EditorGUILayout.Space(10);

            // 生成按钮
            DrawGenerateButton();

            EditorGUILayout.Space(10);

            // 帮助信息
            DrawHelpBox();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }
        #endregion

        #region 绘制方法
        private void DrawTitle()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("地图寻路编辑器", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private void DrawStatusInfo()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("状态检测", EditorStyles.boldLabel);

            // 检测边界
            bool hasBoundary = _target.HasBoundary();
            if (hasBoundary)
            {
                if (_target.GetMapBounds(out BoundsInt bounds))
                {
                    EditorGUILayout.LabelField("边界状态:", "已绘制 ✓");
                    EditorGUILayout.LabelField($"地图大小: {bounds.size.x} x {bounds.size.y}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未检测到边界！请用BoundaryTile画出地图边框（四个角或完整边框）", MessageType.Warning);
            }

            // 检测障碍物（仅在有边界时检测）
            if (hasBoundary)
            {
                bool hasObstacles = _target.HasObstacles();
                if (hasObstacles)
                {
                    EditorGUILayout.LabelField("障碍物状态:", "已绘制 ✓");
                }
                else
                {
                    EditorGUILayout.HelpBox("未检测到障碍物。如果地图内部有不可行走区域，请用ObstacleTile绘制", MessageType.Info);
                }
            }

            // 检测SpawnPoint
            if (_target.ValidateSpawnPoint(out string errorMessage))
            {
                EditorGUILayout.LabelField("出生点状态:", "有效 ✓");
            }
            else
            {
                EditorGUILayout.HelpBox($"出生点问题: {errorMessage}", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGenerateButton()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.5f);

            if (GUILayout.Button("生成JSON文件", GUILayout.Height(40)))
            {
                GenerateAndSaveJson();
            }

            GUI.backgroundColor = originalColor;
        }

        private void DrawHelpBox()
        {
            EditorGUILayout.HelpBox(
                "使用说明：\n" +
                "1. 挂载脚本后自动创建PathfindingTilemap和SpawnPoint\n" +
                "2. 用BoundaryTile画出矩形边界（四个角或完整边框）\n" +
                "3. 用ObstacleTile画出不可行走区域\n" +
                "4. 调整SpawnPoint位置（需在空白格子上）\n" +
                "5. 点击\"生成JSON文件\"导出数据\n\n" +
                "规则：有Tile = 不可行走（1），空格子 = 可行走（0）",
                MessageType.Info);
        }
        #endregion

        #region Scene视图绘制
        private void OnSceneGUI(SceneView sceneView)
        {
            if (_target == null)
                return;

            DrawMapBoundsPreview();
            DrawSpawnPointPreview();
        }

        private void DrawMapBoundsPreview()
        {
            if (!_target.GetWorldBounds(out Bounds worldBounds))
                return;

            Color boundsColor = new Color(0f, 1f, 0f, 0.5f);
            Handles.color = boundsColor;

            Vector3[] corners = new Vector3[4]
            {
                new Vector3(worldBounds.min.x, worldBounds.min.y, 0),
                new Vector3(worldBounds.max.x, worldBounds.min.y, 0),
                new Vector3(worldBounds.max.x, worldBounds.max.y, 0),
                new Vector3(worldBounds.min.x, worldBounds.max.y, 0)
            };

            Handles.DrawLine(corners[0], corners[1]);
            Handles.DrawLine(corners[1], corners[2]);
            Handles.DrawLine(corners[2], corners[3]);
            Handles.DrawLine(corners[3], corners[0]);

            if (_target.GetMapBounds(out BoundsInt cellBounds))
            {
                Vector3 labelPos = worldBounds.center + Vector3.up * worldBounds.extents.y * 1.1f;
                Handles.Label(labelPos, $"{cellBounds.size.x} x {cellBounds.size.y}", EditorStyles.boldLabel);
            }
        }

        private void DrawSpawnPointPreview()
        {
            if (_target.SpawnPoint == null)
                return;

            Vector3 pos = _target.SpawnPoint.position;
            float size = 0.5f;

            bool isValid = _target.ValidateSpawnPoint(out _);
            Handles.color = isValid ? new Color(0f, 0.5f, 1f, 0.8f) : Color.red;

            Handles.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size);
            Handles.DrawLine(pos + Vector3.down * size, pos + Vector3.up * size);
            Handles.DrawWireDisc(pos, Vector3.forward, size * 0.8f);
            Handles.Label(pos + Vector3.up * size * 1.5f, "SpawnPoint", EditorStyles.boldLabel);
        }
        #endregion

        #region 私有方法
        private void GenerateAndSaveJson()
        {
            if (!_target.HasBoundary())
            {
                EditorUtility.DisplayDialog("错误", "请先用BoundaryTile画出地图边界", "确定");
                return;
            }

            if (!_target.ValidateSpawnPoint(out string errorMessage))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "出生点警告",
                    $"出生点存在问题：{errorMessage}\n\n是否仍要继续生成？",
                    "继续生成",
                    "取消");

                if (!proceed) return;
            }

            Undo.RecordObject(_target.transform, "Generate Map Data");

            MapData mapData = _target.GenerateMapData();

            if (mapData == null)
            {
                EditorUtility.DisplayDialog("生成失败", "生成地图数据失败", "确定");
                return;
            }

            string exportDir = Path.Combine(Application.dataPath, _target.ExportPath);

            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            string fileName = _target.ExportFileName;
            string filePath = Path.Combine(exportDir, fileName + ".json");

            // 使用Newtonsoft.Json序列化
            string json = _target.SerializeToJson(mapData);
            File.WriteAllText(filePath, json);

            AssetDatabase.Refresh();

            EditorUtility.SetDirty(_target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_target.gameObject.scene);

            string relativePath = "Assets/" + _target.ExportPath + "/" + fileName + ".json";
            Debug.Log($"[MapPathfindingEditor] JSON文件已保存: {relativePath}");

            int obstacleCount = 0;
            foreach (char c in mapData.gridData)
            {
                if (c == '1') obstacleCount++;
            }

            EditorUtility.DisplayDialog(
                "生成成功",
                $"地图数据已导出！\n\n" +
                $"地图名称: {mapData.mapName}\n" +
                $"文件路径: {relativePath}\n" +
                $"地图大小: {mapData.width} x {mapData.height}\n" +
                $"格子总数: {mapData.gridData.Length}\n" +
                $"不可行走: {obstacleCount}\n" +
                $"可行走: {mapData.gridData.Length - obstacleCount}",
                "确定");

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }
        #endregion
    }
}
