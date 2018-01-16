using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTSCoreFramework
{
    public class RTSSaveManager : MonoBehaviour
    {
        [SerializeField]
        private IGBPI_Data IGBPIDataObject;
        public static RTSSaveManager thisInstance { get; protected set; }
        IGBPI_DataHandler dataHandler { get { return IGBPI_DataHandler.thisInstance; } }

        private void OnEnable()
        {
            if (thisInstance != null)
                Debug.LogError("More than one save manager in scene");
            else
                thisInstance = this;

        }

        public List<IGBPIPanelValue> Load_IGBPI_PanelValues()
        {
            if (!isIGBPISavingPermitted()) return null;
            return ValidateIGBPIValues(IGBPIDataObject.IGBPIPanelData);
        }

        public List<IGBPIPanelValue> ValidateIGBPIValues(List<IGBPIPanelValue> _values)
        {
            List<IGBPIPanelValue> _validValues = new List<IGBPIPanelValue>();
            bool _changedSaveFile = false;
            foreach (var _data in _values)
            {
                bool _hasCondition = dataHandler.IGBPI_Conditions.ContainsKey(_data.condition);
                bool _hasAction = dataHandler.IGBPI_Actions.ContainsKey(_data.action);
                int _order = -1;
                bool _hasOrder = int.TryParse(_data.order, out _order) && _order != -1;
                if (_hasCondition && _hasAction && _hasOrder)
                    _validValues.Add(_data);
                else
                    _changedSaveFile = true;
            }
            if (_changedSaveFile) Debug.Log("Loaded Save File Contents will change on next save.");
            return _validValues;
        }

        public void Save_IGBPI_Values(List<IGBPIPanelValue> _values)
        {
            if (!isIGBPISavingPermitted()) return;
            IGBPIDataObject.IGBPIPanelData.Clear();
            IGBPIDataObject.IGBPIPanelData = ValidateIGBPIValues(_values);

            EditorUtility.SetDirty(IGBPIDataObject);
            AssetDatabase.SaveAssets();
        }

        public IEnumerator YieldSave_IGBPI_Values(List<IGBPIPanelValue> _values)
        {
            Save_IGBPI_Values(_values);
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Finished Saving");
        }

        public void Save_IGBPI_PanelValues(List<IGBPI_UI_Panel> _panels)
        {
            if (!isIGBPISavingPermitted()) return;
            List<IGBPIPanelValue> _saveValues = new List<IGBPIPanelValue>();
            foreach (var _panel in _panels)
            {
                _saveValues.Add(new IGBPIPanelValue(
                    _panel.orderText.text,
                    _panel.conditionText.text,
                    _panel.actionText.text
                ));
            }
            Save_IGBPI_Values(_saveValues);
        }

        bool isIGBPISavingPermitted()
        {
            if (IGBPIDataObject == null)
            {
                Debug.LogError("No IGBPI Data Object on Save Manager");
                return false;
            }
            if (dataHandler == null)
            {
                Debug.LogError("No Data Handler could be found.");
                return false;
            }
            return true;
        }
    }
}