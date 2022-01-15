using UnityEngine;

public class PlayerCharacter : Character
{
    public static Vector3 _startingPos;
    public LevelGenerator _levelgenerator;

    [SerializeField] private PuzzleDisplayer _puzzleDisplayer;
    [SerializeField] private UnityEngine.UI.Image _LSImage;
    [SerializeField] private ParticleSystem _treasurePS;
    [SerializeField] private UnityEngine.UI.Text _hpText;
    [SerializeField] private UnityEngine.UI.Text _attackDamageText;
    [SerializeField] private UnityEngine.UI.Text _levelNumberText;
    [SerializeField] private LayerMask _bossLayer;
    private Collider2D _currentTreasureCollision;

    private bool _onLevelExit = false;
    private bool _onHF = false;
    private bool _healingStatueUsed = false;
    private float _horizontalInput = 0.0f;
    private float _verticalInput = 0.0f;
    private TreasureState _treasureState = TreasureState.TreasureStateCount;
    private FlickerData _flickerData;

    public void SetToStartPos()
    {
        transform.position = new Vector3(_startingPos.x, _startingPos.y, _startingPos.z);
    }

    protected override void Awake()
    {
        base.Awake();
        EnemyCharacter._playerTransform = transform;
        Boss._playerTransform = transform;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _startingPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GameController.SetPMObject();
        SetIconTexts(IconType.HP); SetIconTexts(IconType.AttackDamage); SetIconTexts(IconType.LevelNumber);
        _flickerData = new FlickerData(_spriteRenderer);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        TreasureInteraction();
        LevelExitInteraction();
        HealingStatueInteraction();
        _invincible = _flickerData.Flicker();
        GameController.CheckForPause();
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
        if (!_attacked && Input.GetButtonDown("Fire2") && Time.time >= _nextAttackTime)
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

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.tag == _unMovablesTags[1] && _treasureState == TreasureState.TreasureStateCount
            && !Treasure.IsOpened(collision.transform.position))
        {
            _treasureState = TreasureState.EnterTreasure;
            _puzzleDisplayer._currentTreasurePos = collision.transform.position;
            _currentTreasureCollision = collision;
            return;
        }else if(collision.tag == "LevelExit")
        {
            _onLevelExit = true;
        }else if(collision.tag == "HealingStatue")
        {
            _onHF = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
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
        }else if (collision.tag == "LevelExit")
        {
            _onLevelExit = false;
        }else if (collision.tag == "HealingStatue")
        {
            _onHF = false;
        }
    }

    protected override void TakeDamage(int damage)
    {
        if(!_invincible)
        {
            _hitPoints -= damage;
            SetIconTexts(IconType.HP);
            _flickerData.TriggerFlick();
        }
    }

    protected override void TakeHeal(int heal)
    {
        base.TakeHeal(heal);
        SetIconTexts(IconType.HP);
    }

    protected override void MeleeAttack()
    {
        Collider2D[] damage = Physics2D.OverlapCircleAll(_attackLocation.position, _attackRange, _targetLayer);
        for (int i = 0; i < damage.Length; ++i)
        {
            damage[i].GetComponent<Character>().GetHit(_attackDamage);
        }
        damage = Physics2D.OverlapCircleAll(_attackLocation.position, _attackRange, _bossLayer);
        for (int i = 0; i < damage.Length; ++i)
        {
            damage[i].GetComponent<Boss>().GetHit(_attackDamage);
        }
    }

    private void TreasureInteraction()
    {
        /* Display if collided. */
        if(Input.GetButtonDown("Fire1") && _treasureState == TreasureState.EnterTreasure)
        {
            _treasureState = TreasureState.OnTreasure;
            _puzzleDisplayer.OpenPuzzle();
        }/* If displayed and collided, let it interact. */
        else if (Input.GetButtonDown("Fire3") && _treasureState == TreasureState.OnTreasure)
        {
            bool success = false,
            matched = _puzzleDisplayer.MatchedDisplaySelection(Camera.main.ScreenToWorldPoint(Input.mousePosition), ref success);
            if (matched)
            {
                Treasure.RewardOrPunish(this, _puzzleDisplayer._currentTreasurePos, success);
                _treasureState = TreasureState.TreasureStateCount;
                _puzzleDisplayer.ClosePuzzle(success);
                if (success)
                {
                    /* Open chest. */
                    _currentTreasureCollision.GetComponent<SpriteRenderer>().sprite = _puzzleDisplayer._openTreasureSprite;
                    /* Pop the effects. */
                    var emission = _treasurePS.emission;
                    emission.enabled = true;
                    _treasurePS.transform.position = new Vector3(_puzzleDisplayer._currentTreasurePos.x,
                        _puzzleDisplayer._currentTreasurePos.y - 0.2f, _treasurePS.transform.position.z);
                    _treasurePS.Play();

                }
                _currentTreasureCollision = null;
            }
        }
    }

    private void LevelExitInteraction()
    {
        if(Input.GetButtonDown("Fire1") && _onLevelExit)
        {
            StartCoroutine(CleanerCoroutine());
        }
    }

    private void SetIconTexts(IconType it)
    {
        switch(it)
        {
            case IconType.HP:
                _hpText.text = _hitPoints.ToString();
                break;
            case IconType.AttackDamage:
                _attackDamageText.text = ((int)_attackDamage).ToString();
                break;
            case IconType.LevelNumber:
                _levelNumberText.text = "Level: " + LevelGenerator.GetCurrentLvl();
                break;
        }
    }

    private void HealingStatueInteraction()
    {
        if (_onHF && Input.GetButtonDown("Fire1") && !_healingStatueUsed)
        {
            TakeHeal(_maxHitPoints);
            _healingStatueUsed = true;
        }
    }

    System.Collections.IEnumerator CleanerCoroutine()
    {
        const float wait = 2.0f;

        /* Disable icons. */
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Icon");
        foreach (GameObject go in objects)
        {
            go.SetActive(false);
        }

        /* Enable loading screen, and generate the next level.*/

        _LSImage.enabled = true;
        _levelgenerator.GenerateNextLevel();

        /* Wait for some time to  create loading effect. Then disable loading screen. */
        GameController.PasueOrResume(false);
        yield return new WaitForSeconds(wait);
        GameController.PasueOrResume(false);
        _LSImage.enabled = false;

        SetIconTexts(IconType.LevelNumber);
        /* Enable icons. */
        foreach (GameObject go in objects)
        {
            go.SetActive(true);
        }

        _healingStatueUsed = false;
    }

    public override void GetHit(float damage)
    {
        TakeDamage(Mathf.RoundToInt(damage));
    }

    /* To Use GameController with game components. */
    public void GC_POR()
    {
        GameController.PasueOrResume(true, true);
    }

    public void GC_QFMM()
    {
        GameController.QuitForMainMenu();
    }

}