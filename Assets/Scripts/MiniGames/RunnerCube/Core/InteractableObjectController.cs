using MiniGames.RunnerCube.Data;
using UnityEngine;

namespace MiniGames.RunnerCube.Core
{
    public class InteractableObjectController : MonoBehaviour
    {
        public InteractableObjectData Data;
        
        public void SetData(InteractableObjectData data)
        {
            Data = data;
            transform.position = data.InitialPosition;
            
        }


        public InteractableObjectData GetData()
        {
            return new InteractableObjectData
            {
                InteractableType = Data.InteractableType,
                InitialPosition = transform.localPosition
            };
        }
    }
}