using UnityEngine;

public class PlayerCharacter : Character
{
    /* TODO, when animation is supposed to be speed up, set a bool on character.
       StateMachineBehavior looks at that bool at each state exit, if true, increases speed.
       Will the animation clip length will increase? */
    private float _horizontalInput = 0.0f;
    private float _verticalInput = 0.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void MoveCharacter()
    {
        if (_attacked)
        {
            return;
        }
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");        
        bool haveInput = _horizontalInput > float.Epsilon || _horizontalInput < -float.Epsilon
            || _verticalInput > float.Epsilon || _verticalInput < -float.Epsilon;

        if (!haveInput && _running)
        {
            _animator.SetTrigger("Idle");
            _running = false;
        }
        else if (haveInput && !_running)
        {
            _animator.SetTrigger("Running");
            _running = true;
        }

        if (_horizontalInput > float.Epsilon || _horizontalInput < -float.Epsilon)
        {
            if (_facingRight != (_horizontalInput > float.Epsilon))
            {
                _spriteRenderer.flipX = _facingRight;
                _facingRight = !_facingRight;
                _attackLocation.localPosition = new Vector3(-1 * _attackLocation.localPosition.x,
                    _attackLocation.localPosition.y, _attackLocation.localPosition.z);
            }
        }
        transform.Translate(new Vector3(_horizontalInput, _verticalInput, 0) * _moveSpeed * Time.deltaTime);
    }

    protected override void AttackCharacter()
    {
        if (Input.GetAxis("Fire2") > float.Epsilon && !_attacked)
        {
            _attacked = true;
            _animator.SetTrigger("Attack");
            _attackTime = _attackAnim.length;
            MeleeAttack();
        }
    }

    protected override void SetYourProperties()
    {
        _hitTargetTag = "Enemy";
        _attackAnimName = "FA2Idle";
    }
}