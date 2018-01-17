using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    [CreateAssetMenu]
    public class IGBPIDataCore : ScriptableObject
    {
        public List<IGBPIPanelValue> IGBPIPanelData = new List<IGBPIPanelValue>();

    }

    [System.Serializable]
    public struct IGBPIPanelValue
    {
        public string order;
        public string condition;
        public string action;

        public IGBPIPanelValue(string order, string condition, string action)
        {
            this.order = order;
            this.condition = condition;
            this.action = action;
        }
    }
}