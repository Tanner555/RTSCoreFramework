using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTSCoreFramework
{
    public class AllyAIControllerCore : MonoBehaviour
    {
        #region Components
        protected NavMeshAgent myNavAgent;
        protected AllyEventHandlerCore myEventHandler;
        protected AllyMemberCore allyMember;
        #endregion

        #region Properties
        protected RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        protected RTSGameModeCore gamemode
        {
            get { return RTSGameModeCore.thisInstance; }
        }

        public AllyMemberCore currentTargettedEnemy { get; protected set; }

        protected virtual bool AllCompsAreValid
        {
            get
            {
                return myNavAgent && myEventHandler
                    && allyMember;
            }
        }
        #endregion

        #region UnityMessages
        // Use this for initialization
        protected virtual void Start()
        {
            SetInitialReferences();
            myEventHandler.EventCommandAttackEnemy += HandleCommandAttackEnemy;
            myEventHandler.EventStopTargettingEnemy += HandleStopTargetting;
            gamemaster.EventEnableCameraMovement += OnEnableCameraMovement;
        }

        protected virtual void OnDisable()
        {
            myEventHandler.EventCommandAttackEnemy -= HandleCommandAttackEnemy;
            myEventHandler.EventStopTargettingEnemy -= HandleStopTargetting;
            gamemaster.EventEnableCameraMovement -= OnEnableCameraMovement;
        }
        #endregion

        #region Getters
        public bool isEnemyFor(Transform _transform, out AllyMemberCore _ally)
        {
            _ally = null;
            if (_transform.root.GetComponent<AllyMemberCore>())
                _ally = _transform.root.GetComponent<AllyMemberCore>();

            return _ally != null && allyMember.IsEnemyFor(_ally);
        }

        public bool isSurfaceWalkable(RaycastHit hit)
        {
            return myNavAgent.CalculatePath(hit.point, myNavAgent.path) &&
            myNavAgent.path.status == NavMeshPathStatus.PathComplete;
        }
        #endregion

        #region Handlers
        protected void HandleCommandAttackEnemy(AllyMemberCore enemy)
        {
            currentTargettedEnemy = enemy;
        }

        protected void HandleStopTargetting()
        {
            currentTargettedEnemy = null;
        }

        protected void OnEnableCameraMovement(bool _enable)
        {
            if (!allyMember.isCurrentPlayer) return;
            myEventHandler.CallOnTryAim(_enable);
        }
        #endregion

        protected virtual void SetInitialReferences()
        {
            myNavAgent = GetComponent<NavMeshAgent>();
            myEventHandler = GetComponent<AllyEventHandlerCore>();
            allyMember = GetComponent<AllyMemberCore>();

            if (!AllCompsAreValid)
            {
                Debug.LogError("Not all comps are valid!");
            }
        }
    }
}