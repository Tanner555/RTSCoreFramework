using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public class RTSGameMode : MonoBehaviour
    {
        #region Enums
        [HideInInspector]
        public enum ECommanders
        {
            Commander_01,
            Commander_02,
            Commander_03,
            Commander_04,
            Commander_05,
            Commander_06,
        };
        [HideInInspector]
        public enum EFactions
        {
            Faction_Allies,
            Faction_Enemies,
            Faction_Default,
        };
        [HideInInspector]
        public enum ERTSGameState
        {
            EWaitingToStart,
            EPlaying,
            EGameIsPaused,
            EGameOver,
            EWon,
            EUnknown
        };
        [HideInInspector]
        public enum ERTSRewardTypes
        {
            Reward_Kill,

        };
        [HideInInspector]
        public enum ERTSPunishmentTypes
        {
            Punishment_KilledAnAlly,

        };
        #endregion

        #region Fields
        [Header("Factions")]
        public List<ECommanders> AllyFaction = new List<ECommanders>();
        public List<ECommanders> EnemyFaction = new List<ECommanders>();
        public List<PartyManager> GeneralMembers = new List<PartyManager>();

        [HideInInspector]
        public int TargetKillCount = 0;
        [HideInInspector]
        public int CurrentEnemyCount = 0;
        [HideInInspector]
        public int TargetGoal = 2;
        [HideInInspector]
        public int RoundScaleMultiplier = 2;
        [HideInInspector]
        public int AmmoInLevel = 0;
        [HideInInspector]
        public bool LostGame = false;
        //GameTimer
        [Header("GameMode Stats")]
        public int RemainingMinutes;
        public int RemainingSeconds;
        public float DefaultMatchTimeLimit;

        protected int Ally_Kills;
        protected int Enemy_Kills;
        protected int Ally_Points;
        protected int Enemy_Points;
        protected int Ally_Deaths;
        protected int Enemy_Deaths;
        //timers
        protected float MatchRemainingTime;
        protected float DefaultMatchStartingTime;
        //Point Rewards
        protected int DefaultKillPoints;
        //Point Consequences
        protected int DefaultFriendlyFirePoints;
        //Important GameState Instance
        protected ERTSGameState MatchState;

        #endregion

        #region Properties
        public PartyManager GeneralInCommand { get; protected set; }
        //GameMode must access GameMaster sooner than On Start
        //To Subscribe Events and Prevent Errors when AllySwitching
        public RTSGameMaster gamemaster
        {
            get
            {
                if (RTSGameMaster.thisInstance != null)
                    return RTSGameMaster.thisInstance;
                else if (GetComponent<RTSGameMaster>() != null)
                    return GetComponent<RTSGameMaster>();
                else
                    return GameObject.FindObjectOfType<RTSGameMaster>();
            }
        }
        public RTSUiManager uiManager { get { return RTSUiManager.thisInstance; } }
        //Static GameMode Instance For Easy Access
        [HideInInspector]
        public static RTSGameMode thisInstance
        {
            get; protected set;
        }
        public bool IsSpectatingEnemy
        {
            get { return GeneralInCommand.PartyMembers.Count <= 0; }
        }

        public AllyMember UiTargetAlly
        {
            get; protected set;
        }

        //Used for Getting Past Allies highlighted
        public AllyMember prevHighAlly { get; set; }
        public bool hasPrevHighAlly
        {
            get; set;
        }

        #endregion

        #region UnityMessages
        protected virtual void OnEnable()
        {
            ResetGameModeStats();
            InitializeGameModeValues();
            SubscribeToEvents();
            if (thisInstance != null)
                Debug.LogWarning("More than one instance of RTSGameMode in scene.");
            else
                thisInstance = (RTSGameMode)this;

            PartyManager firstGeneralFound = FindGenerals(false, null);
            if (firstGeneralFound == null)
            {
                Debug.LogWarning("No General could be found!");
            }
            else
            {
                foreach (var General in GeneralMembers)
                {
                    if (General.GeneralCommander == ECommanders.Commander_01)
                    {
                        SetGeneralInCommand(General);
                    }
                }
            }

            if (GeneralInCommand == null)
            {
                Debug.LogWarning("There is no Commander 01 in the scene!");
            }
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        //// Use this for initialization
        protected virtual void Start()
        {
            if (uiManager == null)
                Debug.LogWarning("There is no uimanager in the scene!");

            StartServices();
        }

        //// Update is called once per frame
        protected virtual void Update()
        {
            if (MatchState == ERTSGameState.EWaitingToStart)
            {
                waitingTillBeginMatch();
            }
            else if (MatchState == ERTSGameState.EPlaying)
            {
                playTheMatch();
            }
        }
        #endregion

        #region Getters
        //Used for CamRaycaster to determine if surface is walkable
        //by the AllyInCommand
        public bool isSurfaceReachableForAllyInCommand(RaycastHit hit)
        {
            if (GeneralInCommand.AllyInCommand != null)
            {
                return GeneralInCommand.AllyInCommand.aiController.isSurfaceWalkable(hit);
            }
            return false;
        }

        public int GetAllyFactionPlayerCount(AllyMember teamMember)
        {
            int _playerCount = 0;
            EFactions Faction = GetAllyFaction(teamMember);
            AllyMember[] Allies = GameObject.FindObjectsOfType<AllyMember>();
            foreach (var Ally in Allies)
            {
                if (Ally.AllyFaction == Faction)
                {
                    _playerCount++;
                }
            }
            return _playerCount;
        }
        //Adding new versions of playercount and partymanager functions for flexibility
        public int GetFactionPlayerCount(EFactions Faction)
        {
            int playerCount = 0;
            AllyMember[] Allies = GameObject.FindObjectsOfType<AllyMember>();
            foreach (var Ally in Allies)
            {
                if (Ally.AllyFaction == Faction)
                {
                    playerCount++;
                }
            }
            return playerCount;
        }

        public int GetGeneralPlayerCount(ECommanders General)
        {
            int playerCount = 0;
            AllyMember[] Allies = GameObject.FindObjectsOfType<AllyMember>();
            foreach (var Ally in Allies)
            {
                if (Ally.GeneralCommander == General)
                {
                    playerCount++;
                }
            }
            return playerCount;
        }

        public PartyManager GetPartyManagerFromECommander(ECommanders General)
        {
            foreach (var Gen in GeneralMembers)
            {
                if (Gen.GeneralCommander == General)
                {
                    return Gen;
                }
            }
            return null;
        }

        public int GetAllyGeneralPlayerCount(AllyMember teamMember)
        {
            int playerCount = 0;
            ECommanders General = teamMember.GeneralCommander;
            AllyMember[] allies = GameObject.FindObjectsOfType<AllyMember>();
            foreach (var ally in allies)
            {
                if (ally.GeneralCommander == General)
                {
                    playerCount++;
                }
            }
            return playerCount;
        }

        public PartyManager GetPartyManager(AllyMember _teamMember)
        {
            foreach (var Gen in GeneralMembers)
            {
                if (Gen.GeneralCommander == _teamMember.GeneralCommander)
                {
                    return Gen;
                }
            }
            return null;
        }

        public List<PartyManager> GetPartyManagers(EFactions _faction)
        {
            List<PartyManager> partymanagers = new List<PartyManager>();
            foreach (var Gen in GeneralMembers)
            {
                if (Gen.GeneralFaction == _faction)
                {
                    partymanagers.Add(Gen);
                }
            }
            return partymanagers;
        }

        public EFactions GetAllyFaction(AllyMember _teamMember)
        {
            if (AllyFaction.Contains(_teamMember.GeneralCommander))
            {
                return EFactions.Faction_Allies;
            }
            else if (EnemyFaction.Contains(_teamMember.GeneralCommander))
            {
                return EFactions.Faction_Enemies;
            }
            return EFactions.Faction_Default;
        }

        public int GetPendingReward(AllyMember receiver, ERTSRewardTypes rewardType)
        {
            return DefaultKillPoints;
        }

        public int GetPendingPunishment(AllyMember receiver, ERTSPunishmentTypes punishType)
        {
            return DefaultFriendlyFirePoints;
        }
        #endregion

        #region Setters
        public void SetGeneralInCommand(PartyManager setToCommand)
        {
            if (GeneralMembers.Contains(setToCommand))
            {
                GeneralInCommand = setToCommand;
            }
            //UpdateGeneralStatuses();
        }
        #endregion

        #region Finders
        public PartyManager FindGenerals(bool pendingLeave, PartyManager generalLeaving)
        {
            GeneralMembers.Clear();
            foreach (var PMember in GameObject.FindObjectsOfType<PartyManager>())
            {
                if (pendingLeave)
                {
                    if (PMember != generalLeaving)
                    {
                        GeneralMembers.Add(PMember);
                    }
                }
                else
                {
                    GeneralMembers.Add(PMember);
                }
            }

            if (GeneralMembers.Count <= 0)
            {
                Debug.LogWarning("No PartyManagers in Scene!");
                return null;
            }
            else
            {
                return GeneralMembers[0];
            }
        }

        public PartyManager FindEnemyCommander(PartyManager _partyMan)
        {
            foreach (var Gen in GeneralMembers)
            {
                if (Gen.GeneralFaction != _partyMan.GeneralFaction)
                {
                    return Gen;
                }
            }
            return null;
        }
        #endregion

        #region Updaters and Resetters
        public void UpdateGameModeStats()
        {
            EFactions allyFac = EFactions.Faction_Allies;
            EFactions enemyFac = EFactions.Faction_Enemies;
            GetFactionKills(true, allyFac);
            GetFactionKills(true, enemyFac);
            GetFactionPoints(true, allyFac);
            GetFactionPoints(true, enemyFac);
            GetFactionDeaths(true, allyFac);
            GetFactionDeaths(true, enemyFac);
        }

        public void ResetGameModeStats()
        {
            Ally_Kills = 0;
            Enemy_Kills = 0;
            Ally_Points = 0;
            Enemy_Points = 0;
            Ally_Deaths = 0;
            Enemy_Deaths = 0;
        }
        #endregion

        #region GameStateProcessing
        //GameState Getter and Setter
        public ERTSGameState GetMatchState()
        {
            return MatchState;
        }

        public void SetMatchState(ERTSGameState setmatchstate)
        {
            MatchState = setmatchstate;
        }

        //virtual play state methods handling the match state inside of tick function
        public virtual void playTheMatch()
        {
            //if (MatchRemainingTime > 0) {
            //	MatchRemainingTime--;
            //}
            MatchRemainingTime = DefaultMatchStartingTime - Time.time;
            RemainingMinutes = (int)(MatchRemainingTime / 60.0f);
            RemainingSeconds = ((int)MatchRemainingTime) % 60;
            //if (GetWorld()->GetTimeSeconds() - DefaultMatchStartingTime > DefaultMatchTimeLimit)
            if (MatchRemainingTime <= 0)
            {
                ERTSGameState gameover = ERTSGameState.EGameOver;
                SetMatchState(gameover);
            }
        }

        public virtual void waitingTillBeginMatch()
        {
            if (GetMatchState() == (ERTSGameState.EWaitingToStart))
            {
                ERTSGameState playgame = ERTSGameState.EPlaying;
                SetMatchState(playgame);
            }
        }

        public virtual void CallGameOverEvent(ECommanders callingCommander)
        {

        }
        #endregion

        #region AllyProcessing
        public virtual void ProcessAllySwitch(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {
            if (_toSet != null && _party.isCurrentPlayerCommander)
            {
                SetThirdPersonCam(_party, _toSet, _current);
                SetUiTargetAlly(_toSet);
            }
        }
        #endregion

        #region UIAndCameraProcessing
        void SetThirdPersonCam(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {
            if (!_party.isCurrentPlayerCommander) return;
            SetCameraCharacter(_toSet);
        }

        protected virtual void SetCameraCharacter(AllyMember _target)
        {

        }

        protected void SetUiTargetAlly(AllyMember _allyToSet)
        {
            if (_allyToSet != null)
            {
                //Set to Command
                UiTargetAlly = _allyToSet;
            }
        }
        #endregion

        #region AllyProcessing
        //Ui and Ally Processing
        public void HandlePartyMemberWOutAllies(PartyManager _partyMan, AllyMember _lAlly, bool _onDeath)
        {
            Debug.Log("No party members called on gamemode!");
            if (_onDeath && _partyMan.isCurrentPlayerCommander)
            {
                Debug.Log("All player allies are dead, Game Over!");
                gamemaster.CallEventGameOver();
                var _enemyCom = FindEnemyCommander(_partyMan);
                if (_enemyCom)
                {
                    var _enemy = _enemyCom.FindPartyMembers();
                    SetCameraCharacter(_enemy);
                    SetUiTargetAlly(_enemy);
                }
            }
        }

        public virtual void ProcessAllyDeath(AllyMember _ally)
        {
            //Check rewards for instigator
            AllyMember _instigator = _ally.DamageInstigator;
            if (_instigator != null)
            {
                //Killed enemy, give reward
                if (_ally.IsEnemyFor(_instigator))
                {
                    _instigator.PartyPoints += GetPendingReward(_instigator, RTSGameMode.ERTSRewardTypes.Reward_Kill);
                    _instigator.PartyKills += 1;
                }
                else
                {
                    //Friendly Kill, give punishment
                    _instigator.PartyPoints += GetPendingPunishment(_instigator, RTSGameMode.ERTSPunishmentTypes.Punishment_KilledAnAlly);
                }
                _instigator.npcMaster.CallEventKilledEnemy();
            }

            //Update GameModeStats in the end
            UpdateGameModeStats();
        }

        public bool AllyIsInCommand(AllyMember _allyMember)
        {
            return GetPartyManager(_allyMember).AllyIsCurrentPlayer(_allyMember);
        }

        public bool AllyIsGenCommanderMember(AllyMember _allyMember)
        {
            return GeneralInCommand.AllyIsAPartyMember(_allyMember);
        }
        #endregion

        #region Kills and Points Getters
        //Kills and Points Getters
        public int GetFactionKills(bool CalculateAccurateResults, EFactions Faction)
        {
            int FKills = (Faction == EFactions.Faction_Allies) ? Ally_Kills : Enemy_Kills;
            if (Faction != EFactions.Faction_Allies && Faction != EFactions.Faction_Enemies)
            {
                return 0;
            }
            if (CalculateAccurateResults)
            {
                foreach (var Gen in GeneralMembers)
                {
                    if (Gen.GeneralFaction == Faction)
                    {
                        FKills += Gen.PartyKills;
                    }
                }
                if (Faction == EFactions.Faction_Allies)
                    Ally_Kills = FKills;
                else if (Faction == EFactions.Faction_Enemies)
                    Enemy_Kills = FKills;
            }
            return FKills;
        }

        public int GetFactionPoints(bool CalculateAccurateResults, EFactions Faction)
        {
            int FPoints = (Faction == EFactions.Faction_Allies) ? Ally_Points : Enemy_Points;
            if (Faction != EFactions.Faction_Allies && Faction != EFactions.Faction_Enemies)
            {
                return 0;
            }
            if (CalculateAccurateResults)
            {
                foreach (var Gen in GeneralMembers)
                {
                    if (Gen.GeneralFaction == Faction)
                    {
                        FPoints += Gen.PartyPoints;
                    }
                }
                if (Faction == EFactions.Faction_Allies)
                    Ally_Points = FPoints;
                else if (Faction == EFactions.Faction_Enemies)
                    Enemy_Points = FPoints;
            }
            return FPoints;
        }

        public int GetFactionDeaths(bool CalculateAccurateResults, EFactions Faction)
        {
            int FactionDeaths = (Faction == EFactions.Faction_Allies) ? Ally_Deaths : Enemy_Deaths;
            if (Faction != EFactions.Faction_Allies && Faction != EFactions.Faction_Enemies)
                return 0;

            if (CalculateAccurateResults)
            {
                foreach (var Gen in GeneralMembers)
                {
                    if (Gen.GeneralFaction == Faction)
                    {
                        FactionDeaths += Gen.PartyDeaths;
                    }
                }
                if (Faction == EFactions.Faction_Allies)
                    Ally_Deaths = FactionDeaths;
                else if (Faction == EFactions.Faction_Enemies)
                    Enemy_Deaths = FactionDeaths;
            }
            return FactionDeaths;
        }
        #endregion

        #region GameModeSetupFunctions
        public void InitializeGameModeValues()
        {
            DefaultKillPoints = 3;
            DefaultFriendlyFirePoints = -1;
            DefaultMatchTimeLimit = 60.0f * 5.0f;
            DefaultMatchStartingTime = Time.time + DefaultMatchTimeLimit;
            ERTSGameState waitingstate = ERTSGameState.EWaitingToStart;
            SetMatchState(waitingstate);
        }

        protected virtual void SubscribeToEvents()
        {
            gamemaster.OnAllySwitch += ProcessAllySwitch;
        }

        protected virtual void UnsubscribeFromEvents()
        {
            gamemaster.OnAllySwitch -= ProcessAllySwitch;
        }

        protected virtual void StartServices()
        {
            //InvokeRepeating("SE_UpdateTargetUI", 0.1f, 0.1f);
        }
        #endregion
    }
}