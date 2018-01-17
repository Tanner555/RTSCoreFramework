using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTSCoreFramework;

namespace RTSCoreFramework.ExtraFeatures
{
    public class RTSCursorToggle : MonoBehaviour
    {
        private RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        private void Start()
        {
            gamemaster.EventEnableCameraMovement += setCursorLock;
        }

        private void OnDisable()
        {
            gamemaster.EventEnableCameraMovement -= setCursorLock;
        }

        void ToggleCursorState()
        {
            bool _isLocked = isCursorLockedAndNotVisible();
            setCursorLock(!_isLocked);
        }

        void ToggleCursorState(bool _state)
        {
            setCursorLock(!_state);
        }

        private bool isCursorLockedAndNotVisible()
        {
            //Cursor is Locked and Not Visible
            bool _isLocked = Cursor.lockState == CursorLockMode.Locked;
            return _isLocked && !Cursor.visible;
        }

        private void setCursorLock(bool _lock)
        {
            if (_lock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}