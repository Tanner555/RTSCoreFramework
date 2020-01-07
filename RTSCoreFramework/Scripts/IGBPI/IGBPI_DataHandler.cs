using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseFramework;

namespace RTSCoreFramework
{
    public class IGBPI_DataHandler : BaseSingleton<IGBPI_DataHandler>
    {
        #region Enums
        public ConditionFilters conditionFilter { get; protected set; }
        public ActionFilters actionFilter { get; protected set; }

        public ConditionFilters NavForwardCondition()
        {
            var _filters = Enum.GetValues(typeof(ConditionFilters));
            if ((int)conditionFilter < _filters.Length - 1)
            {
                conditionFilter++;
            }
            else
            {
                conditionFilter = (ConditionFilters)0;
            }
            return conditionFilter;
        }

        public ConditionFilters NavPreviousCondition()
        {
            var _filters = Enum.GetValues(typeof(ConditionFilters));
            if ((int)conditionFilter > 0)
            {
                conditionFilter--;
            }
            else
            {
                conditionFilter = (ConditionFilters)_filters.Length - 1;
            }
            return conditionFilter;
        }

        public ActionFilters NavForwardAction()
        {
            var _filters = Enum.GetValues(typeof(ActionFilters));
            if ((int)actionFilter < _filters.Length - 1)
            {
                actionFilter++;
            }
            else
            {
                actionFilter = (ActionFilters)0;
            }
            return actionFilter;
        }

        public ActionFilters NavPreviousAction()
        {
            var _filters = Enum.GetValues(typeof(ActionFilters));
            if ((int)actionFilter > 0)
            {
                actionFilter--;
            }
            else
            {
                actionFilter = (ActionFilters)_filters.Length - 1;
            }
            return actionFilter;
        }

        #endregion

        #region Properties
        protected RTSGameMode gamemode { get { return RTSGameMode.thisInstance; } }
        #endregion

        #region ConditionDictionary
        public virtual Dictionary<string, IGBPI_Condition> IGBPI_Conditions
        {
            get { return _IGBPI_Conditions; }
        }

