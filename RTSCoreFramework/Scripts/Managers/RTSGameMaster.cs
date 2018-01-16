using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public class RTSGameMaster : MonoBehaviour
    {
        #region Properties
        public static RTSGameMaster thisInstance
        {
            get; protected set;
        }

        public RTSGameMode gamemode
        {
            get { return RTSGameMode.thisInstance; }
        }

        public RTSCamRaycaster rayCaster { get { return RTSCamRaycaster.thisInstance; } }
        #endregion

        #region Fields
        public bool isGameOver;
        public bool isInventoryUIOn;
        public bool isMenuOn;
        #endregion

        #region UnityMessages
        private void OnEnable()
        {
            if (thisInstance != null)
                Debug.LogWarning("More than one instance of GameManagerMaster in scene.");
            else
                thisInstance = this;

            //Listen to Opsive TPC Events

        }

        private void OnDisable()
        {
            //UnSub from TPC Events

        }
        #endregion

        #region Events and Delegates
        public delegate void GameManagerEventHandler();
        public event GameManagerEventHandler RestartLevelEvent;
        public event GameManagerEventHandler GoToMenuSceneEvent;
        public event GameManagerEventHandler GameOverEvent;

        public delegate void OneBoolArgsHandler(bool enable);
        public event OneBoolArgsHandler EventEnableCameraMovement;
        //public event OneBoolArgsHandler EventEnableSelectionBox;

        //Used by CamRaycaster to broadcast mouse hit type
        public delegate void RtsHitTypeAndRayCastHitHandler(rtsHitType hitType, RaycastHit hit);
        public event RtsHitTypeAndRayCastHitHandler OnMouseCursorChange;
        public event RtsHitTypeAndRayCastHitHandler OnLeftClickSendHit;
        public event RtsHitTypeAndRayCastHitHandler OnRightClickSendHit;

        public delegate void AllyMemberHandler(AllyMember ally);
        //public event AllyMemberHandler OnHoverOverAlly;
        //public event AllyMemberHandler OnHoverOverEnemy;
        //public event AllyMemberHandler OnHoverLeaveAlly;
        //public event AllyMemberHandler OnHoverLeaveEnemy;
        public event AllyMemberHandler OnLeftClickAlly;
        public event AllyMemberHandler OnLeftClickEnemy;
        public event AllyMemberHandler OnRightClickAlly;
        public event AllyMemberHandler OnRightClickEnemy;

        public delegate void AllySwitchHandler(PartyManager _party, AllyMember _toSet, AllyMember _current);
        public event AllySwitchHandler OnAllySwitch;

        #endregion

        #region EventCalls
        public void CallEventRestartLevel()
        {
            if (RestartLevelEvent != null)
            {
                RestartLevelEvent();
            }
        }

        public void CallEventGoToMenuScene()
        {
            if (GoToMenuSceneEvent != null)
            {
                GoToMenuSceneEvent();
            }
        }

        public void CallEventGameOver()
        {
            if (GameOverEvent != null)
            {
                if (!isGameOver)
                {
                    isGameOver = true;
                    GameOverEvent();
                }
            }
        }

        public void CallEventEnableCameraMovement(bool enable)
        {
            if (EventEnableCameraMovement != null)
            {
                EventEnableCameraMovement(enable);
            }
        }

        public void CallOnAllySwitch(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {
            if (OnAllySwitch != null)
                OnAllySwitch(_party, _toSet, _current);
        }

        public void CallEventOnMouseCursorChange(rtsHitType hitType, RaycastHit hit)
        {
            bool isNull = hit.collider == null || hit.collider.gameObject == null ||
            hit.collider.gameObject.transform.root.gameObject == null;
            if (isNull) hitType = rtsHitType.Unknown;
            if (OnMouseCursorChange != null)
            {
                OnMouseCursorChange(hitType, hit);
            }

            bool _notAlly = hitType != rtsHitType.Ally && hitType != rtsHitType.Enemy;

            if (gamemode.hasPrevHighAlly && _notAlly)
            {
                gamemode.hasPrevHighAlly = false;
                if (gamemode.prevHighAlly == null) return;
                //if (OnHoverLeaveAlly != null) OnHoverLeaveAlly(gamemode.prevHighAlly);
                gamemode.prevHighAlly.npcMaster.CallEventOnHoverLeave(hitType, hit);
            }

            GameObject hitObjectRoot = null;
            if (hitType != rtsHitType.Unknown)
            {
                hitObjectRoot = hit.collider.gameObject.transform.root.gameObject;
            }

            switch (hitType)
            {
                case rtsHitType.Ally:
                    AllyMember _ally = hitObjectRoot.GetComponent<AllyMember>();
                    if (_ally == null) return;
                    gamemode.hasPrevHighAlly = true;
                    //if (OnHoverOverAlly != null) OnHoverOverAlly(_ally);
                    _ally.npcMaster.CallEventOnHoverOver(hitType, hit);
                    gamemode.prevHighAlly = _ally;
                    break;
                case rtsHitType.Enemy:
                    AllyMember _enemy = hitObjectRoot.GetComponent<AllyMember>();
                    if (_enemy == null) return;
                    gamemode.hasPrevHighAlly = true;
                    //if (OnHoverOverAlly != null) OnHoverOverAlly(_enemy);
                    _enemy.npcMaster.CallEventOnHoverOver(hitType, hit);
                    gamemode.prevHighAlly = _enemy;
                    break;
                case rtsHitType.Cover:
                    break;
                case rtsHitType.Walkable:
                    break;
                case rtsHitType.Unwalkable:
                    break;
                case rtsHitType.Unknown:
                    break;
                default:
                    break;
            }
        }

        public void CallEventOnLeftClickSendHit()
        {
            if (rayCaster != null && rayCaster.enabled == true)
            {
                var _info = rayCaster.GetRaycastInfo();
                CallEventOnLeftClickSendHit(_info._hitType, _info._rayHit);
            }
        }

        private void CallEventOnLeftClickSendHit(rtsHitType hitType, RaycastHit hit)
        {
            bool isNull = hit.collider == null || hit.collider.gameObject == null ||
            hit.collider.gameObject.transform.root.gameObject == null;
            if (isNull) hitType = rtsHitType.Unknown;
            if (OnLeftClickSendHit != null)
            {
                OnLeftClickSendHit(hitType, hit);
            }
            GameObject hitObjectRoot = null;
            if (hitType != rtsHitType.Unknown)
            {
                hitObjectRoot = hit.collider.gameObject.transform.root.gameObject;
            }

            switch (hitType)
            {
                case rtsHitType.Ally:
                    AllyMember _ally = hitObjectRoot.GetComponent<AllyMember>();
                    if (_ally == null) return;
                    if (OnLeftClickAlly != null) OnLeftClickAlly(_ally);
                    break;
                case rtsHitType.Enemy:
                    AllyMember _enemy = hitObjectRoot.GetComponent<AllyMember>();
                    if (_enemy == null) return;
                    if (OnLeftClickEnemy != null) OnLeftClickEnemy(_enemy);
                    break;
                case rtsHitType.Cover:
                    break;
                case rtsHitType.Walkable:
                    break;
                case rtsHitType.Unwalkable:
                    break;
                case rtsHitType.Unknown:
                    break;
                default:
                    break;
            }
        }

        public void CallEventOnRightClickSendHit()
        {
            if (rayCaster != null && rayCaster.enabled == true)
            {
                var _info = rayCaster.GetRaycastInfo();
                CallEventOnRightClickSendHit(_info._hitType, _info._rayHit);
            }
        }

        private void CallEventOnRightClickSendHit(rtsHitType hitType, RaycastHit hit)
        {
            bool isNull = hit.collider == null || hit.collider.gameObject == null ||
            hit.collider.gameObject.transform.root.gameObject == null;
            if (isNull) hitType = rtsHitType.Unknown;
            if (OnRightClickSendHit != null)
            {
                OnRightClickSendHit(hitType, hit);
            }
            GameObject hitObjectRoot = null;
            if (hitType != rtsHitType.Unknown)
            {
                hitObjectRoot = hit.collider.gameObject.transform.root.gameObject;
            }

            switch (hitType)
            {
                case rtsHitType.Ally:
                    AllyMember _ally = hitObjectRoot.GetComponent<AllyMember>();
                    if (_ally == null) return;
                    if (OnRightClickAlly != null) OnRightClickAlly(_ally);
                    break;
                case rtsHitType.Enemy:
                    AllyMember _enemy = hitObjectRoot.GetComponent<AllyMember>();
                    if (_enemy == null) return;
                    if (OnRightClickEnemy != null) OnRightClickEnemy(_enemy);
                    break;
                case rtsHitType.Cover:
                    break;
                case rtsHitType.Walkable:
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
    }
}