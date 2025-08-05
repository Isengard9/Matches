using System;
using System.Collections.Generic;

namespace MiniGames.RunnerCube.Data
{
    [Serializable]
    public class RoadData
    {
        public float RoadLenght;
        public List<InteractableObjectData> InteractableObjects;
    }
}