        protected Dictionary<string, IGBPI_Condition> _IGBPI_Conditions = new Dictionary<string, IGBPI_Condition>()
        {
            {"Self: Any", new IGBPI_Condition((_self, _ai) => (true, _self), ConditionFilters.Standard) },
            {"Leader: Not Within Follow Distance", new IGBPI_Condition((_self, _ai) =>
            { return (!_ai.IsWithinFollowingDistance(), _self.allyInCommand); }, ConditionFilters.Standard) },
            {"Leader: Within Follow Distance", new IGBPI_Condition((_self, _ai) =>
            { return (_self.aiController.IsWithinFollowingDistance(), _self.allyInCommand); }, ConditionFilters.Standard) },
            {"Self: Health < 100", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 1, _self); }, ConditionFilters.AllyHealth) },
            {"Self: Health < 90", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 0.90, _self); }, ConditionFilters.AllyHealth) },
            {"Self: Health < 75", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 0.75, _self); }, ConditionFilters.AllyHealth) },
            {"Self: Health < 50", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 0.50, _self); }, ConditionFilters.AllyHealth) },
            {"Self: Health < 25", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 0.25, _self); }, ConditionFilters.AllyHealth) },
            {"Self: Health < 10", new IGBPI_Condition((_self, _ai) =>
            { return (_self.HealthAsPercentage < 0.10, _self); }, ConditionFilters.AllyHealth) },
            {"Self: CurAmmo < 10", new IGBPI_Condition((_self, _ai) =>
            { return (_self.CurrentEquipedAmmo < 10, _self); }, ConditionFilters.AllyGun) },
            {"Self: CurAmmo = 0", new IGBPI_Condition((_self, _ai) =>
            { return (_self.CurrentEquipedAmmo == 0, _self); }, ConditionFilters.AllyGun) },
            {"Self: CurAmmo > 0", new IGBPI_Condition((_self, _ai) =>
            { return (_self.CurrentEquipedAmmo > 0, _self); }, ConditionFilters.AllyGun) },
            {"Enemy: WithinSightRange", new IGBPI_Condition((_self, _ai) =>
            { return _ai.Tactics_IsEnemyWithinSightRange(); }, ConditionFilters.TargetedEnemy)  },
        };
        #endregion

        #region ActionDictionary
        public virtual Dictionary<string, RTSActionItem> IGBPI_Actions
        {
            get { return _IGBPI_Actions; }
        }

        protected Dictionary<string, RTSActionItem> _IGBPI_Actions = new Dictionary<string, RTSActionItem>()
        {
            {"Self: Attack Targetted Enemy", new RTSActionItem((_self, _ai, _target) =>
                { _ai.AttackTargettedEnemy(_self, _ai, _target); },
                //(_self, _ai, _target) => { return _self.bIsCarryingMeleeWeapon || _self.CurrentEquipedAmmo > 0; },
                (_self, _ai, _target) => true,
            ActionFilters.AI, (_self, _ai, _target) => _ai.ResetTargetting()) },
            {"Self: Attack Nearest Enemy", new RTSActionItem((_self, _ai, _target) =>
            { _ai.Tactics_AttackClosestEnemy(); }, (_self, _ai, _target) => true, 
                ActionFilters.AI, (_self, _ai, _target) => _ai.ResetTargetting()) },
            {"Self: SwitchToNextWeapon", new RTSActionItem((_self, _ai, _target) =>
            { _self.allyEventHandler.CallOnSwitchToNextItem(); },
                (_self, _ai, _target) => true,
                ActionFilters.Weapon, (_self, _ai, _target) => { }) },
            {"Self: SwitchToPrevWeapon", new RTSActionItem((_self, _ai, _target) =>
            { _self.allyEventHandler.CallOnSwitchToPrevItem(); },
                (_self, _ai, _target) => true,
                ActionFilters.Weapon, (_self, _ai, _target) => { }) },
            {"Self: FollowLeader", new RTSActionItem((_self, _ai, _target) =>
            { _ai.Tactics_MoveToLeader(); },
                (_self, _ai, _target) => true,
                ActionFilters.Movement, (_self, _ai, _target) => _ai.FinishMoving()) },
            {"Debug: Log True Message", new RTSActionItem((_self, _ai, _target) =>
            Debug.Log("Condition is true, called from: " + _self.CharacterName),
                (_self, _ai, _target) => true,
                ActionFilters.Debugging, (_self, _ai, _target) => { }) }
        };
        #endregion

        #region UnityMessages
        protected virtual void OnEnable()
        {

        }
        // Use this for initialization
        protected virtual void Start()
        {

        }

        protected virtual void OnDisable()
        {

        }
        #endregion

        #region ConditionHelpers

        #endregion

        #region ActionHelpers

        #endregion

        #region Structs
        public struct IGBPI_Condition
        {
            public Func<AllyMember, AllyAIController, (bool _success, AllyMember _target)> action;
            public ConditionFilters filter;

            public IGBPI_Condition(Func<AllyMember, AllyAIController, (bool, AllyMember)> action, ConditionFilters filter)
            {
                this.action = action;
                this.filter = filter;
            }
        }

        //public struct IGBPI_Action
        //{
        //    public Action<AllyMember> action;
        //    public Func<AllyMember, bool> canPerformAction;
        //    public ActionFilters filter;

        //    public IGBPI_Action(Action<AllyMember> action, 
        //        Func<AllyMember, bool> canPerformAction, 
        //        ActionFilters filter)
        //    {
        //        this.action = action;
        //        this.canPerformAction = canPerformAction;
        //        this.filter = filter;
        //    }
        //}
        #endregion
    }

    #region OutsideClassEnums
    public enum ConditionFilters
    {
        Standard = 0, AllyHealth = 1, AllyGun = 2,
        TargetedEnemy = 3, AllyStamina = 4,
        AllyAbilities = 5
    }
    public enum ActionFilters
    {
        Movement = 0, Weapon = 1, AI = 2,
        Debugging = 3, Abilities = 4
    }
    #endregion
}