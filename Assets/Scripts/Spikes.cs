using UnityEngine;

public class Spikes : MonoBehaviour
{
    private static readonly string SpriteName = "spike-3";
    private static readonly float[]  TrapDeadliness = { 0.5f, 1.0f, 2.0f };

    private static float _nextTrapMultiplier = 1.0f;
    private static float _baseDamage = 2.0f;
    private static float _waitTimeMin = 1f;
    private static float _waitTimeMax = 4f;

    private float _damageMultiplier = 1.0f;
    private float _waitTime = 0.0f;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider2D;
    private Animator _animator;

    static public void SetDamageMultiplierForNext(FilledType ft)
    {
        int deadlinessIndex = (ft >= FilledType.TrapLow && ft <= FilledType.TrapHigh)
            ? (int)ft : (int)FilledType.TrapLow;
        _nextTrapMultiplier = TrapDeadliness[deadlinessIndex-(int)FilledType.TrapLow];
    }

    public int SendDamage()
    {
        return (_spriteRenderer.sprite.name == SpriteName) ?
            Mathf.RoundToInt(_baseDamage * _damageMultiplier)
        : 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _damageMultiplier = _nextTrapMultiplier;
        _waitTime = LevelGenerator.RandomFloat(_waitTimeMin, _waitTimeMax);
    }

    // Update is called once per frame
    void Update()
    {
        if(!_animator.enabled)
        {
            _waitTime -= Time.deltaTime;
            _animator.enabled = _waitTime <= float.Epsilon;
            return;
        }
    }

}
