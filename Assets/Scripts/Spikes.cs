using UnityEngine;

public class Spikes : MonoBehaviour
{
    private static readonly string SpriteName = "spike-3";
    private static readonly float[]  TrapDeadliness = { 0.5f, 1.0f, 2.0f };

    private static float _nextTrapMultiplier = 1.0f;
    private static float _baseDamage = 2.0f;

    private float _damageMultiplier = 1.0f;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider2D;

    static public void SetDamageMultiplierForNext(FilledType ft)
    {
        int deadlinessIndex = (ft >= FilledType.TrapLow && ft <= FilledType.TrapHigh)
            ? (int)ft : (int)FilledType.TrapLow;
        _nextTrapMultiplier = TrapDeadliness[deadlinessIndex-(int)FilledType.TrapLow];
    }

    public int SendDamage()
    {
        return Mathf.RoundToInt(_baseDamage * _damageMultiplier);
    }

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _damageMultiplier = _nextTrapMultiplier;
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
