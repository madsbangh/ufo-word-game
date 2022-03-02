using UnityEngine;

namespace Components.Misc
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SetRandomSprite : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites;
        
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            var randomIndex = Random.Range(0, _sprites.Length);
            _spriteRenderer.sprite = _sprites[randomIndex];
        }
    }
}
