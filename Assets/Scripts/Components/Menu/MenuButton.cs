using UnityEngine;
using UnityEngine.UI;

namespace Components.Menu
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        private static readonly int XPropertyId = Animator.StringToHash("X");

        private Animator _animator;
        private Button _button;

        public bool IsX
        {
            set
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }

                _animator.SetBool(XPropertyId, value);
            }
        }

        public Button.ButtonClickedEvent OnClick
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button.onClick;
            }
        }
    }
}