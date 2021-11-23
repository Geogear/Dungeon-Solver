using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] protected float _moveSpeed = 7.0f;
    [SerializeField] protected float _attackOffsetProportion = 0.25f;

    protected Animator _animator = null;
    protected SpriteRenderer _spriteRenderer = null;

    protected string _hitTargetTag = "";
    protected bool _running = false;
    protected bool _facingRight = true;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        MoveCharacter();
    }

    protected abstract void MoveCharacter();

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        /* Setting a range on y, so it doesn't hit too much above and below. */
        if (collision.tag == _hitTargetTag
            && collision.transform.position.y <=
            transform.position.y + _spriteRenderer.size.y / 2 - _spriteRenderer.size.y / _attackOffsetProportion
            && collision.transform.position.y >=
            transform.position.y - _spriteRenderer.size.y / 2 + _spriteRenderer.size.y / _attackOffsetProportion)
        {
            /* TODO, play anim. 
               TODO, set enemy.gethit */
        }
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
}
