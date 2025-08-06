using UnityEngine;
using UnityEditor;
using MiniGames.RunnerCube.Core;
using MiniGames.RunnerCube.Data;
using System.IO;

public class RunnerGameCreator : EditorWindow
{
    private GameObject runnerGamePrefab;
    private GameObject instantiatedPrefab;
    private float roadLength = 100f;
    private const float MIN_ROAD_LENGTH = 100f;
    private const float MAX_ROAD_LENGTH = 500f;

    [MenuItem("Tools/Runner Game Creator")]
    public static void ShowWindow()
    {
        GetWindow<RunnerGameCreator>("Runner Game Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Runner Game Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Prefab alanı
        runnerGamePrefab = EditorGUILayout.ObjectField("Runner Game Prefab", runnerGamePrefab, typeof(GameObject), false) as GameObject;

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

            GUILayout.Space(20);
            
            if (GUILayout.Button("Create Runner Level", GUILayout.Height(30)))
            {
                CreateRunnerLevel();
            }
        }
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
                // Road uzunluğunu güncelle
                roadController.UpdateRoadLength(roadLength);
            }
        }
    }

    private void CreateRunnerLevel()
    {
        // Resources/Runner klasörünü duplice et
        string sourcePath = "Assets/Resources/Data/Level/Runner";
        string newFolderName = "Runner_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string destinationPath = "Assets/Resources/Data/Level/" + newFolderName;

        if (Directory.Exists(sourcePath))
        {
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
            AssetDatabase.Refresh();

            // RunnerDataSO dosyasını bul
            string[] guids = AssetDatabase.FindAssets("t:RunnerDataSO", new[] { destinationPath });
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                RunnerDataSO runnerData = AssetDatabase.LoadAssetAtPath<RunnerDataSO>(assetPath);

                // RunnerLevelController'ı bul ve veriyi ata
                var levelController = instantiatedPrefab.GetComponent<RunnerLevelController>();
                if (levelController != null)
                {
                    levelController.LevelData = runnerData;
                    levelController.SaveData();
                    
                    Debug.Log($"Runner level başarıyla oluşturuldu: {destinationPath}");
                }
                else
                {
                    Debug.LogError("RunnerLevelController bulunamadı!");
                }
            }
            else
            {
                Debug.LogError("RunnerDataSO bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("Resources/Runner klasörü bulunamadı!");
        }
    }

    private void OnDestroy()
    {
        if (instantiatedPrefab != null)
        {
            DestroyImmediate(instantiatedPrefab);
        }
    }
}