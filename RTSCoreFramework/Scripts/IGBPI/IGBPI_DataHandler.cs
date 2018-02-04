using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTSCoreFramework
{
    public class IGBPI_DataHandler : MonoBehaviour
    {
        #region Enums
        public enum ConditionFilters
        {
            Standard, AllyHealth, AllyGun,
            TargetedEnemyHealth, TargetedEnemyGun
        }
        public enum ActionFilters
        {
            Movement, Weapon, AI, Debugging
        }

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
        public static IGBPI_DataHandler thisInstance { get; protected set; }
        RTSGameMode gamemode { get { return RTSGameMode.thisInstance; } }
        #endregion

        #region ConditionDictionary
        public Dictionary<string, IGBPI_Condition> IGBPI_Conditions = new Dictionary<string, IGBPI_Condition>()
        {
            {"Self: Any", new IGBPI_Condition((_ally) => true, ConditionFilters.Standard) },
            {"Leader: Not Within Follow Distance", new IGBPI_Condition((_ally) => 
            { return !_ally.aiController.IsWithinFollowingDistance(); }, ConditionFilters.Standard) },
            {"Leader: Within Follow Distance", new IGBPI_Condition((_ally) =>
            { return _ally.aiController.IsWithinFollowingDistance(); }, ConditionFilters.Standard) },
            {"Self: Health < 100", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 100; }, ConditionFilters.AllyHealth) },
            {"Self: Health < 90", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 90; }, ConditionFilters.AllyHealth) },
            {"Self: Health < 75", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 75; }, ConditionFilters.AllyHealth) },
            {"Self: Health < 50", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 50; }, ConditionFilters.AllyHealth) },
            {"Self: Health < 25", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 25; }, ConditionFilters.AllyHealth) },
            {"Self: Health < 10", new IGBPI_Condition((_ally) =>
            { return _ally.AllyHealth < 10; }, ConditionFilters.AllyHealth) },
            {"Self: CurAmmo < 10", new IGBPI_Condition((_ally) =>
            { return _ally.CurrentEquipedAmmo < 10; }, ConditionFilters.AllyGun) },
            {"Self: CurAmmo = 0", new IGBPI_Condition((_ally) =>
            { return _ally.CurrentEquipedAmmo == 0; }, ConditionFilters.AllyGun) },
            {"Self: CurAmmo > 0", new IGBPI_Condition((_ally) =>
            { return _ally.CurrentEquipedAmmo > 0; }, ConditionFilters.AllyGun) }
        };
        #endregion

        #region ActionDictionary
        public Dictionary<string, IGBPI_Action> IGBPI_Actions = new Dictionary<string, IGBPI_Action>()
        {
            {"Self: Attack Targetted Enemy", new IGBPI_Action((_ally) =>
            { _ally.aiController.AttackTargettedEnemy(); }, ActionFilters.AI) },
            {"Self: Attack Nearest Enemy", new IGBPI_Action((_ally) =>
            { _ally.aiController.Tactics_AttackClosestEnemy(); }, ActionFilters.AI) },
            {"Self: SwitchToNextWeapon", new IGBPI_Action((_ally) =>
            { _ally.allyEventHandler.CallOnSwitchToNextItem(); }, ActionFilters.Weapon) },
            {"Self: SwitchToPrevWeapon", new IGBPI_Action((_ally) =>
            { _ally.allyEventHandler.CallOnSwitchToPrevItem(); }, ActionFilters.Weapon) },
            {"Self: FollowLeader", new IGBPI_Action((_ally) =>
            { _ally.aiController.Tactics_MoveToLeader(); }, ActionFilters.Movement) },
            {"Debug: Log True Message", new IGBPI_Action((_ally) => 
            Debug.Log("Condition is true, called from: " + _ally), ActionFilters.Debugging) }
        };
        #endregion

        #region UnityMessages
        private void OnEnable()
        {
            if (thisInstance == null)
                thisInstance = this;
            else
                Debug.LogError("More than one DataHandler in scene");
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
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
            public Func<AllyMember, bool> action;
            public ConditionFilters filter;

            public IGBPI_Condition(Func<AllyMember, bool> action, ConditionFilters filter)
            {
                this.action = action;
                this.filter = filter;
            }
        }

        public struct IGBPI_Action
        {
            public Action<AllyMember> action;
            public ActionFilters filter;

            public IGBPI_Action(Action<AllyMember> action, ActionFilters filter)
            {
                this.action = action;
                this.filter = filter;
            }
        }
        #endregion
    }
}