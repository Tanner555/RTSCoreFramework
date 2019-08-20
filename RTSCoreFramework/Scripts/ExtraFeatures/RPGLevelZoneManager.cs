using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RTSCoreFramework
{
    public class RPGLevelZoneManager : MonoBehaviour
    {
        #region FieldsAndProps
        public List<GameObject> LevelZoneList = new List<GameObject>();
        private Dictionary<GameObject, BoxCollider> LevelZoneBoundDictionary = new Dictionary<GameObject, BoxCollider>();

        GameObject player = null;
        float repeatRate = 0.5f;

        GameObject currentZone = null;
        BoxCollider currentBoxCollider = null;

        public LayerMask WaterLayers
        {
            get { return LayerMask.NameToLayer("Water"); }
        }
        public LayerMask IgnoreRaycastLayer => LayerMask.NameToLayer("Ignore Raycast");

        RTSGameMode gamemode
        {
            get { return RTSGameMode.thisInstance; }
        }

        RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }
        #endregion

        #region UnityMessages
        // Start is called before the first frame update
        void Start()
        {
            if (LevelZoneList != null && LevelZoneList.Count > 0)
            {
                for (int i = 0; i < LevelZoneList.Count; i++)
                {
                    var _zone = LevelZoneList[i];
                    var _bounds = new Bounds(_zone.transform.position, Vector3.zero);

                    foreach (var _childRenderer in _zone.GetComponentsInChildren<BoxCollider>())
                    {
                        _bounds.Encapsulate(_childRenderer.bounds);
                    }

                    var _childObject = new GameObject($"{_zone.name} - Zone Bounds Manager");
                    _childObject.layer = IgnoreRaycastLayer;
                    _childObject.transform.position = _bounds.center;
                    _childObject.transform.parent = transform;
                    var _collider = _childObject.AddComponent<BoxCollider>();
                    _collider.isTrigger = true;
                    //Increase the height of all colliders
                    _bounds.size = _bounds.size + new Vector3(25, 30, 25);
                    _collider.size = _bounds.size;
                    LevelZoneBoundDictionary.Add(_zone, _collider);
                }
            }
        }

        private void OnEnable()
        {
            gamemaster.OnAllySwitch += OnAllySwitch;
        }

        private void OnDisable()
        {
            CancelInvoke();
            gamemaster.OnAllySwitch -= OnAllySwitch;
        }
        #endregion

        #region Handlers
        void OnAllySwitch(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {
            if (_party.bIsCurrentPlayerCommander)
            {
                player = _toSet.gameObject;
                if (IsInvoking("SE_CheckPlayerInZoneBounds") == false)
                {
                    InvokeRepeating("SE_CheckPlayerInZoneBounds", 0.5f, repeatRate);
                }
            }
        }
        #endregion

        #region Services
        void SE_CheckPlayerInZoneBounds()
        {
            if (player == null || LevelZoneBoundDictionary == null || LevelZoneBoundDictionary.Count <= 0) return;

            for (int i = 0; i < LevelZoneBoundDictionary.Count; i++)
            {
                currentZone = LevelZoneBoundDictionary.Keys.ElementAt(i);
                currentBoxCollider = LevelZoneBoundDictionary.Values.ElementAt(i);
                if (currentBoxCollider.bounds.Contains(player.transform.position))
                {
                    if (currentZone.activeSelf == false)
                    {
                        currentZone.SetActive(true);
                    }
                }
                else
                {
                    if (currentZone.activeSelf == true)
                    {
                        currentZone.SetActive(false);
                    }
                }
            }
        }
        #endregion

    }
}