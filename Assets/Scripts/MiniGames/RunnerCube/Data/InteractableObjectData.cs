using System;
using UnityEngine;

namespace MiniGames.RunnerCube.Data
{
    public enum InteractableTypeEnum
    {
        None,
        Obstacle,
        Collectible,
        FinishLine
    }
    [Serializable]
    public class InteractableObjectData
    {
        public InteractableTypeEnum InteractableType;
        public Vector3 InitialPosition;
    }
}