using UnityEngine;

public abstract class Character : MonoBehaviour
{
    /* TODO, hurt anim cd, and invincible while hurt. */
    [SerializeField] protected string _characterName = "";
    [SerializeField] protected float _moveSpeed = 7.0f;
    [SerializeField] protected float _attackDamage = 3.0f;
    [SerializeField] protected float _attackRange = 0.35f;
    [SerializeField] protected float _hitPoints = 15.0f;
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] protected Transform _attackLocation;

    protected Animator _animator = null;
    protected AnimationClip _attackAnim = null;
    protected AnimationClip _hurtAnim = null;
    protected SpriteRenderer _spriteRenderer = null;
   
    protected string _attackAnimName = "";
    protected string _hurtAnimName = "";
    protected float _attackTime = 0.0f;
    protected float _hurtTime = 0.0f;
    protected bool _running = false;
    protected bool _facingRight = true;
    protected bool _attacked = false;
    protected bool _invincible = false;
    protected bool _died = false;

    protected virtual void Awake()
    {
        SetYourProperties();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _attackAnim = GetAnimFromAnimator(_attackAnimName);
        _hurtAnim = GetAnimFromAnimator(_hurtAnimName);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        MoveCharacter();
        AttackCharacter();
        AttackAnimCountDown();
    }

    public void GetHit(float damage)
    {
        if (!_invincible)
        {
            _hitPoints -= damage;
            if (_hitPoints < float.Epsilon)
            {
                _animator.SetTrigger("Death");
                _died = _invincible = true;
                return;
            }
            _animator.SetTrigger("Hurt");
            _hurtTime = _hurtAnim.length;
        }      
    }

    protected abstract void MoveCharacter();

    protected abstract void AttackCharacter();

    protected virtual void SetYourProperties()
    {
        _attackAnimName = _characterName + "Attack";
        _hurtAnimName = _characterName + "Hurt";
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
        if (_attacked)
        {
            _attackTime -= Time.deltaTime;
            _attacked = _attackTime > float.Epsilon;
            if (!_attacked)
            {
                _animator.SetTrigger("Idle");
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

    private void OnDrawGizmosSelected()
    {
        /* Draw attack range gizmo. */
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackLocation.position, _attackRange);
    }
}