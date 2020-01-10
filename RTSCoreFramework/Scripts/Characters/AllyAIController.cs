using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTSCoreFramework
{
    #region AllyTacticsItemClass
    [System.Serializable]
    public class AllyTacticsItem
    {
        public int order;
        public IGBPI_DataHandler.IGBPI_Condition condition;
        public RTSActionItem action;

        public AllyTacticsItem(int order,
            IGBPI_DataHandler.IGBPI_Condition condition,
            RTSActionItem action)
        {
            this.order = order;
            this.condition = condition;
            this.action = action;
        }
    }
    #endregion

    #region RTSActionItemClass
    /// <summary>
    /// Used For Queueing Actions Inside AllyActionQueueController
    /// </summary>
    public class RTSActionItem
    {
        /// <summary>
        /// From IGBPI_Action Struct.
        /// </summary>
        public System.Action<AllyMember, AllyAIController, AllyMember> actionToPerform;
        /// <summary>
        /// From IGBPI_Action Struct.
        /// Only Needed For Testing Action Condition Inside 
        /// AllyTacticsController Script.
        /// Use (_self, _ai, _target) => true If No Additional Condition is Needed
        /// </summary>
        public System.Func<AllyMember, AllyAIController, AllyMember, bool> canPerformAction;
        /// <summary>
        /// From IGBPI_Action Struct.
        /// </summary>
        public ActionFilters actionFilter;
        /// <summary>
        /// Optional Action That Will Stop The Execution Of A Task.
        /// Use (_self, _ai, _target) => {} if Stopping Isn't Needed
        /// </summary>
        public System.Action<AllyMember, AllyAIController, AllyMember> stopPerformingTask;

        /// <summary>
        /// Used For Queueing Actions Inside AllyActionQueueController
        /// </summary>
        /// <param name="actionToPerform">From IGBPI_Action Struct.</param>
        /// <param name="canPerformAction">From IGBPI_Action Struct. Only Needed For Testing Action Condition Inside AllyTacticsController Script. Use (_ally) => true If No Additional Condition is Needed</param>
        /// <param name="actionFilter">From IGBPI_Action Struct.</param>
        /// <param name="stopPerformingTask">Optional Action That Will Stop The Execution Of A Task. Use (_ally) => {} if Stopping Isn't Needed</param>
        public RTSActionItem(
            System.Action<AllyMember, AllyAIController, AllyMember> actionToPerform,
            System.Func<AllyMember, AllyAIController, AllyMember, bool> canPerformAction,
            ActionFilters actionFilter,
            System.Action<AllyMember, AllyAIController, AllyMember> stopPerformingTask
            )
        {
            this.actionToPerform = actionToPerform;
            this.canPerformAction = canPerformAction;
            this.actionFilter = actionFilter;
            this.stopPerformingTask = stopPerformingTask;
        }

        private RTSActionItem()
        {

        }
    }
    #endregion

    public class AllyAIController : MonoBehaviour
    {
        #region Components
        protected NavMeshAgent myNavAgent
        {
            get
            {
                if (_myNavAgent == null)
                    _myNavAgent = GetComponent<NavMeshAgent>();

                if (_myNavAgent == null)
                {
                    //NavMesh hasn't been added yet.
                    _myNavAgent = gameObject.AddComponent<NavMeshAgent>();
                }

                return _myNavAgent;
            }
        }
        private NavMeshAgent _myNavAgent = null;
        protected AllyEventHandler myEventHandler
        {
            get
            {
                if (_myEventHandler == null)
                    _myEventHandler = GetComponent<AllyEventHandler>();

                return _myEventHandler;
            }
        }
        private AllyEventHandler _myEventHandler = null;
        protected AllyMember allyMember
        {
            get
            {
                if (_allyMember == null)
                    _allyMember = GetComponent<AllyMember>();

                return _allyMember;
            }
        }
        private AllyMember _allyMember = null;
        #endregion

        #region Fields
        //protected bool bIsShooting = false;
        //protected bool bIsMeleeing
        //{
        //    get { return myEventHandler.bIsMeleeingEnemy; }
        //}
        protected float defaultFireRepeatRate = 0.25f;
        //Used for finding closest ally
        [Header("AI Finder Properties")]
        public float sightRange = 40f;
        public float followDistance = 5f;
        //Private Layers using gamemode values
        //Set to -1 to compare an unset layer
        private LayerMask __allyLayers = -1;
        private LayerMask __sightLayers = -1;
        private LayerMask __allyAndCharacterLayers = -1;
        protected NavMeshQueryFilter agentQueryFilter;
        private NavMeshPath surfaceWalkablePath;

        protected Collider[] colliders;
        protected List<Transform> uniqueTransforms = new List<Transform>();
        protected List<AllyMember> scanEnemyList = new List<AllyMember>();

        //IGBPI
        public List<AllyTacticsItem> AllyTacticsList = new List<AllyTacticsItem>();
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

        protected RTSStatHandler statHandler
        {
            get
            {
                return RTSStatHandler.thisInstance;
            }
        }

        protected RTSUiMaster uiMaster { get { return RTSUiMaster.thisInstance; } }
        protected RTSUiManager uiManager { get { return RTSUiManager.thisInstance; } }

        protected RTSSaveManager saveManager { get { return RTSSaveManager.thisInstance; } }
        protected IGBPI_DataHandler dataHandler { get { return IGBPI_DataHandler.thisInstance; } }

        public AllyMember allyInCommand { get { return allyMember.allyInCommand; } }

        //AllyMember Transforms
        Transform headTransform { get { return allyMember.HeadTransform; } }
        Transform chestTransform { get { return allyMember.ChestTransform; } }
        Transform losTransform { get { return allyMember.MyLOSTransform; } }

        //Layer Props
        public LayerMask allyLayers
        {
            get
            {
                if (__allyLayers == -1)
                    __allyLayers = gamemode.AllyLayers;

                return __allyLayers;
            }
        }

        public LayerMask sightLayers
        {
            get
            {
                if (__sightLayers == -1)
                    __sightLayers = gamemode.SightLayers;

                return __sightLayers;
            }
        }

        public LayerMask allyAndCharacterLayers
        {
            get
            {
                if (__allyAndCharacterLayers == -1)
                    __allyAndCharacterLayers = gamemode.AllyAndCharacterLayers;

                return __allyAndCharacterLayers;
            }
        }

        //protected bool bIsMoving
        //{
        //    get { return myEventHandler.bIsNavMoving; }
        //}

        protected bool bCarryingRangeAndNoAmmoLeft =>
            allyMember.bIsCarryingMeleeWeapon ?
                false : allyMember.CurrentEquipedAmmo <= 0;

        //protected bool bStopUpdatingBattleBehavior
        //{
        //    get
        //    {
        //        return currentTargettedEnemy == null ||
        //        currentTargettedEnemy.IsAlive == false ||
        //        myEventHandler.bIsFreeMoving ||
        //        //Carrying Ranged Weapon and
        //        //Doesn't Have Any Ammo Left 
        //        bCarryingRangeAndNoAmmoLeft;
        //    }
        //}

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
            SubToEvents();
        }

        // Use this for initialization
        protected virtual void Start()
        {
            if (!AllCompsAreValid)
            {
                Debug.LogError("Not all comps are valid!");
            }
            StartServices();
        }

        protected virtual void OnDisable()
        {
            UnSubFromEvents();
            CancelServices();
        }
        #endregion

        #region Getters
        public virtual bool isEnemyFor(Transform _transform, out AllyMember _ally)
        {
            _ally = null;
            if (_transform.root.GetComponent<AllyMember>())
                _ally = _transform.root.GetComponent<AllyMember>();

            return _ally != null && allyMember.IsEnemyFor(_ally);
        }

        public virtual bool isSurfaceWalkable(RaycastHit hit)
        {
            return NavMesh.CalculatePath(transform.position, hit.point, agentQueryFilter, surfaceWalkablePath) &&
                surfaceWalkablePath.status == NavMeshPathStatus.PathComplete;
        }

        public virtual bool isSurfaceWalkable(Vector3 _point)
        {
            return NavMesh.CalculatePath(transform.position, _point, agentQueryFilter, surfaceWalkablePath) &&
                surfaceWalkablePath.status == NavMeshPathStatus.PathComplete;
        }

        public virtual float GetAttackRate()
        {
            return allyMember.WeaponAttackRate;
        }

        public virtual bool IsTargetInMeleeRange(GameObject _target)
        {
            bool _isCarryingMelee = allyMember.bIsCarryingMeleeWeapon;
            if (_isCarryingMelee == false) return false;
            float _distanceToTarget = (_target.transform.position - transform.position).magnitude;
            return _distanceToTarget <= allyMember.MaxMeleeAttackDistance;
        }
        #endregion

        #region Handlers
        protected virtual void OnAllyInitComps(RTSAllyComponentSpecificFields _specific, RTSAllyComponentsAllCharacterFields _allFields)
        {
            sightRange = _allFields.sightRange;
            followDistance = _allFields.followDistance;
            if(myNavAgent.enabled == false)
                myNavAgent.enabled = true;

            agentQueryFilter = new NavMeshQueryFilter
            {
                areaMask = myNavAgent.areaMask,
                agentTypeID = myNavAgent.agentTypeID
            };
            surfaceWalkablePath = new NavMeshPath();
        }

        //protected virtual void OnWeaponChanged(EEquipType _eType, EWeaponType _weaponType, EWeaponUsage _wUsage, bool _equipped)
        //{
        //    if (IsInvoking("UpdateBattleBehavior"))
        //    {
        //        bool _commandAttackRestart = myEventHandler.bIsCommandAttacking ? true : false;
        //        AllyMember _currentTargetRestart = currentTargettedEnemy;
        //        myEventHandler.CallEventFinishedMoving();
        //        myEventHandler.CallEventStopTargettingEnemy();
        //        StartCoroutine(OnWeaponChangedDelay(_commandAttackRestart, _currentTargetRestart));
        //    }
        //}

        /// <summary>
        /// Used To Delay The Restarting of Attacking Enemy Target
        /// </summary>
        protected virtual IEnumerator OnWeaponChangedDelay(bool _isCommand, AllyMember _ally)
        {
            yield return new WaitForSeconds(0.2f);
            if (_isCommand)
            {
                myEventHandler.CallEventPlayerCommandAttackEnemy(_ally);
            }
            else
            {
                myEventHandler.CallEventAICommandAttackEnemy(_ally);
            }
        }

        protected virtual void HandleCommandAttackEnemy(AllyMember enemy)
        {
            //CommandAttackEnemy(enemy);
        }

        //protected virtual void HandleStopTargetting()
        //{
        //    //currentTargettedEnemy = null;
        //    //StopBattleBehavior();
        //    //CancelInvoke();
        //}

        protected virtual void HandleOnMoveAlly(Vector3 _point, bool _isCommandMove)
        {
            //if (myEventHandler.bIsCommandMoving)
            //{
            //    if (IsInvoking("UpdateBattleBehavior"))
            //        StopBattleBehavior();
            //}
        }

        //protected virtual void HandleOnAIStopMoving()
        //{

        //}

        protected virtual void OnEnableCameraMovement(bool _enable)
        {

        }

        //protected virtual void TogglebIsShooting(bool _enable)
        //{
        //    bIsShooting = _enable;
        //}

        protected virtual void HandleAllySwitch(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {

        }

        protected virtual void OnAllyDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            StopAllCoroutines();
            CancelInvoke();
            this.enabled = false;
        }
        #endregion

        #region AITacticsCommands
        public virtual bool IsWithinFollowingDistance()
        {
            //Temp fix for PartyManager Delaying AllyInCommand Init Methods
            var _allyInCommand = allyInCommand;
            if (_allyInCommand == null)
            {
                Debug.Log("IsWithinFollowingDistance: Ally In Command is Null");
                return false;
            }

            return Vector3.Distance(transform.position,
                _allyInCommand.transform.position) <= followDistance;
        }

        public virtual (bool _success, AllyMember _target) Tactics_IsEnemyWithinSightRange()
        {
            return (false, null);
        }

        public virtual void Tactics_MoveToLeader()
        {
            if (allyMember.bIsAllyInCommand) return;

            //Temporarily Fixes Bug with Ally Attacking 
            //An Enemy After They Are Set To Command 
            //After Tactics Have Been Followed when Switching
            //From Command
            //if (IsInvoking("UpdateBattleBehavior"))
            //{
            //    StopBattleBehavior();
            //    myEventHandler.CallEventStopTargettingEnemy();
            //}

            if (IsWithinFollowingDistance() == false)
            {
                myEventHandler.CallEventAIMove(allyInCommand.transform.position);
            }
            else
            {
                FinishMoving();
            }
        }

        public virtual void AttackTargettedEnemy(AllyMember _self, AllyAIController _ai, AllyMember _target)
        {

        }

        public virtual void Tactics_AttackClosestEnemy()
        {

        }
        #endregion

        #region AITacticsHelpers
        protected virtual AllyMember FindClosestEnemy()
        {
            AllyMember _closestEnemy = null;
            if (headTransform == null)
            {
                Debug.LogError("No head assigned on AIController, cannot run look service");
                return _closestEnemy;
            }
            colliders = Physics.OverlapSphere(transform.position, sightRange, allyAndCharacterLayers);
            AllyMember _enemy = null;
            scanEnemyList.Clear();
            uniqueTransforms.Clear();
            foreach (Collider col in colliders)
            {
                if (uniqueTransforms.Contains(col.transform.root)) continue;
                uniqueTransforms.Add(col.transform.root);
                if (isEnemyFor(col.transform, out _enemy))
                {
                    RaycastHit hit;
                    if (hasLOSWithinRange(_enemy, out hit))
                    {
                        if (hit.transform.root == _enemy.transform.root)
                            scanEnemyList.Add(_enemy);
                    }
                }
            }

            if (scanEnemyList.Count > 0)
                _closestEnemy = DetermineClosestAllyFromList(scanEnemyList);

            return _closestEnemy;
        }

        public virtual bool hasLOSWithinRange(AllyMember _enemy, out RaycastHit _hit)
        {
            RaycastHit _myHit;
            bool _bHit = Physics.Linecast(losTransform.position,
                        _enemy.ChestTransform.position, out _myHit);
            _hit = _myHit;
            bool _valid = _bHit && _myHit.transform != null &&
                _myHit.transform.root.tag == gamemode.AllyTag;
            if (_valid)
            {
                AllyMember _hitAlly = _myHit.transform.root.GetComponent<AllyMember>();
                if (_hitAlly == allyMember)
                {
                    Debug.Log(allyMember.CharacterName +
                        " Has LOS With Himself.");
                }
                //TODO: RTSPrototype Fix hasLosWithinRange() hitting self instead of enemy
                return _hitAlly != null &&
                    (_hitAlly == allyMember ||
                    _hitAlly.IsEnemyFor(allyMember));
            }
            return false;
        }

        protected virtual AllyMember DetermineClosestAllyFromList(List<AllyMember> _allies)
        {
            AllyMember _closestAlly = null;
            float _closestDistance = Mathf.Infinity;
            foreach (var _ally in _allies)
            {
                float _newDistance = Vector3.Distance(_ally.transform.position,
                    transform.position);
                if (_newDistance < _closestDistance)
                {
                    _closestDistance = _newDistance;
                    _closestAlly = _ally;
                }
            }
            return _closestAlly;
        }
        #endregion

        #region Helpers
        public virtual bool IsPerformingSpecialAbility()
        {
            return false;
        }

        public virtual bool IsTargettingEnemy(out Transform _currentTarget)
        {
            _currentTarget = null;
            return false;
        }

        public virtual void TryStartSpecialAbility(System.Type _abilityType)
        {

        }

        public virtual void SetEnemyTarget(AllyMember _target)
        {

        }

        public virtual void FinishMoving()
        {

        }

        public virtual void ResetTargetting()
        {

        }

        public virtual void ResetSpecialAbilities()
        {

        }
        #endregion

        #region TacticsMainMethods
        protected virtual void ToggleTactics(bool _enable)
        {
            if (_enable)
            {
                LoadAndExecuteAllyTactics();
            }
            else
            {
                UnLoadAndCancelTactics();
            }
        }

        protected virtual void LoadAndExecuteAllyTactics()
        {
            UnLoadAndCancelTactics();
            var _tactics = statHandler.RetrieveCharacterTactics(
                    allyMember, allyMember.CharacterType);
            foreach (var _data in _tactics.Tactics)
            {
                bool _hasCondition = dataHandler.IGBPI_Conditions.ContainsKey(_data.condition);
                bool _hasAction = dataHandler.IGBPI_Actions.ContainsKey(_data.action);
                int _order = -1;
                bool _hasOrder = int.TryParse(_data.order, out _order) && _order != -1;
                if (_hasCondition && _hasAction && _hasOrder)
                {
                    AllyTacticsList.Add(new AllyTacticsItem(_order,
                        dataHandler.IGBPI_Conditions[_data.condition],
                        dataHandler.IGBPI_Actions[_data.action]));
                }
            }

            //if (AllyTacticsList.Count > 0)
            //    InvokeRepeating("ExecuteAllyTacticsList", 0.05f, 1f / executionsPerSec);
        }

        protected virtual void UnLoadAndCancelTactics()
        {
            AllyTacticsList.Clear();
        }
        #endregion

        #region ShootingAndBattleBehavior
        //protected virtual void CommandAttackEnemy(AllyMember enemy)
        //{
        //    previousTargettedEnemy = currentTargettedEnemy;
        //    currentTargettedEnemy = enemy;
        //    if (IsInvoking("UpdateBattleBehavior") == false)
        //    {
        //        StartBattleBehavior();
        //    }
        //    else if (IsInvoking("UpdateBattleBehavior") && previousTargettedEnemy != currentTargettedEnemy)
        //    {
        //        StopBattleBehavior();
        //        Invoke("StartBattleBehavior", 0.05f);
        //    }
        //}

        //protected virtual void UpdateBattleBehavior()
        //{
        //    // Pause Ally Tactics If Ally Is Paused
        //    // Due to the Game Pausing Or Control Pause Mode
        //    // Is Active
        //    if (myEventHandler.bAllyIsPaused) return;

        //    if (bStopUpdatingBattleBehavior)
        //    {
        //        myEventHandler.CallEventStopTargettingEnemy();
        //        myEventHandler.CallEventFinishedMoving();
        //        return;
        //    }

        //    if (allyMember.bIsCarryingMeleeWeapon)
        //    {
        //        //Melee Behavior
        //        if (IsTargetInMeleeRange(currentTargettedEnemy.gameObject))
        //        {
        //            if(bIsMeleeing == false)
        //            {
        //                myEventHandler.CallEventFinishedMoving();
        //                StartMeleeAttackBehavior();
        //            }
        //        }
        //        else
        //        {
        //            if (bIsMeleeing == true)
        //            {
        //                StopMeleeAttackBehavior();
        //            }

        //            myEventHandler.CallEventAIMove(currentTargettedEnemy.transform.position);
        //        }
        //    }
        //    else
        //    {
        //        //Shooting Behavior
        //        RaycastHit _hit;
        //        if (hasLOSWithinRange(currentTargettedEnemy, out _hit))
        //        {
        //            if (bIsShooting == false)
        //            {
        //                myEventHandler.CallEventFinishedMoving();
        //                StartShootingBehavior();
        //            }
        //        }
        //        else
        //        {
        //            if (bIsShooting == true)
        //                StopShootingBehavior();

        //            if (bIsMoving == false)
        //            {
        //                myEventHandler.CallEventAIMove(currentTargettedEnemy.transform.position);
        //            }
        //        }
        //    }
        //}

        //protected virtual void StartBattleBehavior()
        //{
        //    InvokeRepeating("UpdateBattleBehavior", 0f, 0.2f);
        //}

        //protected virtual void StopBattleBehavior()
        //{
        //    CancelInvoke("UpdateBattleBehavior");
        //    StopShootingBehavior();
        //}

        //protected virtual void StartShootingBehavior()
        //{
        //    myEventHandler.CallEventToggleIsShooting(true);
        //    InvokeRepeating("MakeFireRequest", 0.0f, GetAttackRate());
        //}

        //protected virtual void StopShootingBehavior()
        //{
        //    myEventHandler.CallEventToggleIsShooting(false);
        //    CancelInvoke("MakeFireRequest");
        //}

        //protected virtual void StartMeleeAttackBehavior()
        //{
        //    myEventHandler.CallEventToggleIsMeleeing(true);
        //    InvokeRepeating("MakeMeleeAttackRequest", 0.0f, GetAttackRate());
        //}

        //protected virtual void StopMeleeAttackBehavior()
        //{
        //    myEventHandler.CallEventToggleIsMeleeing(false);
        //    CancelInvoke("MakeMeleeAttackRequest");
        //}

        //protected virtual void MakeFireRequest()
        //{
        //    if (allyMember != null && allyMember.ActiveTimeBarIsFull())
        //    {
        //        // Pause Ally Tactics If Ally Is Paused
        //        // Due to the Game Pausing Or Control Pause Mode
        //        // Is Active
        //        if (myEventHandler.bAllyIsPaused) return;

        //        myEventHandler.CallOnTryUseWeapon();
        //    }
        //}

        //protected virtual void MakeMeleeAttackRequest()
        //{
        //    if (allyMember != null && allyMember.ActiveTimeBarIsFull())
        //    {
        //        // Pause Ally Tactics If Ally Is Paused
        //        // Due to the Game Pausing Or Control Pause Mode
        //        // Is Active
        //        if (myEventHandler.bAllyIsPaused) return;

        //        myEventHandler.CallOnTryUseWeapon();
        //    }
        //}
        #endregion

        #region Initialization
        protected virtual void SubToEvents()
        {
            myEventHandler.EventCommandAttackEnemy += HandleCommandAttackEnemy;
            //myEventHandler.EventStopTargettingEnemy += HandleStopTargetting;
            //myEventHandler.EventToggleIsShooting += TogglebIsShooting;
            myEventHandler.EventCommandMove += HandleOnMoveAlly;
            //myEventHandler.EventFinishedMoving += HandleOnAIStopMoving;
            //myEventHandler.OnWeaponChanged += OnWeaponChanged;
            myEventHandler.InitializeAllyComponents += OnAllyInitComps;
            myEventHandler.EventAllyDied += OnAllyDeath;            
            gamemaster.EventHoldingRightMouseDown += OnEnableCameraMovement;
            gamemaster.OnAllySwitch += HandleAllySwitch;
        }

        protected virtual void UnSubFromEvents()
        {
            myEventHandler.EventCommandAttackEnemy -= HandleCommandAttackEnemy;
            //myEventHandler.EventStopTargettingEnemy -= HandleStopTargetting;
            //myEventHandler.EventToggleIsShooting -= TogglebIsShooting;
            myEventHandler.EventCommandMove -= HandleOnMoveAlly;
            //myEventHandler.EventFinishedMoving -= HandleOnAIStopMoving;
            //myEventHandler.OnWeaponChanged -= OnWeaponChanged;
            myEventHandler.InitializeAllyComponents -= OnAllyInitComps;
            myEventHandler.EventAllyDied -= OnAllyDeath;
            gamemaster.EventHoldingRightMouseDown -= OnEnableCameraMovement;
            gamemaster.OnAllySwitch -= HandleAllySwitch;
        }

        protected virtual void StartServices()
        {

        }

        protected virtual void CancelServices()
        {
            CancelInvoke();
        }
        #endregion        
    }
}