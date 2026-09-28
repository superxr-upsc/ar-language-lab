using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LessonCompleteWindow
{
    public class LessonCompleteView : ViewBase
    {
        public Button ContinueButton => _continueButton;
        [SerializeField] private Button _continueButton;
    }
}