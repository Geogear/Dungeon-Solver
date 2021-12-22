using UnityEngine;

public class PlayerCharacter : Character
{
    /* TODO, Attack speed buff, when animation is supposed to be speed up, set a bool on character.
       StateMachineBehavior looks at that bool at each state exit, if true, increases speed.
       Will the animation clip length will increase? OR can do actual attack at the end of attack anim!!*/

    [SerializeField] private PuzzleDisplayer _puzzleDisplayer;

    private float _horizontalInput = 0.0f;
    private float _verticalInput = 0.0f;
    private TreasureState _treasureState = TreasureState.TreasureStateCount;

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
        TreasureInteraction();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
            return;
        }
        else if (haveInput && !_running)
        {
            _animator.SetTrigger("Running");
            _running = true;
        }

        if (_running)
        {
            /* Check for unmovables on the walked direction. */
            if (_verticalInput > float.Epsilon && CheckUnmovablesForDirection(Vector2.up)
                || _verticalInput < -float.Epsilon && CheckUnmovablesForDirection(Vector2.down))
            {
                _verticalInput = 0.0f;
            }
            if (_horizontalInput > float.Epsilon && CheckUnmovablesForDirection(Vector2.right)
                || _horizontalInput < -float.Epsilon && CheckUnmovablesForDirection(Vector2.left))
            {
                _horizontalInput = 0.0f;
            }

            /* Flip the sprite and attack location if needed. */
            if ((_horizontalInput > float.Epsilon || _horizontalInput < -float.Epsilon) &&
                _facingRight != (_horizontalInput > float.Epsilon))
            {
                _spriteRenderer.flipX = _facingRight;
                _facingRight = !_facingRight;
                _attackLocation.localPosition = new Vector3(-1 * _attackLocation.localPosition.x,
                    _attackLocation.localPosition.y, _attackLocation.localPosition.z);
            }

            transform.Translate(new Vector3(_horizontalInput, _verticalInput, 0) * _moveSpeed * Time.deltaTime);
        }        
    }

    protected override void AttackCharacter()
    {
        if (!_attacked && Input.GetAxis("Fire2") > float.Epsilon && _leftAttackCD < float.Epsilon)
        {
            _attacked = true;
            _running = false;
            _animator.SetTrigger("Attack");
            _animCounter = _attackAnim.length;
        }
    }

    protected override void SetYourProperties()
    {
        base.SetYourProperties();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == _unMovablesTags[1] && _treasureState == TreasureState.TreasureStateCount
            && !Treasure.IsOpened(collision.transform.position))
        {
            _treasureState = TreasureState.EnterTreasure;
            _puzzleDisplayer._currentTreasurePos = collision.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == _unMovablesTags[1] && collision.transform.position == _puzzleDisplayer._currentTreasurePos &&
            (_treasureState == TreasureState.OnTreasure || _treasureState == TreasureState.EnterTreasure))
        {
            _treasureState = TreasureState.TreasureStateCount;
            /* Puzzle might be closed because of success or fail. So need to check if open. */
            if (_puzzleDisplayer.IsOpen())
            {
                Treasure.RewardOrPunish(this, collision.transform.position, false);
                _puzzleDisplayer.ClosePuzzle(false);
            }           
        }
    }

    private void TreasureInteraction()
    {
        /* Display if collided. */
        if(Input.GetAxis("Fire1") > float.Epsilon && _treasureState == TreasureState.EnterTreasure)
        {
            _treasureState = TreasureState.OnTreasure;
            _puzzleDisplayer.OpenPuzzle();
        }/* If diplayed and collided, let it interact. */
        else if (Input.GetAxis("Fire3") > float.Epsilon && _treasureState == TreasureState.OnTreasure)
        {
            bool success = false,
            matched = _puzzleDisplayer.MatchedDisplaySelection(Camera.main.ScreenToWorldPoint(Input.mousePosition), ref success);
            if (matched)
            {
                Treasure.RewardOrPunish(this, _puzzleDisplayer._currentTreasurePos, success);
                _treasureState = TreasureState.TreasureStateCount;
                _puzzleDisplayer.ClosePuzzle(success);
            }
        }
    }
}