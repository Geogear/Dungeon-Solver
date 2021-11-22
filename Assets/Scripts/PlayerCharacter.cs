using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7.0f;
    private float _horizontalInput = 0.0f;
    private float _verticalInput = 0.0f;

    private bool _running = false;
    private bool _facingRight = true;

    private Animator _animator = null;
    private SpriteRenderer _spriteRenderer = null;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCharacter();
    }

    private void MoveCharacter()
    {
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
            }
        }
        transform.Translate(new Vector3(_horizontalInput, _verticalInput, 0) * _moveSpeed * Time.deltaTime);
    }
}
