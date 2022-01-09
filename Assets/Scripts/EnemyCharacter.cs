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
    protected float _targetDistance = 0.0f;
    protected float _lerpTime = 1f;
    protected float _currentLerpTime = 0.0f;

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
        LerpToCurrentTarget();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void MoveCharacter()
    {
        /* TODO */
    }

    protected override void AttackCharacter()
    {
        /* TODO */
    }

    protected override void SetYourProperties()
    {
        base.SetYourProperties();
    }

    protected void GoToTarget()
    {
        Indexes targetIndexes = _pathToLatestTarget[0];
        Indexes dungeonDif = LevelGenerator.GetDifIndex();
        /* DM coord to tilemap coord. */
        targetIndexes.i -= dungeonDif.i; targetIndexes.j -= dungeonDif.j;
        _targetPos = _tileMap.GetCellCenterWorld(new Vector3Int(targetIndexes.j, targetIndexes.i, 0));
        _targetDistance = Vector3.Distance(_startPos, _targetPos);

        _pathToLatestTarget.RemoveAt(0);
    }

    protected void LerpToCurrentTarget()
    {
        if(PFState.Wait == _PFState)
        {
            return;
        }

        _currentLerpTime += Time.deltaTime;
        if(_currentLerpTime > _lerpTime)
        {
            _currentLerpTime -= _lerpTime;
            if(0 == _pathToLatestTarget.Count)
            {
                _PFState = PFState.Wait;
                return;
            }
            _startPos = _targetPos;
            GoToTarget();
        }

        float perc = _currentLerpTime / _lerpTime;
        transform.position = Vector3.Lerp(_startPos, _targetPos, perc);

        if(transform.position == _targetPos)
        {      
            if (0 == _pathToLatestTarget.Count)
            {
                _PFState = PFState.Wait;
                return;
            }
            _startPos = transform.position;
            _currentLerpTime = 0.0f;
            GoToTarget();   
        }
    }

    protected void ChasePlayer()
    {
        /* Chase if in range and waiting. */
        if ((PFState.Wait == _PFState &&
            Vector3.Distance(transform.position, _playerTransform.position) <= _chaseRange)
            || PFState.Wait != _PFState)
        {
            return;
        }

        Indexes dungeonDif = LevelGenerator.GetDifIndex();

        /* Get your cell pos, from your world pos. */
        Vector3Int tmp = _tileMap.WorldToCell(transform.position);
        Indexes start = new Indexes(tmp.x, tmp.y);

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
            _currentLerpTime = 0.0f;
            GoToTarget();
        }
    }

    public static void PathFind()
    {
        /* False values are walkable, trues are non-walkable. */
        bool[,] obstacleMap = new bool[6, 8];
        int maxI = 5, maxJ = 7;
        obstacleMap[1, 4] = obstacleMap[2, 4] =
        obstacleMap[3, 4] = true;
        bool dontPut = false, pathFound = false;
        int i = 0, lowestScoreIndex = 0, minF = 0;
        Indexes startingPoint = new Indexes(2, 2);
        Indexes targetPoint = new Indexes(6, 2);
        Indexes currentPoint = new Indexes(0, 0);
        Node currentNode;

        System.Collections.Generic.List<Node> openList = new System.Collections.Generic.List<Node>();
        System.Collections.Generic.List<Node> closedList = new System.Collections.Generic.List<Node>();

        openList.Add(new Node(startingPoint.j, startingPoint.i));
        closedList.Add(openList[lowestScoreIndex]);
        currentNode = openList[lowestScoreIndex];
        currentPoint.i = currentNode._coord.i;
        currentPoint.j = currentNode._coord.j;
        openList.RemoveAt(lowestScoreIndex);

        while(true)
        {
            /* Check all adjacents, add to the openlist if not added, ignore if on the closedlist or unwalkable. */
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
                    if((l != 0 && k != 0) &&
                        (obstacleMap[currentPoint.i, currentPoint.j + l] ||
                        obstacleMap[currentPoint.i + k, currentPoint.j]))
                    {
                        continue;
                    }

                    /* Check if in closedList. */
                    dontPut = false;
                    foreach (Node node in closedList)
                    {
                        if (node.SameByCoord(currentPoint.j + l, currentPoint.i + k))
                        {
                            dontPut = true;
                            break;
                        }
                    }

                    /* Put in openlist if not unwalkable and not in closedList. */
                    if (!(dontPut || obstacleMap[currentPoint.i + k, currentPoint.j + l]))
                    {
                        /* If adjacent is on the open list and new cost is lower,
                         * change the cost and the parent values for the adjacent. */
                        int currentCost = (k == 0 || l == 0) ? Node.normalCost : Node.diagonalCost;
                        foreach (Node node in openList)
                        {
                            if(node.SameByCoord(currentPoint.j + l, currentPoint.i + k))
                            {
                                dontPut = true;
                                if(node._gCost > currentNode._gCost + currentCost)
                                {
                                    var tmpNode = node;
                                    tmpNode._parent.i = currentPoint.i;
                                    tmpNode._parent.j = currentPoint.j;
                                    tmpNode._gCost = currentNode._gCost + currentCost;
                                }
                                break;
                            }
                        }
                        if (!dontPut)
                        {
                            openList.Add(new Node(currentPoint.j + l, currentPoint.i + k, currentPoint.j, currentPoint.i, currentNode._gCost + currentCost));
                        }
                    }
                }
            }

            /* Select the node from openlist with lowest F cost as the current node.
             * Remove it from the openList and add to the closed list.*/
            minF = openList[0].CalculateF(targetPoint);
            lowestScoreIndex = 0;
            for(i = 1; i < openList.Count; ++i)
            {
                if(minF > openList[i].CalculateF(targetPoint))
                {
                    lowestScoreIndex = i;
                    minF = openList[i].CalculateF(targetPoint);
                }
            }
            closedList.Add(openList[lowestScoreIndex]);
            currentNode = openList[lowestScoreIndex];
            currentPoint.i = currentNode._coord.i;
            currentPoint.j = currentNode._coord.j;
            openList.RemoveAt(lowestScoreIndex);

            /* Drop from the open list, add to the closed list. */
            if (currentNode.SameByCoord(targetPoint.j, targetPoint.i))
            {
                pathFound = true;
                break;
            }
            else if(0 == openList.Count)
            {
                break;
            }
        }

        /* Create the path. */
        if (pathFound)
        {
            int safety = 0;
            Node printNode = currentNode;
            for(; safety < 100; ++safety)
            {
                Debug.Log("x: " + printNode._coord.j + " y: " + printNode._coord.i);
                if(printNode._parent.i == -1)
                {
                    break;
                }
                /* Get the parent node. */
                foreach(Node node in closedList)
                {
                    if(node._coord.i == printNode._parent.i 
                        && node._coord.j == printNode._parent.j)
                    {
                        printNode = node;
                        break;
                    }
                }
            }
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