using UnityEngine;

public class EnemyCharacter : Character
{
    public static UnityEngine.Tilemaps.Tilemap _tileMap = null;
    public static Transform _playerTransform = null;

    [SerializeField] protected float _chaseRange = 1.0f;

    protected System.Collections.Generic.List<Indexes> _pathToLatestTarget = null;
    protected PFState _PFState = PFState.Wait;
    protected Vector3 _targetPos = new Vector3();
    protected Vector3 _startPos = new Vector3();
    protected Vector3 _moveDirection = new Vector3();
    protected Indexes _startTileCell = new Indexes();
    protected Indexes _targetTileCell = new Indexes();
    protected float _targetDistance = 0.0f;
    protected bool _continuousAttack = false;

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
        ChasePlayer();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void MoveCharacter()
    {
        if (PFState.Wait == _PFState)
        {
            if(_running)
            {
                _animator.SetTrigger("Idle");
                _running = false;
            }           
            return;
        }

        /* Move, flip and run animation. */
        transform.Translate(_moveDirection * _moveSpeed * Time.deltaTime);
        bool prev = _spriteRenderer.flipX;
        _spriteRenderer.flipX = _moveDirection.x < 0.0f;

        if(prev != _spriteRenderer.flipX)
        {
            _attackLocation.localPosition = new Vector3(-1 * _attackLocation.localPosition.x,
                _attackLocation.localPosition.y, _attackLocation.localPosition.z);
        }

        if(!_running)
        {
            _continuousAttack = _attacked = false;
            _animator.SetTrigger("Running");
            _running = true;
        }

        Vector3Int currentTile = _tileMap.WorldToCell(transform.position);
        if (currentTile.y == _targetTileCell.i && currentTile.x == _targetTileCell.j)
        {
            /* Reached target. */
            if (0 == _pathToLatestTarget.Count)
            {
                _continuousAttack = true;
                _PFState = PFState.Wait;
                return;
            }

            /* Keep chasing to the new pos, if player moved. */
            Indexes dungeonDif = LevelGenerator.GetDifIndex();
            Indexes lastTarget = _pathToLatestTarget[_pathToLatestTarget.Count-1];
            lastTarget.i -= dungeonDif.i; lastTarget.j -= dungeonDif.j;
            Vector3Int playerTile = _tileMap.WorldToCell(_playerTransform.position);
            if(playerTile.y != lastTarget.i || playerTile.x != lastTarget.j)
            {
                ChasePlayer(true);
                return;
            }

            _startPos = transform.position;
            _startTileCell.i = currentTile.y; _startTileCell.j = currentTile.x;
            GoToTarget();
        }
    }

