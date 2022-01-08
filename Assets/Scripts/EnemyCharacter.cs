using UnityEngine;

public class EnemyCharacter : Character
{
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
                            if (node.SameByCoord(currentPoint.j + l, currentPoint.i + k)
                                && node._gCost > currentNode._gCost + currentCost)
                            {
                                var tmpNode = node;
                                tmpNode._parent.i = currentPoint.i;
                                tmpNode._parent.j = currentPoint.j;
                                tmpNode._gCost = currentNode._gCost + currentCost;
                                dontPut = true;
                                break;
                            }
                        }
                        if (!dontPut)
                        {
                            openList.Add(new Node(currentPoint.j + l, currentPoint.i + k, currentPoint.j, currentPoint.i, currentCost));
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
            else if(0 == closedList.Count)
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
}