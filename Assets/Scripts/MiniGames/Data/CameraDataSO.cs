#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MiniGames.Data
{
    [CreateAssetMenu(fileName = "CamaraData", menuName = "NC/CameraData", order = 0)]
    public class CameraDataSO : ScriptableObject
    {
        public Vector3 originalPosition;
        public Vector3 originalRotation;
        public Vector3 originalScale;

        public bool UseOrtographic;

        [ContextMenu("Save Camera Data from Scene")]
        public virtual void SetCameraValues()
        {
            Camera sceneCamera = Camera.main;
            if (sceneCamera == null)
            {
                sceneCamera = FindObjectOfType<Camera>();
            }

            if (sceneCamera == null)
            {
                Debug.LogError("No camera found in scene!");
                return;
            }

            originalPosition = sceneCamera.transform.position;
            originalRotation = sceneCamera.transform.rotation.eulerAngles;
            originalScale = sceneCamera.transform.localScale;
            UseOrtographic = sceneCamera.orthographic;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            Debug.Log($"Camera data saved from: {sceneCamera.name}");
        }


       
    }

}