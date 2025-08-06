using Core.Data;
using System.IO;
using Constants;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Managers
{
    public class SaveLoadManager: MManager
    {
        private UserSaveData _userSaveData;

        public UserSaveData UserSaveData => _userSaveData;
        
        public override void Initialize()
        {
            LoadUserData();
        }

        public void SaveUserData()
        {
            SaveData(_userSaveData,SaveDataNames.UserSaveData);
        }
        
        private void LoadUserData()
        {
            _userSaveData = LoadData<UserSaveData>(SaveDataNames.UserSaveData);
            if (_userSaveData == null)
            {
                Debug.Log("User save data not found, creating new one.");
                _userSaveData = new UserSaveData();
                SaveUserData();
            }
        }

        public void SaveData<T>(T data, string fileName = null)
        {
            string dataFileName = fileName ?? typeof(T).Name + ".json";
            string filePath = Path.Combine(Application.persistentDataPath, dataFileName);
            
            try
            {
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, jsonData);
                Debug.Log($"Data saved: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public T LoadData<T>(string fileName = null) where T : class
        {
            string dataFileName = fileName ?? typeof(T).Name + ".json";
            string filePath = Path.Combine(Application.persistentDataPath, dataFileName);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found: {filePath}");
                return null;
            }

            try
            {
                string jsonData = File.ReadAllText(filePath);
                T data = JsonConvert.DeserializeObject<T>(jsonData);
                Debug.Log($"Data loaded: {filePath}");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
        }
    }
}