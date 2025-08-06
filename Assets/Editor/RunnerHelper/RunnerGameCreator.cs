using UnityEngine;
using UnityEditor;
using MiniGames.RunnerCube.Core;
using MiniGames.RunnerCube.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core;
using MiniGames.Data;

public class RunnerGameCreator : EditorWindow
{
    private GameObject runnerGamePrefab;
    private GameObject instantiatedPrefab;
    private float roadLength = 100f;
    private const float MIN_ROAD_LENGTH = 100f;
    private const float MAX_ROAD_LENGTH = 500f;

    // Prefab alanları
    private GameObject collectiblePrefab;
    private GameObject obstaclePrefab;
    private GameObject finishLinePrefab;

    // Seçili obje
    private InteractableObjectController selectedObject;
    private Vector2 scrollPosition;

    [MenuItem("Matches/Runner Level Creator")]
    public static void ShowWindow()
    {
        GetWindow<RunnerGameCreator>("Runner Game Creator");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Runner Game Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Prefab alanı
        runnerGamePrefab =
            EditorGUILayout.ObjectField("Runner Game Prefab", runnerGamePrefab, typeof(GameObject),
                false) as GameObject;

        if (runnerGamePrefab != null && instantiatedPrefab == null)
        {
            if (GUILayout.Button("Place Prefab in Scene"))
            {
                PlacePrefabInScene();
            }
        }

        if (instantiatedPrefab != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Road Length Settings", EditorStyles.boldLabel);

            float newRoadLength = EditorGUILayout.Slider("Road Length", roadLength, MIN_ROAD_LENGTH, MAX_ROAD_LENGTH);

            if (newRoadLength != roadLength)
            {
                roadLength = newRoadLength;
                UpdateRoadLength();
            }

            GUILayout.Space(10);
            DrawInteractablePrefabFields();

            GUILayout.Space(10);
            DrawAddObjectButtons();

            GUILayout.Space(10);
            DrawObjectList();

            GUILayout.Space(10);
            DrawSelectedObjectSettings();

            GUILayout.Space(20);

            if (GUILayout.Button("Create Runner Level", GUILayout.Height(30)))
            {
                CreateRunnerLevel();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawInteractablePrefabFields()
    {
        GUILayout.Label("Interactable Prefabs", EditorStyles.boldLabel);

        // Collectible Prefab
        Color originalColor = GUI.color;
        if (collectiblePrefab == null) GUI.color = Color.red;
        collectiblePrefab =
            EditorGUILayout.ObjectField("Collectible Prefab", collectiblePrefab, typeof(GameObject), false) as
                GameObject;
        GUI.color = originalColor;

        // Obstacle Prefab
        if (obstaclePrefab == null) GUI.color = Color.red;
        obstaclePrefab =
            EditorGUILayout.ObjectField("Obstacle Prefab", obstaclePrefab, typeof(GameObject), false) as GameObject;
        GUI.color = originalColor;

        // Finish Line Prefab
        if (finishLinePrefab == null) GUI.color = Color.red;
        finishLinePrefab =
            EditorGUILayout.ObjectField("Finish Line Prefab", finishLinePrefab, typeof(GameObject),
                false) as GameObject;
        GUI.color = originalColor;
    }

    private void DrawAddObjectButtons()
    {
        GUILayout.Label("Add Objects", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Add Collectible - Her zaman eklenebilir
        GUI.enabled = collectiblePrefab != null;
        if (GUILayout.Button("Add Collectible"))
        {
            AddInteractableObject(InteractableTypeEnum.Collectible, collectiblePrefab);
        }

        // Add Obstacle - Collectible varsa eklenebilir
        GUI.enabled = obstaclePrefab != null && HasCollectible();
        if (GUILayout.Button("Add Obstacle"))
        {
            AddInteractableObject(InteractableTypeEnum.Obstacle, obstaclePrefab);
        }

        // Add Finish Line - Collectible varsa ve finish line yoksa eklenebilir
        GUI.enabled = finishLinePrefab != null && HasCollectible() && !HasFinishLine();
        if (GUILayout.Button("Add Finish Line"))
        {
            AddInteractableObject(InteractableTypeEnum.FinishLine, finishLinePrefab);
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectList()
    {
        GUILayout.Label("Objects in Scene", EditorStyles.boldLabel);

        var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
        if (roadController?.InteractableParent != null)
        {
            var objects = roadController.InteractableParent.GetComponentsInChildren<InteractableObjectController>();

            foreach (var obj in objects)
            {
                EditorGUILayout.BeginHorizontal();

                string objName = obj.Data.InteractableType.ToString();
                Vector3 pos = obj.transform.localPosition;
                GUILayout.Label($"{objName} (Z: {pos.z:F1})");

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedObject = obj;
                    Selection.activeGameObject = obj.gameObject;
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (selectedObject == obj) selectedObject = null;
                    DestroyImmediate(obj.gameObject);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawSelectedObjectSettings()
    {
        if (selectedObject != null)
        {
            GUILayout.Label("Selected Object Settings", EditorStyles.boldLabel);

            Vector3 currentPos = selectedObject.transform.localPosition;

            // Collectible için X ve Z, diğerleri için sadece Z
            if (selectedObject.Data.InteractableType == InteractableTypeEnum.Collectible)
            {
                float newX = EditorGUILayout.FloatField("Position X", currentPos.x);
                float newZ = EditorGUILayout.Slider("Position Z", currentPos.z, 0, roadLength - 5);

                if (newX != currentPos.x || newZ != currentPos.z)
                {
                    selectedObject.transform.localPosition = new Vector3(newX, currentPos.y, newZ);
                }
            }
            else
            {
                float maxZ = selectedObject.Data.InteractableType == InteractableTypeEnum.FinishLine
                    ? roadLength - 2
                    : roadLength - 5;

                float newZ = EditorGUILayout.Slider("Position Z", currentPos.z, 0, maxZ);

                if (newZ != currentPos.z)
                {
                    selectedObject.transform.localPosition = new Vector3(currentPos.x, currentPos.y, newZ);
                }
            }
        }
    }

    private void AddInteractableObject(InteractableTypeEnum type, GameObject prefab)
    {
        var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
        if (roadController?.InteractableParent != null)
        {
            var newObj =
                PrefabUtility.InstantiatePrefab(prefab, roadController.InteractableParent.transform) as GameObject;
            var controller = newObj.GetComponent<InteractableObjectController>();

            if (controller == null)
            {
                controller = newObj.AddComponent<InteractableObjectController>();
            }

            // Pozisyon hesaplama
            float zPos = type == InteractableTypeEnum.FinishLine ? roadLength - 2 : roadLength * 0.5f;

            var data = new InteractableObjectData
            {
                InteractableType = type,
                InitialPosition = new Vector3(0, 1, zPos),
            };

            controller.SetData(data);
            selectedObject = controller;
            Selection.activeGameObject = newObj;
        }
    }

    private bool HasCollectible()
    {
        var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
        if (roadController?.InteractableParent != null)
        {
            var objects = roadController.InteractableParent.GetComponentsInChildren<InteractableObjectController>();
            return objects.Any(obj => obj.Data.InteractableType == InteractableTypeEnum.Collectible);
        }

        return false;
    }

    private bool HasFinishLine()
    {
        var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
        if (roadController?.InteractableParent != null)
        {
            var objects = roadController.InteractableParent.GetComponentsInChildren<InteractableObjectController>();
            return objects.Any(obj => obj.Data.InteractableType == InteractableTypeEnum.FinishLine);
        }

        return false;
    }

    private void PlacePrefabInScene()
    {
        instantiatedPrefab = PrefabUtility.InstantiatePrefab(runnerGamePrefab) as GameObject;
        instantiatedPrefab.transform.position = Vector3.zero;
        Selection.activeGameObject = instantiatedPrefab;
    }

    private void UpdateRoadLength()
    {
        if (instantiatedPrefab != null)
        {
            var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
            if (roadController != null)
            {
                roadController.UpdateRoadLength(roadLength);

                // Finish Line pozisyonunu güncelle
                var objects = roadController.InteractableParent.GetComponentsInChildren<InteractableObjectController>();
                foreach (var obj in objects)
                {
                    if (obj.Data.InteractableType == InteractableTypeEnum.FinishLine)
                    {
                        var pos = obj.transform.localPosition;
                        obj.transform.localPosition = new Vector3(pos.x, pos.y, roadLength - 2);
                    }
                }
            }
        }
    }

    private void CreateRunnerLevel()
    {
        // Yeni klasör adı oluştur
        string basePath = "Assets/Resources/Data/Level/";
        string newFolderName = GetNextAvailableFolderName(basePath);
        string destinationPath = basePath + newFolderName;

        // Yeni klasör oluştur
        if (!AssetDatabase.IsValidFolder(destinationPath))
        {
            AssetDatabase.CreateFolder(basePath.TrimEnd('/'), newFolderName);
        }

        // RunnerDataSO oluştur
        RunnerDataSO runnerData = ScriptableObject.CreateInstance<RunnerDataSO>();

        // Camera data ata
        string cameraAssetPath = "Assets/Resources/Config/Runner_Camera.asset";
        var cameraData = AssetDatabase.LoadAssetAtPath<CameraDataSO>(cameraAssetPath);
        if (cameraData != null)
        {
            runnerData.CameraData = cameraData;
        }
        else
        {
            Debug.LogWarning($"Camera data bulunamadı: {cameraAssetPath}");
        }

        // LevelPrefab'ı ata (Orijinal prefab'ın asset yolundan)
        if (runnerGamePrefab != null)
        {
            string prefabPath = AssetDatabase.GetAssetPath(runnerGamePrefab);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            runnerData.LevelPrefab = prefabAsset;
        }
        else
        {
            Debug.LogError("RunnerGamePrefab bulunamadı!");
            return;
        }

        // Road data'yı kaydet
        var roadController = instantiatedPrefab.GetComponentInChildren<RoadController>();
        if (roadController != null)
        {
            var roadData = roadController.GetData();
            runnerData.RoadData = roadData;
        }
        else
        {
            Debug.LogError("RoadController bulunamadı!");
            return;
        }

        // RunnerDataSO'yu kaydet
        string runnerDataPath = $"{destinationPath}/RunnerData_{newFolderName}.asset";
        AssetDatabase.CreateAsset(runnerData, runnerDataPath);

        // LevelDataSO oluştur
        LevelDataSO levelData = ScriptableObject.CreateInstance<LevelDataSO>();
        levelData.LevelName = $"Runner Level {newFolderName}";
        levelData.Level = runnerData;

        // LevelDataSO'yu kaydet
        string levelDataPath = $"{destinationPath}/LevelData_{newFolderName}.asset";
        AssetDatabase.CreateAsset(levelData, levelDataPath);

        // Asset database'i refresh et
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // RunnerLevelController'a veriyi ata ve kaydet
        var levelController = instantiatedPrefab.GetComponent<RunnerLevelController>();
        if (levelController != null)
        {
            levelController.LevelData = runnerData;
            levelController.SaveData();

            Debug.Log($"Runner level başarıyla oluşturuldu: {destinationPath}");
            Debug.Log($"RunnerData: {runnerDataPath}");
            Debug.Log($"LevelData: {levelDataPath}");
        }
        else
        {
            Debug.LogError("RunnerLevelController bulunamadı!");
        }
    }

    private string GetNextAvailableFolderName(string basePath)
    {
        int counter = 1;
        string folderName;

        do
        {
            folderName = $"Runner_{counter}";
            counter++;
        } while (AssetDatabase.IsValidFolder(basePath + folderName));

        return folderName;
    }

    private void OnDestroy()
    {
        if (instantiatedPrefab != null)
        {
            DestroyImmediate(instantiatedPrefab);
        }
    }
}