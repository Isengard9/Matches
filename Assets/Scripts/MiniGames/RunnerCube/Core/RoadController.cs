using System.Collections.Generic;
using MiniGames.RunnerCube.Data;
using UnityEngine;

namespace MiniGames.RunnerCube.Core
{
    public class RoadController : MonoBehaviour
    {
        public RoadData RoadData;
        public List<InteractableObjectController> InteractableObjects = new();
        public GameObject InteractableParent;

        private List<InteractableObjectController> interactableObjects = new();

        public void UpdateRoadLength(float newLength)
        {
            transform.localScale = new Vector3(10, 1, newLength);
        }

        public void CreateRoad()
        {
            if (RoadData == null || InteractableObjects == null || InteractableObjects.Count == 0)
            {
                Debug.LogError("RoadData or InteractableObjectController is not set.");
                return;
            }


            for (int i = 0; i < RoadData.InteractableObjects.Count; i++)
            {
                var InteractableObjectController = InteractableObjects.Find(x =>
                    x.Data.InteractableType == RoadData.InteractableObjects[i].InteractableType);
                var interactableObject = Instantiate(InteractableObjectController, InteractableParent.transform);
                interactableObject.SetData(RoadData.InteractableObjects[i]);
                interactableObjects.Add(interactableObject);
            }

            transform.localScale = new Vector3(10, 1, RoadData.RoadLenght);
        }

        public void SetData(RoadData roadData)
        {
            RoadData = roadData;
            CreateRoad();
        }

        public RoadData GetData()
        {
            var roadData = new RoadData
            {
                RoadLenght = transform.localScale.z,
            };
            var data = new List<InteractableObjectData>();

            foreach (Transform obj in InteractableParent.transform)
            {
                var interactableObject = obj.GetComponent<InteractableObjectController>();
                if (interactableObject != null)
                {
                    var objData = interactableObject.GetData();
                    data.Add(objData);
                }
            }

            roadData.InteractableObjects = data;

            return roadData;
        }
    }
}