using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BaseFramework;

namespace RTSCoreFramework
{
    public class RTSGameInstance : GameInstance
    {
        #region FieldsAndProps
        float transitionTime = 1f;
        string startTrigger = "Start";
        string endTrigger = "End";
        string FadeNameRef = "CrossFade";
        private Animator crossFadeAnimator
        {
            get
            {
                if (_crossFadeAnimator == null)
                {                                        
                    _crossFadeAnimator = CrossFadeTransform.GetComponent<Animator>();
                }
                return _crossFadeAnimator;
            }
        }
        private Animator _crossFadeAnimator = null;

        private Transform CrossFadeTransform
        {
            get
            {
                //Do not call ToggleCrossFadeGameObject From Here. 
                //Will Create Infinite Loop and Crash Unity.
                if (_CrossFadeTransform == null)
                {
                    //CrossFade Should Be A Child Of This Transform
                    foreach (Transform _child in transform)
                    {
                        if (_child.name.Contains(FadeNameRef))
                        {
                            //Found CrossFade Transform
                            _CrossFadeTransform = _child;
                        }
                    }
                }
                return _CrossFadeTransform;
            }
        }
        private Transform _CrossFadeTransform = null;
        #endregion

        #region OverrideAndHideProperties
        new public static RTSGameInstance thisInstance
        {
            get { return GameInstance.thisInstance as RTSGameInstance; }
        }
        #endregion

        #region UnityMessages
        // Use this for initialization
        protected override void OnEnable()
        {
            base.OnEnable();            
        }
        #endregion

        #region Overrides
        public override void LoadLevel(LevelIndex _level, ScenarioIndex _scenario)
        {
            StartCoroutine(FadeAndLoadLevelAfterDelay(_level, _scenario));
        }

        IEnumerator FadeAndLoadLevelAfterDelay(LevelIndex _level, ScenarioIndex _scenario)
        {
            ToggleCrossFadeGameObject(true);
            crossFadeAnimator.SetTrigger(startTrigger);
            yield return new WaitForSeconds(transitionTime);
            LoadLevelDelay(_level, _scenario);
            yield return new WaitForSeconds(transitionTime / 2);
            crossFadeAnimator.SetTrigger(endTrigger);
            yield return new WaitForSeconds(transitionTime);
            ToggleCrossFadeGameObject(false);
        }

        private void LoadLevelDelay(LevelIndex _level, ScenarioIndex _scenario)
        {
            //Make Sure to Call Base Method, and Not Override
            base.LoadLevel(_level, _scenario);
        }
        #endregion

        #region Helpers
        void ToggleCrossFadeGameObject(bool _enable)
        {
            CrossFadeTransform.gameObject.SetActive(_enable);
        }
        #endregion
    }
}