using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Transform _playerTransform = null;

    private const float FaceTurningCap = 0.5f;

    [SerializeField] private string _bossName = "";
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
    [SerializeField] private Animator _spellAnimator = null;

    private SpriteRenderer _spriteRenderer = null;
    private BoxCollider2D _boxCollider2D = null;
    private Animator _animator = null;
    private float _nextAttackTime = 0.0f;
    private float _nextSpellTime = 0.0f;
    private float _spellCastPlayTime = 0.0f;
    private float _spellLeftCD = 0.0f;
    private bool _contAttack = false;
    private bool _contSpellAttack = false;
    private bool _facingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _spellCastPlayTime = GetAnimFromAnimator(_spellAnimator, _bossName + "Spell");
    }

    // Update is called once per frame
    void Update()
    {
        /* If out of range, stop spell.*/
        if(_contSpellAttack &&
            Vector3.Distance(_playerTransform.position, transform.position) > _spellRange)
        {
            _contSpellAttack = false;
        }

        /* If player in spell attack range start spell attack. */
        if(!_contSpellAttack && !_contAttack &&
            Vector3.Distance(_playerTransform.position, transform.position) <= _spellRange)
        {
            _contSpellAttack = true;
            _spellLeftCD = _spellCastPlayTime;
            _nextSpellTime = -1f;
            _animator.SetTrigger("Spell");
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

        if(_contSpellAttack)
        {
            /* Casting the spell. */
            if(_spellLeftCD > float.Epsilon + _spellCastPlayTime * 0.1f)
            {
                _spellLeftCD -= Time.deltaTime;
            }
            else
            {
                /* Casting is done either waiting for the next cast,
                 * or trigger the spell object and start waiting. */
                if(_nextSpellTime == -1f)
                {
                    /* TODO trigger spell object. */
                    _nextSpellTime = Time.time + 1f / _spellRate;
                }
                else if(Time.time > _nextSpellTime)
                {
                    _nextSpellTime = -1f;
                    _spellLeftCD = _spellCastPlayTime;
                    _animator.SetTrigger("Spell");
                }
            }
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

    private float GetAnimFromAnimator(Animator animator, string name)
    {
        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            if (animator.runtimeAnimatorController.animationClips[i].name == name)
            {
                return animator.runtimeAnimatorController.animationClips[i].length;
            }
        }
        return 0.0f;
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