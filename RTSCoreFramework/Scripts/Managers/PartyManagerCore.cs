using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public class PartyManagerCore : MonoBehaviour
    {
        #region Fields
        public RTSGameModeCore.ECommanders GeneralCommander;
        public RTSGameModeCore.EFactions GeneralFaction;
        //Keep track of kills and points for all partymembers
        [Header("Party Stats")]
        public int PartyKills;
        public int PartyPoints;
        public int PartyDeaths;
        #endregion

        #region Properties
        protected RTSGameModeCore gamemode
        {
            get { return RTSGameModeCore.thisInstance; }
        }
        protected RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }
        public AllyMemberCore AllyInCommand { get; protected set; }
        public List<AllyMemberCore> PartyMembers
        {
            get; protected set;
        }

        public AllyMemberCore FirstNonPlayerAlly
        {
            get
            {
                foreach (var _ally in PartyMembers)
                {
                    if (!AllyIsCurrentPlayer(_ally))
                        return _ally;
                }
                return null;
            }
        }

        public bool noPartyCommandsAllowed
        {
            get { return PartyMembers.Count <= 0; }
        }

        public bool isCurrentPlayerCommander { get { return GeneralCommander == gamemode.GeneralInCommand.GeneralCommander; } }

        #endregion

        #region UnityMessages
        protected virtual void OnEnable()
        {
            //isInOverview = false;
            PartyMembers = new List<AllyMemberCore>();
        }

        protected virtual void OnDisable()
        {
            EventNoPartyManagers -= HandleNoPartyMembers;
        }

        // Use this for initialization
        protected virtual void Start()
        {
            ResetPartyStats();
            if (gamemode == null)
                Debug.LogWarning("RTS GameMode does not exist!");

            EventNoPartyManagers += HandleNoPartyMembers;
            gamemaster.OnLeftClickAlly += HandleLeftClickPartyMember;
            gamemaster.OnRightClickSendHit += HandleRightClick;

            AllyMemberCore firstAlly = FindPartyMembers(false, null);
            SetAllyInCommand(firstAlly);
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
        #endregion

        #region Find-Set-Possess-AllyInCommand
        public AllyMemberCore FindPartyMembers(bool pendingAllyLeave, AllyMemberCore allyToLeave)
        {
            PartyMembers.Clear();
            AllyMemberCore[] Allies = GameObject.FindObjectsOfType<AllyMemberCore>();
            foreach (var ally in Allies)
            {
                if (pendingAllyLeave)
                {
                    if (ally != allyToLeave)
                    {
                        if (ally.GeneralCommander == this.GeneralCommander)
                        {
                            PartyMembers.Add(ally);
                        }
                    }
                }
                else
                {
                    if (ally.GeneralCommander == this.GeneralCommander)
                    {
                        PartyMembers.Add(ally);
                    }
                }
            }

            if (PartyMembers.Count <= 0)
            {
                Debug.LogWarning("No partyMembers in Scene!");
                return null;
            }
            else
            {
                AllyMemberCore firstallyfound = PartyMembers[0];
                return firstallyfound;
            }
        }

        public AllyMemberCore FindPartyMembers()
        {
            PartyMembers.Clear();
            AllyMemberCore[] Allies = GameObject.FindObjectsOfType<AllyMemberCore>();
            foreach (var ally in Allies)
            {
                if (ally.GeneralCommander == this.GeneralCommander)
                    PartyMembers.Add(ally);
            }

            if (PartyMembers.Count <= 0)
            {
                Debug.LogWarning("No partyMembers in Scene!");
                return null;
            }
            else
            {
                AllyMemberCore firstallyfound = PartyMembers[0];
                return firstallyfound;
            }
        }

        public void SetAllyInCommand(AllyMemberCore _setToCommand)
        {
            bool _validSet = _setToCommand != null &&
                _setToCommand.GetComponent<AllyMemberCore>() != null &&
                PartyMembers.Contains(_setToCommand);

            if (_validSet)
            {
                foreach (var _ally in PartyMembers)
                {
                    //TODO: RTSPrototype Fix null exception from foreach loop, this should not happen
                    if (_ally != null)
                        _ally.npcMaster.CallEventPartySwitching();
                }
                gamemaster.CallOnAllySwitch((PartyManagerCore)this, _setToCommand, AllyInCommand);
                if (AllyInCommand != null)
                    AllyInCommand.GetComponent<AllyEventHandlerCore>().CallEventSwitchingFromCom();

                AllyInCommand = _setToCommand;
                AllyInCommand.GetComponent<AllyEventHandlerCore>().CallEventSetAsCommander();
            }
        }

        public void PossessAllyAdd()
        {
            if (AllyInCommand && PartyMembers.Count > 0)
            {
                int allyCommandIndex = PartyMembers.IndexOf(AllyInCommand);
                if (allyCommandIndex + 1 > 0 && allyCommandIndex + 1 < PartyMembers.Count)
                {
                    SetAllyInCommand(PartyMembers[allyCommandIndex + 1]);
                }
                else if (PartyMembers.Count > 0)
                {
                    SetAllyInCommand(PartyMembers[0]);
                }
            }
        }

        public void PossessAllySubtract()
        {
            if (AllyInCommand && PartyMembers.Count > 0)
            {
                int allyCommandIndex = PartyMembers.IndexOf(AllyInCommand);
                int endIndex = PartyMembers.Count - 1;

                if (allyCommandIndex - 1 > -1 && allyCommandIndex - 1 < PartyMembers.Count)
                {
                    SetAllyInCommand(PartyMembers[allyCommandIndex - 1]);
                }
                else if (endIndex > 0 && endIndex < PartyMembers.Count)
                {
                    SetAllyInCommand(PartyMembers[endIndex]);
                }
            }
        }
        #endregion

        #region Getters
        public bool AllyIsCurrentPlayer(AllyMemberCore _ally)
        {
            return isCurrentPlayerCommander && _ally == AllyInCommand;
        }
        public bool AllyIsAPartyMember(AllyMemberCore _ally)
        {
            return PartyMembers.Contains(_ally);
        }
        #endregion

        #region EventCalls
        public void CallEventNoPartyMembers(PartyManagerCore partyMan, AllyMemberCore lastMember, bool onDeath)
        {
            if (EventNoPartyManagers != null)
            {
                EventNoPartyManagers(partyMan, lastMember, onDeath);
            }
        }
        #endregion

        #region Handlers
        protected void HandleNoPartyMembers(PartyManagerCore _partyMan, AllyMemberCore _lAlly, bool _onDeath)
        {
            AllyInCommand = null;
            gamemode.HandlePartyMemberWOutAllies(_partyMan, _lAlly, _onDeath);
        }
        protected void HandleLeftClickPartyMember(AllyMemberCore ally)
        {
            if (!isCurrentPlayerCommander || noPartyCommandsAllowed) return;
            if (PartyMembers.Contains(ally) && ally != AllyInCommand)
            {
                SetAllyInCommand(ally);
            }
        }
        protected void HandleRightClick(rtsHitType hitType, RaycastHit hit)
        {
            if (!isCurrentPlayerCommander || noPartyCommandsAllowed) return;
            switch (hitType)
            {
                case rtsHitType.Ally:
                    break;
                case rtsHitType.Enemy:
                    GameObject _root = hit.collider.gameObject.transform.root.gameObject;
                    AllyMemberCore _enemy = _root.GetComponent<AllyMemberCore>();
                    AllyInCommand.npcMaster.CallEventCommandAttackEnemy(_enemy);
                    break;
                case rtsHitType.Cover:
                    break;
                case rtsHitType.Walkable:
                    AllyInCommand.npcMaster.CallEventCommandMove(hitType, hit);
                    break;
                case rtsHitType.Unwalkable:
                    break;
                case rtsHitType.Unknown:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region DelegatesAndEvents
        public delegate void GeneralEventHandler();

        public delegate void ThreeParamPartyAllyBoolHandler(PartyManagerCore partyMan, AllyMemberCore lastMember, bool onDeath);
        public ThreeParamPartyAllyBoolHandler EventNoPartyManagers;
        #endregion

        public void ResetPartyStats()
        {
            PartyKills = 0;
            PartyPoints = 0;
            PartyDeaths = 0;
        }
    }
}