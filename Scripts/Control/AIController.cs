using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine.AI;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspisionTime = 3.0f;
        [SerializeField] float wayPointTolorance = 1.0f; // how far can AI be from waypoint until it knows to follow path
        [SerializeField] float wayPointDwellTime = 2.0f; // how long AI should wait at any waypoint
        [SerializeField] float gaurdPatrolSpeed = 2f;
        [SerializeField] float gaurdChaseSpeed = 3.5f;
        float timeSinceSawPlayer = Mathf.Infinity;
        int currentWayPointIndex = 0;
        float timeSinceLastWayPoint = Mathf.Infinity;

        NavMeshAgent navAgent;
        GameObject player;
        Fighter fighter;
        Health health;
        Mover mover;
        Vector3 guardPosition;

        private void Start()
        {
            player = GameObject.FindWithTag("Player");
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            guardPosition = transform.position;
            mover = GetComponent<Mover>();
            navAgent = GetComponent<NavMeshAgent>();
        }
        private void Update()
        {
            if (health.IsDead()) { return; }
            GameObject player = GameObject.FindWithTag("Player");
            if (PlayerInAttackRange() && fighter.CanAttack(player))
            {
               
                AttackBehavior();
            }
            else if (timeSinceSawPlayer < suspisionTime)
            {
                SuspicionBehavior();
            }
            else
            {
                PatrolBehavior();
            }
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            timeSinceSawPlayer += Time.deltaTime;
            timeSinceLastWayPoint += Time.deltaTime;
        }

        private void PatrolBehavior()
        {
            Vector3 nextPos = guardPosition;
            if (patrolPath != null)
            {
                if (AtWayPoint())
                {
                    navAgent.speed = gaurdPatrolSpeed; // set patrol speed
                    timeSinceLastWayPoint = 0f;
                    CycleWayPoint();
                }
                nextPos = GetCurrentWayPoint();
            }
            if (timeSinceLastWayPoint > wayPointDwellTime)
            {
                
                mover.StartMoveAction(nextPos);
            }

        }

       private bool AtWayPoint()
        {
            float distanceToWayPoint = Vector3.Distance(this.transform.position, GetCurrentWayPoint());
            return distanceToWayPoint < wayPointTolorance;
        }
        private Vector3 GetCurrentWayPoint()
        {
            return patrolPath.GetWayPoint(currentWayPointIndex);
        }
        private void CycleWayPoint()
        {
            currentWayPointIndex = patrolPath.GetNextIndex(currentWayPointIndex);
        }
        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehavior()
        {
            navAgent.speed = gaurdChaseSpeed;
            timeSinceSawPlayer = 0.0f;
            fighter.Attack(player);
        }

        private bool PlayerInAttackRange()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer < chaseDistance;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
