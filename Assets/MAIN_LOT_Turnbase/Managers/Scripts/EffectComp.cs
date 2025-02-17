using System.Collections.Generic;
using UnityEngine;

namespace JumpeeIsland
{
    public class EffectComp : MonoBehaviour
    {
        private Entity m_Entity;
        private CreatureData m_CreatureData;
        [SerializeField] private int _remainTempJumpBoost;

        [SerializeField] private int _magnitudeJumpBoost;

        public void Init(Entity entity)
        {
            m_Entity = entity;

            if (m_Entity.GetType() == typeof(CreatureEntity))
                m_CreatureData = (CreatureData)m_Entity.GetData();
        }

        public void EffectCountDown()
        {
            if (m_CreatureData.EffectCaches == null)
                return;

            foreach (var effect in m_CreatureData.EffectCaches)
            {
                // Reset entity if the effect is over
                if (effect.EffectRemain == 1)
                {
                    switch (effect.EffectType)
                    {
                        case SkillEffectType.StrengthBoost:
                            ResetStrength();
                            break;

                        case SkillEffectType.Frozen:
                            RecoverFromFrozenState();
                            break;
                    }
                }

                effect.EffectRemain = Mathf.Clamp(effect.EffectRemain - 1, 0, effect.EffectRemain - 1);
            }
        }

        public bool CheckSkipTurn()
        {
            return GetEffectCache(SkillEffectType.Frozen)?.EffectRemain > 0;
        }

        #region STRENGTH BOOST

        private void ResetStrength()
        {
            if (m_Entity is IStatsProvider<UnitStats> statsProvider)
                m_CreatureData.CurrentDamage = statsProvider.GetStats().Strengh;
        }

        public void AdjustStrength(int magnitude, int duration)
        {
            var strengthCache = GetEffectCache(SkillEffectType.StrengthBoost);
            if (strengthCache.EffectRemain <= 0)
            {
                // Debug.Log($"Boost strength of {m_Entity.name} during {duration} steps.");
                m_CreatureData.CurrentDamage *= magnitude;
                strengthCache.EffectRemain = duration;
            }
        }

        #endregion

        #region TEMPORARY JUMP BOOST

        public void JumpBoost(int duration, int magnitude)
        {
            _remainTempJumpBoost = duration;
            _magnitudeJumpBoost = magnitude;
        }

        public int GetJumpBoost()
        {
            return _magnitudeJumpBoost;
        }

        public bool UseJumpBoost()
        {
            if (_remainTempJumpBoost <= 0)
                return false;
            _remainTempJumpBoost--;
            return true;
        }

        #endregion

        #region FROZEN

        public void RecordFrozen(int duration, Material effectMaterial)
        {
            m_Entity.GetSkin().SetCustomMaterial(effectMaterial);

            var frozenCache = GetEffectCache(SkillEffectType.Frozen);
            if (frozenCache.EffectRemain <= 0)
            {
                frozenCache.EffectRemain = duration;
                SufferFrozen();
            }
        }

        private void SufferFrozen()
        {
            if (m_Entity.TryGetComponent(out CreatureInGame creatureInGame))
                creatureInGame.SkipThisTurn();
        }

        private void RecoverFromFrozenState()
        {
            m_Entity.GetSkin().SetActiveMaterial();
        }

        #endregion

        private EffectCache GetEffectCache(SkillEffectType effectType)
        {
            if (m_CreatureData.EffectCaches == null)
                return null;
                
            EffectCache effectCache =
                m_CreatureData.EffectCaches.Find(t => t.EffectType == effectType);

            if (effectCache == null)
            {
                effectCache = new EffectCache(effectType);
                m_CreatureData.EffectCaches.Add(effectCache);
            }

            return effectCache;
        }
    }
}