using System;
using System.Collections.Generic;
using UnityEngine;

namespace JumpeeIsland
{
    [RequireComponent(typeof(IFactionController))]
    [RequireComponent(typeof(ObjectPool))]
    public class PlayerNpcLoader : MonoBehaviour
    {
        private IFactionController _factionController;
        private ObjectPool _creaturePool;
        private List<CreatureData> _creatureDatas;

        public void Init(List<CreatureData> npcData)
        {
            _factionController = GetComponent<IFactionController>();
            _creaturePool = GetComponent<ObjectPool>();

            _creatureDatas = npcData;

            foreach (var creatureData in _creatureDatas)
                if (creatureData.EntityName.Equals("King") == false)
                    TrainANewCreature(creatureData);

            _factionController.Init();
        }

        public void PlaceNewObject<T>(T data)
        {
            var creatureData = (CreatureData)Convert.ChangeType(data, typeof(CreatureData));
            TrainANewCreature(creatureData);
        }

        private void TrainANewCreature(CreatureData creatureData)
        {
            var creatureObj = _creaturePool.GetObject(creatureData.EntityName);
            if (creatureObj == null)
                return;

            // check if creature stand out of the map
            if (GameFlowManager.Instance.GetEnvManager().FreeToMove(creatureData.Position) == false)
            {
                var availablePos = GameFlowManager.Instance.GetEnvManager().GetRandomAvailableTile();
                if (availablePos.x.Equals(float.NegativeInfinity) == false)
                    creatureData.Position = availablePos;
            }

            creatureData.FactionType = _factionController.GetFaction(); // assign Faction
            GameFlowManager.Instance.OnDomainRegister.Invoke(creatureObj, _factionController.GetFaction());

            if (creatureObj.TryGetComponent(out CreatureInGame creatureInGame))
            {
                creatureInGame.gameObject.SetActive(true);
                creatureInGame.Init(creatureData, _factionController);
            }
        }
    }
}