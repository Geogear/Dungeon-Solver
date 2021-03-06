using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Transform _playerTransform = null;

    private const float FaceTurningCap = 0.5f;
    private const float HBBaseScale = 2.0f;

    private static bool _bossIsDead = true;

    [SerializeField] private int _maxHealth = 15;
    [SerializeField] private string _bossName = "";
    [SerializeField] private string _displayName = "";
    [SerializeField] private float _attackDamage = 5.0f;
    [SerializeField] private float _attackRange = 0.9f;
    [SerializeField] private float _attackRate = 1.0f;
    [SerializeField] private float _spellDamage = 3.0f;
    [SerializeField] private float _spellRange = 4.0f;
    [SerializeField] private float _spellRate = 0.7f;
    [SerializeField] private float _spellCastShortener = 0.1f; /* For 1.0f, cast time becomes zero, and vice versa. */
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Animator _attackAnimator = null;
    [SerializeField] private SpriteRenderer _spellRenderer = null;
    [SerializeField] private SpriteRenderer _attackRenderer = null;
    [SerializeField] private Spell _spell = null;
    [SerializeField] private Transform _attackLocation = null;

    private SpriteRenderer _spriteRenderer = null;
    private BoxCollider2D _boxCollider2D = null;
    private Animator _animator = null;
    private GameObject _healthBar;
    private UnityEngine.UI.Image _healthBarBorder = null;
    private UnityEngine.UI.Text _healthBarBorderText = null;
    private float _nextAttackTime = -1f;
    private float _attackLeftCD = 0.0f;
    private float _attackAnimTime = 0.0f;
    private float _nextSpellTime = 0.0f;
    private float _spellCastPlayTime = 0.0f;
    private float _spellLeftCD = 0.0f;
    private float _hitRange = 0.2f;
    private int _hitPoints = 0;
    private bool _contAttack = false;
    private bool _contSpellAttack = false;
    private bool _facingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        _bossIsDead = false;
        _hitPoints = _maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _spellCastPlayTime = GetAnimLenFromAnimator(_animator, _bossName + "Spell");
        _attackAnimTime = GetAnimLenFromAnimator(_animator, _bossName + "Attack");
        _hitRange += _attackRange;
        /* HBB image and text. */
        _healthBarBorder = GameObject.FindGameObjectWithTag("HealthBorder").GetComponent<UnityEngine.UI.Image>();
        _healthBarBorderText = GameObject.FindGameObjectWithTag("HealthBorder").GetComponentInChildren<UnityEngine.UI.Text>();
        _healthBarBorder.enabled = true;
        _healthBarBorderText.enabled = true;
        _healthBarBorderText.text = _displayName;
        /* HB. */
        _healthBar = GameObject.FindGameObjectWithTag("HealthBar");
        _healthBar.transform.localScale = new Vector3(HBBaseScale,
            _healthBar.transform.localScale.y, _healthBar.transform.localScale.z);
        _healthBar.GetComponentInChildren<UnityEngine.UI.Image>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        /* Hit, out of range. */
        if(_contAttack &&
            Vector3.Distance(_playerTransform.position, transform.position) > _hitRange)
        {
            _contAttack = false;
        }

        /* Hit, in range. */
        if(!_contAttack &&
            Vector3.Distance(_playerTransform.position, transform.position) <= _hitRange)
        {
            _contAttack = true;
            _contSpellAttack = false;
            /* Trigger anim. */
            _attackLeftCD = _attackAnimTime;
            _nextAttackTime = -1f;
            _animator.SetTrigger("Attack");
            _attackAnimator.SetTrigger(_bossName);
        }


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
            _spell.SetPos(_playerTransform.position);
        }

        /* If player in normal attack range start normal attack. */
        if(!_contAttack && Vector3.Distance(_playerTransform.position, transform.position) <= _spellRange)

        /* If attacking and player changes direction, change direction. */
        if(_contSpellAttack || _contAttack)
        {
            if(!_facingRight && 
                _playerTransform.position.x > transform.position.x + FaceTurningCap)
            {
                _spriteRenderer.flipX = _spellRenderer.flipX = _attackRenderer.flipX = false;
                _facingRight = true;
                _attackLocation.localPosition = new Vector3(-1 * _attackLocation.localPosition.x,
                    _attackLocation.localPosition.y, _attackLocation.localPosition.z);
                }
            else if(_facingRight &&
                _playerTransform.position.x + FaceTurningCap < transform.position.x )
            {
                _spriteRenderer.flipX = _spellRenderer.flipX = _attackRenderer.flipX = true;
                _facingRight = false;
                _attackLocation.localPosition = new Vector3(-1 * _attackLocation.localPosition.x,
                    _attackLocation.localPosition.y, _attackLocation.localPosition.z);
                }
        }

        if(_contSpellAttack)
        {
            /* Casting the spell. */
            if(_spellLeftCD > float.Epsilon + _spellCastPlayTime * _spellCastShortener)
            {
                _spellLeftCD -= Time.deltaTime;
            }
            else
            {
                /* Casting is done, either waiting for the next cast,
                 * or trigger the spell object and start waiting. */
                if(_nextSpellTime == -1f)
                {
                    /* Trigger spell object. */
                    _nextSpellTime = Time.time + 1f / _spellRate;
                    _spell.ActivateSpell(_bossName, _spellDamage);
                }
                else if(Time.time > _nextSpellTime)
                {
                    /* Start casting. */
                    _nextSpellTime = -1f;
                    _spellLeftCD = _spellCastPlayTime;
                    _animator.SetTrigger("Spell");
                    _spell.SetPos(_playerTransform.position);
                }
            }
        }

        if(_contAttack)
        {
            /* Wait for anim to end. */
            if (_attackLeftCD > float.Epsilon)
            {
                _attackLeftCD -= Time.deltaTime;
            }
            else
            {
                if (_nextAttackTime == -1f)
                {
                    /* Deal damage at the end of anim. */
                    _spell._playerScript.GetHit(_attackDamage);
                    _nextAttackTime = Time.time + 1f / _attackRate;
                }
                else if (Time.time > _nextAttackTime)
                {
                    /* Animate next attack. */
                    _nextAttackTime = -1f;
                    _attackLeftCD = _attackAnimTime;
                    _animator.SetTrigger("Attack");
                    _attackAnimator.SetTrigger(_bossName);
                }
            }      
        }
    }

    private float GetAnimLenFromAnimator(Animator animator, string name)
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

    private void OnDrawGizmosSelected()
    {
        /* Draw attack range gizmo. */
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackLocation.position, _attackRange);
    }

    public void GetHit(float damage)
    {
        _hitPoints -= Mathf.RoundToInt(damage);
        float hp = _hitPoints, hpMax = _maxHealth;
        float hbScaleX = (_hitPoints <= 0) ? 0 : hp / hpMax * HBBaseScale;
        _healthBar.transform.localScale =new Vector3(hbScaleX,
            _healthBar.transform.localScale.y, _healthBar.transform.localScale.z);
        if (_hitPoints < float.Epsilon)
        {
            _animator.SetTrigger("Death");
            _bossIsDead = true;
            _boxCollider2D.enabled = false;
            _healthBarBorder.enabled = false;
            _healthBarBorderText.enabled = false;
            _healthBar.GetComponentInChildren<UnityEngine.UI.Image>().enabled = false;          
            this.enabled = false;
        }
    }

    public static bool BossIsDead() => _bossIsDead;
}