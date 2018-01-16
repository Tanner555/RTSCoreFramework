using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RTSCoreFramework
{
    /// <summary>
    /// Used to tell subs what type of object the Mouse Cursor is hitting
    /// </summary>
    public enum rtsHitType
    {
        Ally, Enemy, Cover, Walkable, Unwalkable, Unknown
    }

    public struct RTSRayCastDataObject
    {
        public rtsHitType _hitType;
        public RaycastHit _rayHit;
    }

    public class RTSCamRaycaster : MonoBehaviour
    {
        #region Properties
        RTSUiMaster uimaster
        {
            get { return RTSUiMaster.thisInstance; }
        }

        RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        RTSGameMode gamemode
        {
            get { return RTSGameMode.thisInstance; }
        }

        public static RTSCamRaycaster thisInstance { get; protected set; }

        bool noMoreChecking
        {
            get
            {
                return gamemode.GeneralInCommand == null ||
gamemode.GeneralInCommand.PartyMembers.Count <= 0;
            }
        }
        #endregion

        #region Fields
        //Tags
        [SerializeField]
        private string AllyTag = "Ally";
        [SerializeField]
        private string CoverTag = "Cover";

        //Method Fields
        private float maxRaycastDepth = 100f; // Hard coded value
        private Ray ray;
        private RaycastHit rayHit;
        private rtsHitType rayHitType = rtsHitType.Unknown;
        private rtsHitType rayHitTypeLastFrame = rtsHitType.Unknown;
        private GameObject gObject, gObjectRoot = null;
        private GameObject gObjectLastFrame = null;
        private string hitTag = "";
        private AllyMember hitAlly = null;

        //For event initialization checking
        bool hasStarted = false;
        #endregion

        #region UnityMessages
        private void OnEnable()
        {
            if (thisInstance == null)
                thisInstance = this;
            else if (hasStarted == false)
            {
                Debug.LogError("More than one RTS_CamRaycaster in scene!");
            }

            if (hasStarted == true)
            {
                gamemaster.GameOverEvent += DestroyRaycaster;
            }
        }

        private void OnDisable()
        {
            gamemaster.GameOverEvent -= DestroyRaycaster;
        }

        private void Start()
        {
            if (hasStarted == false)
            {
                gamemaster.GameOverEvent += DestroyRaycaster;
            }
            if (gamemode == null)
            {
                Debug.LogError("GameMode is Null!");
                Destroy(this);
            }
            if (uimaster == null)
            {
                Debug.LogError("UIMaster is Null!");
                Destroy(this);
            }

            hasStarted = true;
        }

        // Update is called once per frame
        void Update()
        {
            //Makes Sure Code is Valid Before Running
            if (TimeToReturn()) return;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out rayHit, maxRaycastDepth))
            {
                gObject = rayHit.collider.gameObject;
                gObjectRoot = rayHit.collider.gameObject.transform.root.gameObject;
                rayHitType = GetHitType();
                if (rayHitType != rayHitTypeLastFrame)
                {
                    //Layer has Changed
                    gamemaster.CallEventOnMouseCursorChange(rayHitType, rayHit);
                }
                gObjectLastFrame = gObject;
                rayHitTypeLastFrame = rayHitType;
            }
            else
            {
                if (gObjectLastFrame != null)
                {
                    gamemaster.CallEventOnMouseCursorChange(rtsHitType.Unknown, rayHit);
                }
                gObjectLastFrame = null;
                rayHitTypeLastFrame = rtsHitType.Unknown;
            }

        }
        #endregion

        #region RayFunctions
        public RTSRayCastDataObject GetRaycastInfo()
        {
            return new RTSRayCastDataObject { _hitType = rayHitType, _rayHit = rayHit };
        }

        rtsHitType GetHitType()
        {
            hitTag = gObjectRoot.tag;
            if (hitTag == AllyTag) return CheckAllyObject(gObjectRoot);
            else if (hitTag == CoverTag) return rtsHitType.Cover;
            else return CheckIfHitIsWalkable();
        }

        rtsHitType CheckAllyObject(GameObject gObjectRoot)
        {
            hitAlly = gObjectRoot.GetComponent<AllyMember>();
            if (hitAlly == null) return rtsHitType.Unknown;
            return gamemode.AllyIsGenCommanderMember(hitAlly) ?
                rtsHitType.Ally : rtsHitType.Enemy;
        }

        rtsHitType CheckIfHitIsWalkable()
        {
            return gamemode.isSurfaceReachableForAllyInCommand(rayHit) ?
                rtsHitType.Walkable : rtsHitType.Unwalkable;
        }

        bool TimeToReturn()
        {
            if (gamemode == null)
            {
                Debug.LogError("GameMode is Null!");
                Destroy(this);
                return true;
            }
            if (uimaster == null)
            {
                Debug.LogError("UIMaster is Null!");
                Destroy(this);
                return true;
            }
            if (uimaster.isUiAlreadyInUse) return true;
            if (noMoreChecking) return true;
            // Check if pointer is over an interactable UI element
            if (EventSystem.current.IsPointerOverGameObject()) return true;
            return false;
        }

        void DestroyRaycaster()
        {
            Destroy(this);
        }
        #endregion
    }
}