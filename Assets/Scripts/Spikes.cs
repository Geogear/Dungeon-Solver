using UnityEngine;

public class Spikes : MonoBehaviour
{
    private static readonly string SpriteName = "spike-3";

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider2D;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(!_boxCollider2D.enabled &&
            _spriteRenderer.sprite.name == SpriteName)
        {
            _boxCollider2D.enabled = true;
            return;
        }

        if(_boxCollider2D.enabled &&
            _spriteRenderer.sprite.name != SpriteName)
        {
            _boxCollider2D.enabled = false;
        }
    }
}
