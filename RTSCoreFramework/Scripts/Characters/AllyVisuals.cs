using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTSCoreFramework
{
    public class AllyVisuals : MonoBehaviour
    {
        #region PropsAndFields
        [Header("Ally Highlighting")]
        public Color selAllyColor;
        public Color selEnemyColor;
        public Light SelectionLight;
        [Header("Ally Waypoint Navigation")]
        public Material waypointRendererMaterial;
        private LineRenderer waypointRenderer;

        RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        AllyMember thisAlly
        {
            get { return GetComponent<AllyMember>(); }
        }

        AllyEventHandler myEventHandler
        {
            get { return GetComponent<AllyEventHandler>(); }
        }

        RTSUiMaster uiMaster { get { return RTSUiMaster.thisInstance; } }

        bool friend
        {
            get { return thisAlly.bIsInGeneralCommanderParty; }
        }

        //NavMesh used for Waypoint Rendering
        NavMeshAgent myNavMesh
        {
            get
            {
                if (_myNavMesh == null)
                    _myNavMesh = GetComponent<NavMeshAgent>();

                return _myNavMesh;
            }
        }
        NavMeshAgent _myNavMesh = null;

        bool cameraIsMoving = false;
        #endregion

        #region UnityMessages
        private void OnDisable()
        {
            myEventHandler.OnHoverOver -= OnCursEnter;
            myEventHandler.OnHoverLeave -= OnCursExit;
            myEventHandler.EventAllyDied -= HandleDeath;
            myEventHandler.EventCommandMove -= SetupWaypointRenderer;
            myEventHandler.EventFinishedMoving -= DisableWaypointRenderer;
            gamemaster.GameOverEvent -= HandleGameOver;
            gamemaster.EventEnableCameraMovement -= HandleCameraMovement;
            uiMaster.EventAnyUIToggle -= HandleUIEnable;
        }
        // Use this for initialization
        void Start()
        {
            SelectionLight.enabled = false;
            myEventHandler.OnHoverOver += OnCursEnter;
            myEventHandler.OnHoverLeave += OnCursExit;
            myEventHandler.EventAllyDied += HandleDeath;
            myEventHandler.EventCommandMove += SetupWaypointRenderer;
            myEventHandler.EventFinishedMoving += DisableWaypointRenderer;
            gamemaster.GameOverEvent += HandleGameOver;
            gamemaster.EventEnableCameraMovement += HandleCameraMovement;
            uiMaster.EventAnyUIToggle += HandleUIEnable;
        }
        #endregion

        #region CursorHoverandExit
        void OnCursEnter(rtsHitType hitType, RaycastHit hit)
        {
            if (cameraIsMoving) return;
            SelectionLight.enabled = true;
            if (friend)
            {
                SelectionLight.color = selAllyColor;
            }
            else
            {
                SelectionLight.color = selEnemyColor;
            }

        }

        void OnCursExit(rtsHitType hitType, RaycastHit hit)
        {
            if (cameraIsMoving) return;
            SelectionLight.enabled = false;
        }

        #endregion

        #region Handlers
        void SetupWaypointRenderer(Vector3 _point)
        {
            Invoke("WaitToSetupWaypointRenderer", 0.1f);
            
        }

        void WaitToSetupWaypointRenderer()
        {
            if (myNavMesh == null || myNavMesh.path == null ||
                myEventHandler.bIsAIMoving)
                return;

            if (waypointRenderer != null && waypointRenderer.enabled == false)
            {
                waypointRenderer.enabled = true;
            }
            else if (waypointRenderer == null)
            {
                waypointRenderer = this.gameObject.AddComponent<LineRenderer>();
                if (waypointRendererMaterial != null)
                    waypointRenderer.material = waypointRendererMaterial;

                waypointRenderer.startWidth = 0.05f;
                waypointRenderer.endWidth = 0.05f;
                waypointRenderer.startColor = Color.yellow;
                waypointRenderer.endColor = Color.yellow;
            }

            var path = myNavMesh.path;

            waypointRenderer.positionCount = path.corners.Length;

            for (int i = 0; i < path.corners.Length; i++)
            {
                waypointRenderer.SetPosition(i, path.corners[i]);
            }
        }

        void DisableWaypointRenderer()
        {
            if(waypointRenderer != null)
            {
                waypointRenderer.enabled = false;
            }
        }

        void HandleDeath()
        {
            DestroyOnDeath();
        }

        void HandleGameOver()
        {
            DestroyOnDeath();
        }

        void DestroyOnDeath()
        {
            if (SelectionLight != null)
            {
                SelectionLight.enabled = true;
                Destroy(SelectionLight);
            }
            if (waypointRenderer != null)
            {
                waypointRenderer.enabled = true;
                Destroy(waypointRenderer);
            }
            Destroy(this);
        }

        void HandleCameraMovement(bool _isMoving)
        {
            cameraIsMoving = _isMoving;
            SelectionLight.enabled = false;
        }

        void HandleUIEnable(bool _enabled)
        {
            if (_enabled && SelectionLight != null && SelectionLight.enabled)
            {
                SelectionLight.enabled = false;
            }
        }
        #endregion

    }
}