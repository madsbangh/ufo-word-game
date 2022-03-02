using UnityEngine;

namespace Components.Misc
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SetRandomSpriteColor : MonoBehaviour
    {
        [SerializeField] private Color[] _colors;
        
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            var randomIndex = Random.Range(0, _colors.Length);
            _spriteRenderer.color = _colors[randomIndex];
        }
    }
}
