using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JumpeeIsland
{
    public class AnimateComp : MonoBehaviour
    {
        [SerializeField] private Transform m_RotatePart;
        
        private Animator m_Animator;
        [SerializeField] private List<Vector3> tiles = new();
        private ICreatureMove m_Creature;
        private Vector3 direction;
        [SerializeField] private Vector3 destination; // Ending point of the jump (Point B)
        private bool isStartMoves; // Flag to track if the object is in a move loop
        [SerializeField] private bool isMoving; // Flag to track if the object is jumping
        [SerializeField] private int moveIndex;

        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int JumpUp = Animator.StringToHash("JumpUp");
        private static readonly int JumpDown = Animator.StringToHash("JumpDown");
        private static readonly int JumpUp1 = Animator.StringToHash("JumpUp1");
        private static readonly int JumpDown1 = Animator.StringToHash("JumpDown1");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

        private void Start()
        {
            if (m_RotatePart == null)
                m_RotatePart = transform;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isStartMoves)
                ResetMoves();

            if (isStartMoves && !isMoving)
                StartMove();
        }

        public void MoveToTarget(Vector3 currPos, int moveDir, ICreatureMove creature)
        {
            m_Creature ??= creature;
            tiles.Clear();
            GameFlowManager.Instance.GetEnvManager().GetMovementInspector().MovingPath(currPos, moveDir, tiles);
            ResetMoves();
        }

        private void ResetMoves()
        {
            moveIndex = 0;
            isStartMoves = true;
        }

        private void StartMove()
        {
            if (tiles.Count == 0)
            {
                EndMove();
                return;
            }

            destination = tiles[moveIndex];
            direction = new Vector3(destination.x, m_RotatePart.position.y, destination.z);
            m_RotatePart.LookAt(direction);

            if (Mathf.Abs(destination.y - transform.position.y) < 0.1f)
            {
                if (Mathf.Abs(Vector3.Distance(destination, transform.position)) < 1.5f)
                    m_Animator.SetBool(Walk, true);
                else
                {
                    if (moveIndex >= tiles.Count - 1)
                        TriggerAttackAnim(1); // 1 is jump attack, combo attack is from 2 afterward
                    else
                        m_Animator.SetTrigger(Jump);
                }
            }
            else if (destination.y > transform.position.y)
            {
                // Debug.Log($"Jump up from {transform.position} to {destination}: {Mathf.Abs(Vector3.Distance(destination, transform.position))}");
                m_Animator.SetTrigger(Mathf.Abs(Vector3.Distance(destination, transform.position)) < 1.5f
                    ? JumpUp1
                    : JumpUp);
            }
            else if (destination.y < transform.position.y)
                m_Animator.SetTrigger(Mathf.Abs(Vector3.Distance(destination, transform.position)) < 1.5f
                    ? JumpDown1
                    : JumpDown);

            isMoving = true;
        }

        public void EndMove()
        {
            m_Animator.SetBool(Walk, false);

            isMoving = false;
            moveIndex++;

            if (moveIndex >= tiles.Count)
            {
                isStartMoves = false;
                m_Creature.CreatureEndMove();
            }
        }

        public void SetAnimation(AnimateType animate)
        {
            switch (animate)
            {
                case AnimateType.Die:
                    m_Animator.SetTrigger(Die);
                    break;
            }
        }

        public void SetAnimator(Animator animator)
        {
            m_Animator = animator;
        }

        public void TriggerAttackAnim(int attackIndex)
        {
            m_Animator.SetInteger(AttackIndex, attackIndex);
            m_Animator.SetTrigger(Attack);
        }
    }
}