    protected override void AttackCharacter()
    {
        if (!_attacked && _continuousAttack && Time.time >= _nextAttackTime)
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

    protected void GoToTarget()
    {
        _targetTileCell = _pathToLatestTarget[0];
        Indexes dungeonDif = LevelGenerator.GetDifIndex();
        /* DM coord to tilemap coord. */
        _targetTileCell.i -= dungeonDif.i; _targetTileCell.j -= dungeonDif.j;
        /* Get targetpos from target tilemap coord. */
        _targetPos = _tileMap.GetCellCenterWorld(new Vector3Int(_targetTileCell.j, _targetTileCell.i, 0));
        /* Calc distance between positions to translate. */
        _targetDistance = Vector3.Distance(_startPos, _targetPos);

        /* Set direction vector. */
        _moveDirection.x = _targetPos.x - _startPos.x;
        _moveDirection.y = _targetPos.y - _startPos.y;

        _pathToLatestTarget.RemoveAt(0);
    }

    protected void ChasePlayer(bool chase = false)
    {
        /* Chase if in range and waiting. */
        if (!chase &&
            ((PFState.Wait == _PFState &&
            Vector3.Distance(transform.position, _playerTransform.position) > _chaseRange)
            || PFState.Wait != _PFState)
            )
        {
            return;
        }

        Indexes dungeonDif = LevelGenerator.GetDifIndex();

        /* Get your cell pos, from your world pos. */
        Vector3Int tmp = _tileMap.WorldToCell(transform.position);
        Indexes start = new Indexes(tmp.x, tmp.y);
        _startTileCell.j = tmp.x; _startTileCell.i = tmp.y;

        start.i += dungeonDif.i; start.j += dungeonDif.j;

        /* Get player cell pos, from player world pos. */
        tmp = _tileMap.WorldToCell(_playerTransform.position);
        Indexes target = new Indexes(tmp.x, tmp.y);

        target.i += dungeonDif.i; target.j += dungeonDif.j;

        /* If path available, set the needed variables. */
        if(PathFind(start, target))
        {
            _PFState = PFState.OnRoute;
            _startPos = transform.position;
            GoToTarget();
        }
    }

    public bool PathFind(Indexes start, Indexes target)
    {
        System.Collections.Generic.List<Indexes> openList = new System.Collections.Generic.List<Indexes>();
        System.Collections.Generic.List<Indexes> closedList = new System.Collections.Generic.List<Indexes>();
        /* Keys are children, values are parents. */
        System.Collections.Generic.Dictionary<Indexes, Indexes> parents = new System.Collections.Generic.Dictionary<Indexes, Indexes>();
        Indexes currentPoint = new Indexes(start.j, start.i);
        int i = 0, lowestScoreIndex = 0, minF = 0,
        maxI = LevelGenerator._dungeonSize.i - 1,
        maxJ = LevelGenerator._dungeonSize.j - 1;
        bool dontPut = false, pathFound = false;

        Cell[,] dungeonMatrix = LevelGenerator._dungeonMatrix;

        if(dungeonMatrix == null)
        {
            return false;
        }

        /* Add the starting point with parents -1,-1 */
        parents.Add(start, new Indexes(-1, -1));

        closedList.Add(currentPoint);

        while (true)
        {
            /* Check all adjacents, add to the openlist if not added, ignore if on the closedlist or not walkable. */
            for (int k = -1; k < 2; ++k)
            {
                for (int l = -1; l < 2; ++l)
                {
                    /* Skip yourself or out of bounds. */
                    if ((l == 0 && k == 0)
                        || currentPoint.j + l < 0 || currentPoint.j + l > maxJ
                        || currentPoint.i + k < 0 || currentPoint.i + k > maxI)
                    {
                        continue;
                    }

                    /* Skip a cutting corner. */
                    if ((l != 0 && k != 0) &&
                        (!dungeonMatrix[currentPoint.i, currentPoint.j + l]._walkable ||
                        !dungeonMatrix[currentPoint.i + k, currentPoint.j]._walkable))
                    {
                        continue;
                    }

                    /* Check if in closedList. */
                    dontPut = false;
                    foreach (Indexes index in closedList)
                    {
                        if (index.j == currentPoint.j + l && index.i == currentPoint.i + k)
                        {
                            dontPut = true;
                            break;
                        }
                    }

                    /* Put in openlist if walkable and not in closedList. */
                    if (!(dontPut || !dungeonMatrix[currentPoint.i + k, currentPoint.j + l]._walkable))
                    {
                        /* If adjacent is on the open list and new cost is lower,
                         * change the cost and the parent values for the adjacent. */
                        int currentCost = (k == 0 || l == 0) ? Cell._normalCost : Cell._diagonalCost;
                        foreach (Indexes index in openList)
                        {
                            if (index.j == currentPoint.j + l && index.i == currentPoint.i + k)
                            {
                                dontPut = true;
                                if (dungeonMatrix[index.i, index.j]._gCost >
                                    dungeonMatrix[currentPoint.i, currentPoint.j]._gCost + currentCost)
                                {
                                    dungeonMatrix[index.i, index.j]._gCost = dungeonMatrix[currentPoint.i, currentPoint.j]._gCost + currentCost;
                                    var foundIndex = parents[index];
                                    foundIndex.j = currentPoint.j; foundIndex.i = currentPoint.i;
                                }
                                break;
                            }
                        }
                        if (!dontPut)
                        {
                            openList.Add(new Indexes(currentPoint.j + l, currentPoint.i + k));
                            parents.Add(openList[openList.Count - 1], new Indexes(currentPoint.j, currentPoint.i));
                            dungeonMatrix[currentPoint.i + k, currentPoint.j + l]._gCost = dungeonMatrix[currentPoint.i, currentPoint.j]._gCost + currentCost;
                        }
                    }
                }
            }

            /* Select the node from openlist with lowest F cost as the current node.
            * Remove it from the openList and add to the closed list.*/
            lowestScoreIndex = -1;
            if(openList.Count != 0)
            {
                minF = dungeonMatrix[openList[0].i, openList[0].j].CalculateF(openList[0].j, openList[0].i, target);
                lowestScoreIndex = 0;
            }
            for (i = 1; i < openList.Count; ++i)
            {
                if (minF > dungeonMatrix[openList[i].i, openList[i].j].
                    CalculateF(openList[i].j, openList[i].i, target))
                {
                    lowestScoreIndex = i;
                    minF = dungeonMatrix[openList[i].i, openList[i].j].
                        CalculateF(openList[i].j, openList[i].i, target);
                }
            }

            if(lowestScoreIndex != -1)
            {
                closedList.Add(openList[lowestScoreIndex]);
                currentPoint = openList[lowestScoreIndex];
                openList.RemoveAt(lowestScoreIndex);
            }

            /* Drop from the open list, add to the closed list. */
            if (target.i == currentPoint.i && target.j == currentPoint.j)
            {
                pathFound = true;
                break;
            }
            else if (0 == openList.Count)
            {
                break;
            }
        }

        /* Create the path. */
        if (pathFound)
        {
            _pathToLatestTarget = new System.Collections.Generic.List<Indexes>();
            while(currentPoint.i != -1)
            {
                _pathToLatestTarget.Insert(0, currentPoint);
                currentPoint = parents[currentPoint];
            }

            /* Remove first point, bceause it's the start pos. */
            _pathToLatestTarget.RemoveAt(0);
        }

        /* Clear gCosts. */
        foreach (Indexes index in openList)
        {
            dungeonMatrix[index.i, index.j]._gCost = 0;
        }

        foreach(Indexes index in closedList)
        {
            dungeonMatrix[index.i, index.j]._gCost = 0;
        }

        return pathFound;
    }
}