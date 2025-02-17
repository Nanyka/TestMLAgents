using System;
using System.Collections;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace JumpeeIsland
{
    public class NPCInGame : CreatureInGame, IGetEntityInfo
    {
        public DummyAction InferMoving;

        [Tooltip("NPC will switch brain to infer their motion based on skills")]
        public bool _isSwitchBrain = true;

        [Tooltip("Some NPC just move around without jumping. If NPC can not jump, its animator is not set as root motion and not require ParentGoWithRoot script")]
        public bool _isJumpable = true;

        private BehaviorParameters m_BehaviorParameters;
        private Agent m_Agent;

        public virtual void Awake()
        {
            // Since this example does not inherit from the Agent class, explicit registration
            // of the RpcCommunicator is required. The RPCCommunicator should only be compiled
            // for Standalone platforms (i.e. Windows, Linux, or Mac)
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!CommunicatorFactory.CommunicatorRegistered)
            {
                CommunicatorFactory.Register<ICommunicator>(RpcCommunicator.Create);
            }
#endif
        }

        public override void OnEnable()
        {
            base.OnEnable();

            m_Agent = GetComponent<Agent>();
            m_BehaviorParameters = GetComponent<BehaviorParameters>();
        }

        #region INFER PHASE

        /// <summary>
        ///   <para>Send an action to agent, instead of asking for inferring from a brain, and ask for its reaction</para>
        /// </summary>
        public virtual void SelfInfer(NPCActionInferer inferer)
        {
            for (int i = 0; i <= 4; i++) // check all direction and record what will happen for each one
            {
                DummyAction action = new DummyAction(SelfResponseAction(i));
                inferer.AddActionToCache(action);
            }
        }

        private DummyAction SelfResponseAction(int action)
        {
            InferMoving.Action = action;
            InferMoving.CurrentPos = m_Transform.position;
            GetPositionByDirection(InferMoving.Action);
            return InferMoving;
        }

        public virtual void AskForAction()
        {
            if (m_BehaviorParameters.Model == null)
                m_FactionController.WaitForCreature();
            else
                m_Agent?.RequestDecision();
        }

        public virtual void ResponseAction(int direction)
        {
            InferMoving.Action = direction;
            InferMoving.CurrentPos = m_Transform.position;
            GetPositionByDirection(InferMoving.Action);
            m_FactionController.WaitForCreature();
        }

        protected virtual void GetPositionByDirection(int direction)
        {
            var movement = m_FactionController.GetMovementInspector()
                .MovingPath(m_Transform.position, direction, 0, 0);
            InferMoving.TargetPos = movement.returnPos;
            InferMoving.JumpCount = movement.jumpCount;

            if (InferMoving.TargetPos != m_Transform.position)
                InferMoving.Direction = InferMoving.TargetPos - m_Transform.position;
        }

        public new int GetJumpStep()
        {
            return InferMoving.JumpCount;
        }

        public Vector3 GetDirection()
        {
            return InferMoving.Direction;
        }

        public void SetBrain(NNModel brain)
        {
            if (brain != null)
                m_BehaviorParameters.Model = brain;
        }

        #endregion

        #region ACTION PHASE

        public void ConductSelectedAction(DummyAction selectedAction)
        {
            MarkAsUsedThisTurn();
            InferMoving = selectedAction;
            CreatureStartMove(m_Transform.position, InferMoving.Action);
        }
        
        // Use in battleReplayMode
        public void ConductSelectedAction(int action, int jump)
        {
            MarkAsUsedThisTurn();
            InferMoving.Action = action;
            InferMoving.JumpCount = jump;
            CreatureStartMove(m_Transform.position, InferMoving.Action);
        }

        protected override void CreatureStartMove(Vector3 currentPos, int direction)
        {
            MainUI.Instance.OnShowInfo.Invoke(this);
            m_Entity.ConductCreatureMove(currentPos, direction, this);

            if (InferMoving.JumpCount > 0)
                m_Entity.AttackSetup(this);
            
            m_Entity.TurnHealthSlider(false);
        }

        public override void CreatureEndMove()
        {
            m_Entity.UpdateTransform(InferMoving.TargetPos, m_RotatePart.eulerAngles);
            m_Entity.TurnHealthSlider(true);
            m_FactionController.KickOffNewTurn();
        }

        private IEnumerator MoveOverTime()
        {
            m_Entity.SetAnimation(AnimateType.Walk, true);
            while (transform.position != InferMoving.TargetPos)
            {
                m_Transform.position =
                    Vector3.MoveTowards(transform.position, InferMoving.TargetPos, 0.5f * Time.deltaTime);
                yield return null;
            }

            m_Entity.SetAnimation(AnimateType.Walk, false);
            m_Entity.UpdateTransform(InferMoving.TargetPos, m_RotatePart.eulerAngles);
            // Ask for the next inference
            if (GetJumpStep() > 0)
                Attack();
            else
                m_FactionController.KickOffNewTurn();
        }

        private new void Attack()
        {
            m_Entity.AttackSetup(this, this);
        }
        
        public override void AttackResponse()
        {
            m_FactionController.KickOffNewTurn();
        }

        #endregion

        #region GET & SET

        public new (string name, int health, int damage, int power) InfoToShow()
        {
            return (name, m_Entity.GetCurrentHealth(), m_Entity.GetAttackDamage(), InferMoving.JumpCount);
        }

        public new (Vector3 midPos, Vector3 direction, int jumpStep, FactionType faction) GetCurrentState()
        {
            return (InferMoving.TargetPos, GameFlowManager.Instance.GetEnvManager().GetMovementInspector().DirectionTo(InferMoving.Action),
                InferMoving.JumpCount, m_FactionController.GetFaction());
        }

        public new EnvironmentManager GetEnvironment()
        {
            return m_FactionController.GetEnvironment();
        }

        public Entity GetEntity()
        {
            return m_Entity;
        }

        #endregion
    }
}