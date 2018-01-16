using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public class RTSUiMaster : MonoBehaviour
    {
        #region DelegatesAndEvents
        public delegate void GeneralEventHandler();
        public delegate void MenuToggleHandler(bool enable);
        public event MenuToggleHandler EventMenuToggle;
        public event MenuToggleHandler EventInventoryUIToggle;
        public event MenuToggleHandler EventIGBPIToggle;
        public event MenuToggleHandler EventAnyUIToggle;
        //IGBPI Events
        public delegate void UI_PanelHandler(IGBPI_UI_Panel _info);
        public delegate void UI_MovePanelHandler(IGBPI_UI_Panel _info, int _order);
        public event UI_PanelHandler EventRemoveDropdownInstance;
        public event UI_PanelHandler EventUIPanelSelectionChanged;
        public event UI_PanelHandler EventResetPanelUIMenu;
        public event UI_MovePanelHandler EventMovePanelUI;
        public event GeneralEventHandler EventAddDropdownInstance;
        public event GeneralEventHandler EventResetAllPaneUIMenus;
        public event GeneralEventHandler EventReorderIGBPIPanels;
        public event GeneralEventHandler EventOnSaveIGBPIComplete;
        #endregion

        #region Properties
        public static RTSUiMaster thisInstance
        {
            get; protected set;
        }

        public RTSUiManager uiManager
        {
            get { return RTSUiManager.thisInstance; }
        }

        public RTSCamRaycaster rayCaster { get { return RTSCamRaycaster.thisInstance; } }

        //For Ui Conflict Checking
        public bool isUiAlreadyInUse
        {
            get { return isInventoryUIOn || isPauseMenuOn || isIGBPIOn; }
        }

        public bool isInventoryUIOn
        {
            get { return uiManager.InventoryUi.activeSelf; }
        }
        public bool isPauseMenuOn
        {
            get { return uiManager.PauseMenuUi.activeSelf; }
        }
        public bool isIGBPIOn
        {
            get { return uiManager.IGBPIUi.activeSelf; }
        }
        #endregion

        #region Fields
        public bool isDraggingIGBPI = false;
        #endregion

        #region UnityMessages
        // Use this for initialization
        private void OnEnable()
        {
            if (thisInstance != null)
                Debug.LogWarning("More than one instance of UIManagerMaster in scene.");
            else
                thisInstance = this;
        }
        #endregion

        #region EventCalls
        public void CallEventMenuToggle()
        {
            if (EventMenuToggle != null /*&& !isUiAlreadyInUse*/)
            {
                CallEventAnyUIToggle(!isPauseMenuOn);
                EventMenuToggle(!isPauseMenuOn);
                EnableRayCaster(!isPauseMenuOn);
            }
        }

        public void CallEventInventoryUIToggle()
        {
            if (EventInventoryUIToggle != null /*&& !isUiAlreadyInUse*/)
            {
                CallEventAnyUIToggle(!isInventoryUIOn);
                EventInventoryUIToggle(!isInventoryUIOn);
                EnableRayCaster(!isInventoryUIOn);
            }
        }

        public void CallEventIGBPIToggle()
        {
            if (EventIGBPIToggle != null /*&& !isUiAlreadyInUse*/)
            {
                CallEventAnyUIToggle(!isIGBPIOn);
                EventIGBPIToggle(!isIGBPIOn);
                EnableRayCaster(!isIGBPIOn);
            }
        }

        private void CallEventAnyUIToggle(bool _enabled)
        {
            if (EventAnyUIToggle != null) EventAnyUIToggle(_enabled);
        }

        //IGBPI
        public void CallEventAddDropdownInstance()
        {
            if (EventAddDropdownInstance != null)
            {
                EventAddDropdownInstance();
            }
        }

        public void CallEventRemoveDropdownInstance(IGBPI_UI_Panel _info)
        {
            if (EventRemoveDropdownInstance != null)
            {
                EventRemoveDropdownInstance(_info);
            }
        }

        public void CallEventUIPanelSelectionChanged(IGBPI_UI_Panel _info)
        {
            if (EventUIPanelSelectionChanged != null)
            {
                EventUIPanelSelectionChanged(_info);
            }
        }

        public void CallEventResetPanelUIMenu(IGBPI_UI_Panel _info)
        {
            if (EventResetPanelUIMenu != null)
            {
                EventResetPanelUIMenu(_info);
            }
        }

        public void CallEventMovePanelUI(IGBPI_UI_Panel _info, int _order)
        {
            if (EventMovePanelUI != null)
                EventMovePanelUI(_info, _order);
        }

        public void CallEventResetAllPanelUIMenus()
        {
            if (EventResetAllPaneUIMenus != null)
            {
                EventResetAllPaneUIMenus();
            }
        }

        public void CallEventReorderIGBPIPanels()
        {
            if (EventReorderIGBPIPanels != null)
                EventReorderIGBPIPanels();
        }

        public void CallEventOnSaveIGBPIComplete()
        {
            if (EventOnSaveIGBPIComplete != null)
                EventOnSaveIGBPIComplete();
        }
        #endregion

        #region Helpers
        void EnableRayCaster(bool _enable)
        {
            if (rayCaster != null) rayCaster.enabled = _enable;
        }
        #endregion
    }
}