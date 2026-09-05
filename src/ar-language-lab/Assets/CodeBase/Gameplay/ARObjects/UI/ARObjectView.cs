using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Gameplay.ARObjects.UI
{
    public class ARObjectView : ViewBase
    {
        public Button PlayAudioButton => _playAudioButton;
        
        [SerializeField] private TMP_Text _objectNameText;
        [SerializeField] private Button _playAudioButton;
        
        public void SetObjectName(string objectName) => 
            _objectNameText.text = objectName;
    }
}