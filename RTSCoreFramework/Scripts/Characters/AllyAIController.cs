using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTSCoreFramework
{
    public class AllyAIController : MonoBehaviour
    {
        #region Components
        protected NavMeshAgent myNavAgent;
        protected AllyEventHandler myEventHandler;
        protected AllyMember allyMember;
        #endregion

        #region Properties
        protected RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        protected RTSGameMode gamemode
        {
            get { return RTSGameMode.thisInstance; }
        }

        public AllyMember currentTargettedEnemy { get; protected set; }

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
        protected virtual void OnEnable()
        {

        }

        // Use this for initialization
        protected virtual void Start()
        {
            SetInitialReferences();
            SubToEvents();
            StartServices();
        }

        protected virtual void Update()
        {

        }

        protected virtual void LateUpdate()
        {

        }

        protected virtual void OnDisable()
        {
            UnSubFromEvents();
            CancelServices();
        }

        protected virtual void OnDrawGizmos()
        {

        }
        #endregion

        #region Getters
        public bool isEnemyFor(Transform _transform, out AllyMember _ally)
        {
            _ally = null;
            if (_transform.root.GetComponent<AllyMember>())
                _ally = _transform.root.GetComponent<AllyMember>();

            return _ally != null && allyMember.IsEnemyFor(_ally);
        }

        public bool isSurfaceWalkable(RaycastHit hit)
        {
            return myNavAgent.CalculatePath(hit.point, myNavAgent.path) &&
            myNavAgent.path.status == NavMeshPathStatus.PathComplete;
        }
        #endregion

        #region Handlers
        protected virtual void HandleCommandAttackEnemy(AllyMember enemy)
        {
            currentTargettedEnemy = enemy;
        }

        protected virtual void HandleStopTargetting()
        {
            currentTargettedEnemy = null;
        }

        protected void OnEnableCameraMovement(bool _enable)
        {
            if (!allyMember.isCurrentPlayer) return;
            myEventHandler.CallOnTryAim(_enable);
        }
        #endregion

        #region Initialization
        protected virtual void SubToEvents()
        {
            myEventHandler.EventCommandAttackEnemy += HandleCommandAttackEnemy;
            myEventHandler.EventStopTargettingEnemy += HandleStopTargetting;
            gamemaster.EventEnableCameraMovement += OnEnableCameraMovement;
        }

        protected virtual void UnSubFromEvents()
        {
            myEventHandler.EventCommandAttackEnemy -= HandleCommandAttackEnemy;
            myEventHandler.EventStopTargettingEnemy -= HandleStopTargetting;
            gamemaster.EventEnableCameraMovement -= OnEnableCameraMovement;
        }

        protected virtual void StartServices()
        {

        }

        protected virtual void CancelServices()
        {
            CancelInvoke();
        }

        protected virtual void SetInitialReferences()
        {
            myNavAgent = GetComponent<NavMeshAgent>();
            myEventHandler = GetComponent<AllyEventHandler>();
            allyMember = GetComponent<AllyMember>();

            if (!AllCompsAreValid)
            {
                Debug.LogError("Not all comps are valid!");
            }
        }
        #endregion

    }
}