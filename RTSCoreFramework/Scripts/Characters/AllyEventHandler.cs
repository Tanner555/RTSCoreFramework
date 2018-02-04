using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public class AllyEventHandler : MonoBehaviour
    {
        #region FieldsAndProps
        public bool isSprinting { get; protected set; }
        public bool bIsTacticsEnabled { get; protected set; }
        //Is moving through nav mesh agent, regardless of
        //whether it's ai or a command
        public bool bIsNavMoving { get { return bIsCommandMoving || bIsAIMoving; } }
        public bool bIsCommandMoving { get; protected set; }
        public bool bIsAIMoving { get; protected set; }
        public bool bIsFreeMoving { get; protected set; }
        public bool bIsCommandAttacking { get; protected set; }
        public bool bIsAiAttacking { get; protected set; }
        public bool bIsAimingToShoot { get; protected set; }
        public bool bCanEnableAITactics
        {
            get { return (bIsCommandMoving || 
                    bIsFreeMoving) == false && 
                    bIsCommandAttacking == false; }
        }
        
        #endregion

        #region DelegatesAndEvents
        public delegate void GeneralEventHandler();
        public delegate void GeneralOneBoolHandler(bool _enable);
        public event GeneralEventHandler EventAllyDied;
        public event GeneralEventHandler EventSwitchingFromCom;
        public event GeneralEventHandler EventPartySwitching;
        public event GeneralEventHandler EventSetAsCommander;
        public event GeneralEventHandler EventKilledEnemy;
        public event GeneralEventHandler EventStopTargettingEnemy;
        public event GeneralEventHandler EventFinishedMoving;
        public event GeneralEventHandler EventToggleIsSprinting;
        public event GeneralOneBoolHandler EventToggleAllyTactics;
        public event GeneralOneBoolHandler EventTogglebIsFreeMoving;
        public event GeneralOneBoolHandler EventToggleIsShooting;
        //Opsive TPC Events
        public event GeneralEventHandler OnSwitchToPrevItem;
        public event GeneralEventHandler OnSwitchToNextItem;
        public event GeneralEventHandler OnTryFire;
        public event GeneralEventHandler OnTryReload;
        public event GeneralEventHandler OnTryCrouch;
        public event GeneralOneBoolHandler OnTryAim;

        public delegate void RtsHitTypeAndRayCastHitHandler(rtsHitType hitType, RaycastHit hit);
        public event RtsHitTypeAndRayCastHitHandler OnHoverOver;
        public event RtsHitTypeAndRayCastHitHandler OnHoverLeave;

        public delegate void GeneralVector3Handler(Vector3 _point);
        public event GeneralVector3Handler EventCommandMove;

        public delegate void AllyHandler(AllyMember ally);
        public event AllyHandler EventCommandAttackEnemy;

        #endregion

        #region UnityMessages
        protected virtual void Awake()
        {
            isSprinting = true;
            bIsTacticsEnabled = false;
            bIsAimingToShoot = false;
            bIsFreeMoving = false;
            bIsCommandAttacking = false;
            bIsAiAttacking = false;
        }

        protected virtual void OnDisable()
        {

        }
        #endregion

        #region EventCalls
        public virtual void CallEventAllyDied()
        {
            if (EventAllyDied != null)
            {
                EventAllyDied();
                this.enabled = false;
            }
        }

        public void CallEventToggleIsShooting(bool _enable)
        {
            bIsAimingToShoot = _enable;
            if (EventToggleIsShooting != null)
            {
                EventToggleIsShooting(_enable);
            }
        }

        public void CallEventToggleIsSprinting()
        {
            isSprinting = !isSprinting;
            if (EventToggleIsSprinting != null) EventToggleIsSprinting();
        }

        public void CallEventFinishedMoving()
        {
            bIsCommandMoving = false;
            bIsAIMoving = false;
            if (bCanEnableAITactics)
            {
                CallEventToggleAllyTactics(true);
            }          
            if (EventFinishedMoving != null) EventFinishedMoving();
        }

        public void CallOnSwitchToPrevItem()
        {
            if (OnSwitchToPrevItem != null) OnSwitchToPrevItem();
        }

        public void CallOnSwitchToNextItem()
        {
            if (OnSwitchToNextItem != null) OnSwitchToNextItem();
        }

        public void CallOnTryFire()
        {
            if (OnTryFire != null) OnTryFire();
        }

        public void CallOnTryReload()
        {
            if (OnTryReload != null) OnTryReload();
        }

        public void CallOnTryCrouch()
        {
            if (OnTryCrouch != null) OnTryCrouch();
        }

        public void CallOnTryAim(bool _enable)
        {
            if (OnTryAim != null) OnTryAim(_enable);
        }

        public void CallEventSwitchingFromCom()
        {
            if (bIsFreeMoving) CallEventTogglebIsFreeMoving(false);
            if (EventSwitchingFromCom != null) EventSwitchingFromCom();
        }

        public void CallEventPartySwitching()
        {
            if (EventPartySwitching != null) EventPartySwitching();
        }

        public void CallEventSetAsCommander()
        {
            CallEventFinishedMoving();
            if (EventSetAsCommander != null) EventSetAsCommander();
        }

        public void CallEventKilledEnemy()
        {
            if (EventKilledEnemy != null) EventKilledEnemy();
        }

        public void CallEventOnHoverOver(rtsHitType hitType, RaycastHit hit)
        {
            if (OnHoverOver != null)
            {
                OnHoverOver(hitType, hit);
            }
        }

        public void CallEventOnHoverLeave(rtsHitType hitType, RaycastHit hit)
        {
            if (OnHoverLeave != null)
            {
                OnHoverLeave(hitType, hit);
            }
        }

        public void CallEventCommandMove(rtsHitType hitType, RaycastHit hit)
        {
            bIsAimingToShoot = bIsCommandAttacking = bIsAiAttacking = false;
            bIsCommandMoving = true;
            bIsAIMoving = false;
            CallEventCommandMove(hit.point);
        }

        public void CallEventAIMove(Vector3 _point)
        {
            bIsAimingToShoot = false;
            bIsCommandAttacking = false;
            bIsAIMoving = true;
            bIsCommandMoving = false;
            CallEventCommandMove(_point);
        }

        private void CallEventCommandMove(Vector3 _point)
        {
            if (EventCommandMove != null) EventCommandMove(_point);
        }

        public void CallEventPlayerCommandAttackEnemy(AllyMember ally)
        {
            bIsAIMoving = bIsCommandMoving = false;
            bIsCommandAttacking = true;
            bIsAiAttacking = false;
            CallEventCommandAttackEnemy(ally);
        }

        public void CallEventAICommandAttackEnemy(AllyMember ally)
        {
            bIsAIMoving = bIsCommandMoving = false;
            bIsAiAttacking = true;
            bIsCommandAttacking = false;
            CallEventCommandAttackEnemy(ally);
        }

        private void CallEventCommandAttackEnemy(AllyMember ally)
        {
            if (EventCommandAttackEnemy != null)
            {
                EventCommandAttackEnemy(ally);
            }
        }

        public void CallEventStopTargettingEnemy()
        {
            bIsCommandAttacking = bIsAiAttacking = bIsAimingToShoot = false;
            if (EventStopTargettingEnemy != null) EventStopTargettingEnemy();
        }

        public void CallEventTogglebIsFreeMoving(bool _enable)
        {
            bIsFreeMoving = _enable;
            //If Free Moving Is Enabled, Diable Tactics
            bool _enableTactics = !_enable && bCanEnableAITactics;
            CallEventToggleAllyTactics(_enableTactics);
            if (EventTogglebIsFreeMoving != null) EventTogglebIsFreeMoving(_enable);
        }

        //Event handler controls bIsTacticsEnabled, makes code more centralized
        private void CallEventToggleAllyTactics(bool _enable)
        {
            bIsTacticsEnabled = _enable;
            if (EventToggleAllyTactics != null) EventToggleAllyTactics(_enable);
        }
        #endregion

    }
}