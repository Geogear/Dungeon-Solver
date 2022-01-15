using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Transform _playerTransform = null;

    private const float FaceTurningCap = 0.5f;

    [SerializeField] private float _attackDamage = 5.0f;
    [SerializeField] private float _attackRange = 0.35f;
    [SerializeField] private float _attackRate = 1.0f;
    [SerializeField] private float _spellDamage = 3.0f;
    [SerializeField] private float _spellRange = 4.0f;
    [SerializeField] private float _spellRate = 0.7f;
    [SerializeField] private int _maxHitPoints = 15;
    [SerializeField] private int _hitPoints = 15;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Transform _attackLocation;

    private SpriteRenderer _spriteRenderer = null;
    private BoxCollider2D _boxCollider2D = null;
    private Animator _animator = null;
    private float _nextAttackTime = 0.0f;
    private float _nextSpellTime = 0.0f;
    private bool _contAttack = false;
    private bool _contSpellAttack = false;
    private bool _facingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        /* If player in spell attack range start spell attack. */
        if(!_contSpellAttack && !_contAttack &&
            Vector3.Distance(_playerTransform.position, transform.position) <= _spellRange)
        {
            _contSpellAttack = true;
        }

        /* If player in normal attack range start normal attack. */
        if(!_contAttack && Vector3.Distance(_playerTransform.position, transform.position) <= _spellRange)

        /* If attacking and player changes direction, change direction. */
        if(_contSpellAttack || _contAttack)
        {
            if(!_facingRight && 
                _playerTransform.position.x > transform.position.x + FaceTurningCap)
            {
                _spriteRenderer.flipX = false;
                _facingRight = true;
            }
            else if(_facingRight &&
                _playerTransform.position.x + FaceTurningCap < transform.position.x )
            {
                _spriteRenderer.flipX = true;
                _facingRight = false;
            }
        }

        if(_contSpellAttack && Time.time >= _nextSpellTime)
        {
            /* TODO, spell attack. */
            _nextSpellTime = Time.time + 1f / _spellRate;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            _contSpellAttack = false;
            _contSpellAttack = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if(Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + 1f / _attackRate;
                /* TODO normal attack. */
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            _contSpellAttack = _contSpellAttack = false;
        }
    }

    public void GetHit(float damage)
    {
        _hitPoints -= Mathf.RoundToInt(damage);
        if (_hitPoints < float.Epsilon)
        {
            _animator.SetBool("Death", true);
            _boxCollider2D.enabled = false;
            this.enabled = false;
        }
        _animator.SetTrigger("Hurt");
    }
}
