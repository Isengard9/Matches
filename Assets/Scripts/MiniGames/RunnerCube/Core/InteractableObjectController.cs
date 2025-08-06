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

            if(!Application.isPlaying)
                return;
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (data.InteractableType == InteractableTypeEnum.Collectible)
                renderer.material.color = Color.green;
            else if (data.InteractableType == InteractableTypeEnum.Obstacle)
                renderer.material.color = Color.red;
            else if (data.InteractableType == InteractableTypeEnum.FinishLine)
                renderer.material.color = new Color(0, 0, 0, 1);
        }


        public InteractableObjectData GetData()
        {
            return new InteractableObjectData
            {
                InteractableType = Data.InteractableType,
                InitialPosition = transform.localPosition,
            };
        }
    }
}