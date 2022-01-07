using UnityEngine;

public abstract class Character : MonoBehaviour
{
    protected static readonly string TrapTag = "Trap";

    [SerializeField] protected string _characterName = "";
    [SerializeField] protected float _moveSpeed = 7.0f;
    [SerializeField] protected float _attackDamage = 3.0f;
    [SerializeField] protected float _attackRange = 0.35f;
    [SerializeField] protected float _attackRate = 1.0f;
    [SerializeField] protected int _maxHitPoints = 15;
    [SerializeField] protected int _hitPoints = 15;
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] protected Transform _attackLocation;

    protected Animator _animator = null;
    protected AnimationClip _attackAnim = null;
    protected AnimationClip _hurtAnim = null;
    protected SpriteRenderer _spriteRenderer = null;
    protected BoxCollider2D _boxCollider2D = null;
    protected Spikes _currentSpikeTrap = null; 
   
    protected string _attackAnimName = "";
    protected string _hurtAnimName = "";
    protected string _dyingAnimName = "";
    protected float _animCounter = 0.0f;
    protected float _leftHurtCD = -1.0f;
    protected float _raycastDistanceProportion = 0.1f;
    protected float _raycastPositionProportion = 0.55f;
    protected float _nextAttackTime = 0.0f;
    protected bool _running = false;
    protected bool _facingRight = true;
    protected bool _attacked = false;
    protected bool _invincible = false;
    protected bool _died = false;
    protected bool _tookTrapDamage = false;

    protected static string[] _unMovablesTags = 
        {"Wall", "Treasure", "HealingStatue"};

    protected virtual void Awake()
    {
        SetYourProperties();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _attackAnim = GetAnimFromAnimator(_attackAnimName);
        _hurtAnim = GetAnimFromAnimator(_hurtAnimName);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_died || GameController.IsPaused())
        {
            return;
        }

        if (_leftHurtCD > float.Epsilon)
        {
            _leftHurtCD -= Time.deltaTime;
            _running = false;
            return;
        }

        TrapInteraction();
        AttackAnimCountDown();
        MoveCharacter();
        AttackCharacter();
    }

    protected virtual void FixedUpdate()
    {
    }

    public void GetHit(float damage)
    {
        if (!_invincible)
        {
            _hitPoints -= Mathf.RoundToInt(damage);
            if (_hitPoints < float.Epsilon)
            {
                _animator.SetBool("Death", true);
                _died = _invincible = true;
                _boxCollider2D.enabled = false;
                this.enabled = false;
            }
            _animator.SetTrigger("Hurt");
            _leftHurtCD = _hurtAnim.length;
        }      
    }

    protected abstract void MoveCharacter();

    protected abstract void AttackCharacter();

    protected virtual void SetYourProperties()
    {
        _attackAnimName = _characterName + "Attack";
        _hurtAnimName = _characterName + "Hurt";
        _dyingAnimName = _characterName + "Dying";
    }

    protected AnimationClip GetAnimFromAnimator(string name)
    {
        for (int i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            if (_animator.runtimeAnimatorController.animationClips[i].name == name)
            {
                return _animator.runtimeAnimatorController.animationClips[i];
            }
        }
        return null;
    }

    protected void AttackAnimCountDown()
    {
        if (Time.time < _nextAttackTime)
        {
            return;
        }

        if (_attacked)
        {
            _animCounter -= Time.deltaTime;
            _attacked = _animCounter > float.Epsilon;
            if (!_attacked)
            {
                MeleeAttack();
                _nextAttackTime = Time.time + 1f / _attackRate;
            }
        }
    }

    protected void MeleeAttack()
    {
        Collider2D[] damage = Physics2D.OverlapCircleAll(_attackLocation.position, _attackRange, _targetLayer);
        for (int i = 0; i < damage.Length; ++i)
        {
            damage[i].GetComponent<Character>().GetHit(_attackDamage);
        }
    }

    protected bool CheckUnmovablesForDirection(Vector2 direction)
    {
        Vector3 newPos = new Vector3();
        newPos.x = transform.position.x + direction.x * _boxCollider2D.size.x * _raycastPositionProportion;
        newPos.y = transform.position.y + direction.y * _boxCollider2D.size.y * _raycastPositionProportion;
        newPos.z = transform.position.z;

        RaycastHit2D hitRay = Physics2D.Raycast(newPos, direction, _raycastDistanceProportion);
        return (hitRay.collider != null && System.Array.IndexOf(_unMovablesTags, hitRay.collider.tag) > -1);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == TrapTag)
        {
            _currentSpikeTrap = collision.GetComponent<Spikes>();
            return;
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == TrapTag)
        {
            _currentSpikeTrap = null;
            _tookTrapDamage = false;
            return;
        }
    }

    protected virtual void TakeDamage(int damage)
    {
        _hitPoints -= damage;
    }

    protected virtual void TakeHeal(int heal)
    {
        _hitPoints += heal;
        _hitPoints = (_hitPoints > _maxHitPoints) ? _maxHitPoints : _hitPoints;
    }

    protected virtual void TrapInteraction()
    {
        if (_currentSpikeTrap != null)
        {
            int damage = _currentSpikeTrap.SendDamage();
            if(damage == 0 && _tookTrapDamage)
            {
                _tookTrapDamage = false;
            }
            else if(damage > 0 && !_tookTrapDamage)
            {
                TakeDamage(damage);
                _tookTrapDamage = true;
            }        
        }
    }

    private void OnDrawGizmosSelected()
    {
        /* Draw attack range gizmo. */
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackLocation.position, _attackRange);
    }
}