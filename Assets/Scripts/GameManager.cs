using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2Int gridSize;
    public float xSpacing, ySpacing, yOffset;
    public Transform gridParent;
    [Space]
    public Transform selector;
    public BoxCollider selectorTrigger;
    public Renderer selectorRenderer;
    public Color colorSelectorEnabled, colorSelectorDisabled;
    [Space]
    public GameObject ballPrefab;
    public Material outlineMat;
    public Material ballMat;
    public List<Color> ballColors = new List<Color>();
    //public List<Material> ballMaterials = new List<Material>();
    [Space]
    public GameObject examples;

    private GridCell[,] grid;
    private List<GridBall> allBalls = new List<GridBall>();
    private Vector2Int selector_gridPosition;
    private int selectorOrientation; // 0 = horizontal, 1 = vertical
    private bool isInitializing;

    #region Callbacks da Unity
    private void Start()
    {
        examples.SetActive(false);

        resetGrid();
    }

    private void Update()
    {
        inputs();

        ballRefill();
    }


    /*
     * um for que vai checando todas as bolas, todos os frames, pra ver se elas estao se movimentando ou nao
     * se elas estavam se movimentando, e acabaram de terminar seu movimento, verificar se parou em uma combinacao
     */
    private void FixedUpdate()
    {
        bool ballChangedState = false;

        for (int k = 0; k < allBalls.Count; k++)
        {
            if (allBalls[k] == null)
                continue;

            if (allBalls[k].thisRigidbody.velocity.sqrMagnitude > .001f)
            {
                if (allBalls[k].animateCooldown <= 0f) // se acabou de comecar a animar
                {
                    if (!allBalls[k].textPause.activeSelf)
                        allBalls[k].textPause.SetActive(true);

                    ballChangedState = true;
                }

                allBalls[k].animateCooldown = .25f;
            }
            else
            {
                if (allBalls[k].animateCooldown > 0f)
                {
                    allBalls[k].animateCooldown -= Time.deltaTime;
                    if (allBalls[k].animateCooldown <= 0f) // se acabou de parar
                    {
                        if (allBalls[k].textPause.activeSelf)
                            allBalls[k].textPause.SetActive(false);

                        ballChangedState = true;

                        findPhysicsMatches(allBalls[k].transform.position);
                    }
                }
            }
        }

        if (ballChangedState)
            updateSelectorGraphics();
    }
    #endregion

    #region Initialization
    private void resetGrid()
    {
        // deletar grid anterior, se houver
        for (int k = 0; k < allBalls.Count; k++)
            Destroy(allBalls[k].gameObject);
        allBalls.Clear();

        StartCoroutine(_spawnNewGrid());
    }

    private IEnumerator _spawnNewGrid()
    {
        isInitializing = true;
        selector.gameObject.SetActive(false);

        initializeGrid();

        for (int j = 0; j < gridSize.y; j++)
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                Vector3 worldPos = gridToWorldPosition(i, j);

                allBalls.Add(spawnBall(worldPos.x, grid[i, j].ballType));

                // espera por X frames
                for (int f = 0; f < 10; f++)
                    yield return 0;
            }
        }

        selector.gameObject.SetActive(true);
        isInitializing = false;
        updateSelectorGraphics();
    }

    private void initializeGrid()
    {
        grid = new GridCell[gridSize.x, gridSize.y];

        for (int j = 0; j < gridSize.y; j++)
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                GridCell newCell = new GridCell();
                grid[i, j] = newCell;

                // checo quais tipos podem ser inicializados naquela celula

                // 1. atribuo a celula com um tipo aleatorio, excluindo os de tentativas passadas
                List<int> allTypes = new List<int>();
                for (int k = 0; k < ballColors.Count; k++)
                    allTypes.Add(k);
                List<int> exclude = new List<int>();
                List<GridMatch> matches;

                do
                {
                    List<int> typesToChose = allTypes.FindAll(_t => !exclude.Contains(_t));
                    int pickedType = typesToChose[Random.Range(0, typesToChose.Count)];
                    newCell.ballType = pickedType;

                    // 2. verifico se apos essa atribuicao, o grid possui matches para tratar
                    matches = getMatchesAtCoord(i, j);

                    // 3. se possuir matches, adiciono o tipo para lista de excludes e tento de novo
                    if (matches.Count > 0)
                        exclude.Add(pickedType);
                } while (matches.Count > 0);

                // se nao houver matches naquela posicao com aquele tipo, assumo que encontrei o tipo certo e passo para o proximo
            }
        }
    }

    // checa se existem 3 ou mais bolas do mesmo tipo em sequencia naquela coordenada do grid
    private List<GridMatch> getMatchesAtCoord(int pickedCol, int pickedRow)
    {
        List<GridMatch> allMatches = new List<GridMatch>();

        int previousType = -1;
        int startCol = -1;
        int matchCount = -1;

        for (int col = 0; col < gridSize.x; col++)
        {
            // finalizar match se: encontrar celula nula, ou encontrar um tipo diferente, ou bola estiver animando
            if (grid[col, pickedRow] == null
                || grid[col, pickedRow].ballType != previousType)
            {
                // verifica se ja estava contando um match valido ate agora. se sim, adiciona na lista
                if (matchCount >= 3)
                    allMatches.Add(new GridMatch(startCol, pickedRow, matchCount, 0));

                // reseta variaveis para comecar a contar um novo match
                previousType = grid[col, pickedRow] == null ? -1 : grid[col, pickedRow].ballType;
                startCol = grid[col, pickedRow] == null ? -1 : col;
                matchCount = 1;
            }
            else // se tipo da bola atual for igual a anterior
            {
                // incremento o tamanho do combo
                matchCount++;
            }
        }

        // se chegar ao final com match valido, adicionar tambem
        if (matchCount >= 3)
            allMatches.Add(new GridMatch(startCol, pickedRow, matchCount, 0));

        // faco a mesma coisa, porem agora na vertical
        int startRow = -1;
        previousType = -1;
        matchCount = -1;

        for (int row = 0; row < gridSize.y; row++)
        {
            if (grid[pickedCol, row] == null
                || grid[pickedCol, row].ballType != previousType)
            {
                if (matchCount >= 3)
                    allMatches.Add(new GridMatch(pickedCol, startRow, matchCount, 1));

                previousType = grid[pickedCol, row] == null ? -1 : grid[pickedCol, row].ballType;
                startRow = grid[pickedCol, row] == null ? -1 : row;
                matchCount = 1;
            }
            else
            {
                matchCount++;
            }
        }
        if (matchCount >= 3)
            allMatches.Add(new GridMatch(pickedCol, startRow, matchCount, 1));

        return allMatches;
    }
    #endregion

    #region Inputs
    private void inputs()
    {
        if (isInitializing)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            moveSelectorUp();
            updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveSelectorLeft();
            updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveSelectorDown();
            updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveSelectorRight();
            updateSelectorGraphics();
        }
        //else if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    changeSelectorOrientation();
        //    updateSelectorGraphics();
        //}
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            trySwapBalls();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            resetGrid();
        }
    }

    private void moveSelectorUp()
    {
        selector_gridPosition.y++;

        if (selectorOrientation == 0)
        {
            if (selector_gridPosition.y >= gridSize.y)
            {
                selector_gridPosition.y = 0;
            }
        }
        else
        {
            if (selector_gridPosition.y >= gridSize.y - 1)
            {
                selector_gridPosition.y = 0;
            }
        }
    }
    private void moveSelectorDown()
    {
        selector_gridPosition.y--;

        if (selectorOrientation == 0)
        {
            if (selector_gridPosition.y < 0) selector_gridPosition.y = (int)gridSize.y - 1;
        }
        else
        {
            if (selector_gridPosition.y < 0) selector_gridPosition.y = (int)gridSize.y - 2;
        }
    }
    private void moveSelectorLeft()
    {
        selector_gridPosition.x--;

        if (selectorOrientation == 0)
        {
            if (selector_gridPosition.x < 0) selector_gridPosition.x = (int)gridSize.x - 2;
        }
        else
        {
            if (selector_gridPosition.x < 0) selector_gridPosition.x = (int)gridSize.x - 1;
        }
    }
    private void moveSelectorRight()
    {
        selector_gridPosition.x++;

        // se estiver no sentido horizontal, o selector nao pode ficar na ultima coluna pois ele ja considera a bola a direita
        if (selectorOrientation == 0)
        {
            if (selector_gridPosition.x >= (int)gridSize.x - 1) selector_gridPosition.x = 0;
        }
        else
        {
            if (selector_gridPosition.x >= (int)gridSize.x) selector_gridPosition.x = 0;
        }
    }

    private void changeSelectorOrientation()
    {
        if (selectorOrientation == 0)
            selectorOrientation = 1;
        else
            selectorOrientation = 0;

        // corrigir posição atual. por ex: se eu estou na vertical, na ultima coluna, e mudo para orient. horizontal, preciso corrigir a coluna selecionada pra uma antes
        if (selectorOrientation == 0 && selector_gridPosition.x == gridSize.x - 1)
        {
            selector_gridPosition.x--;
        }
        else if (selectorOrientation == 1 && selector_gridPosition.y == gridSize.y - 1)
        {
            selector_gridPosition.y--;
        }
    }

    private void updateSelectorGraphics()
    {
        // atualiza position
        Vector3 selectedCell_worldPosition = gridToWorldPosition(selector_gridPosition.x, selector_gridPosition.y);

        // para casos horizontais, a posicao do seletor sera a media entre a bola selecionada e a bola a direita
        if (selectorOrientation == 0)
        {
            Vector3 rightCell_worldPosition = gridToWorldPosition(selector_gridPosition.x + 1, selector_gridPosition.y);
            selector.position = (selectedCell_worldPosition + rightCell_worldPosition) / 2f;
        }
        else // para casos verticais, e a media entre a selecionada e a de cima
        {
            Vector3 topCell_worldPosition = gridToWorldPosition(selector_gridPosition.x, selector_gridPosition.y + 1);
            selector.position = (selectedCell_worldPosition + topCell_worldPosition) / 2f;
        }

        // atualiza rotation
        if (selectorOrientation == 0)
            selector.localRotation = Quaternion.identity;
        else
            selector.localRotation = Quaternion.Euler(0, 0, 90);

        // atualiza estado do seletor
        selectorRenderer.sharedMaterial.color = areSelectedBallsSwappable() ? colorSelectorEnabled : colorSelectorDisabled;
    }

    // trocar bolas de posicao
    private void trySwapBalls()
    {
        //if (areSelectedBallsSwappable())
        StartCoroutine(_swapBalls());
    }

    private bool areSelectedBallsSwappable()
    {
        Debug.Log("refazer habilitancia do selector");
        //GridCell cellA = grid[selector_gridPosition.x, selector_gridPosition.y];

        //if (cellA == null || cellA.ball == null || ballsCurrentlyAnimating.Contains(cellA.ball))
        //    return false;

        //GridCell cellB;

        //Vector2Int ballB_gridPosition;
        //if (selectorOrientation == 0)
        //    ballB_gridPosition = new Vector2Int(selector_gridPosition.x + 1, selector_gridPosition.y);
        //else
        //    ballB_gridPosition = new Vector2Int(selector_gridPosition.x, selector_gridPosition.y + 1);
        //cellB = grid[ballB_gridPosition.x, ballB_gridPosition.y];

        //if (cellB == null || cellB.ball == null || ballsCurrentlyAnimating.Contains(cellB.ball))
        //    return false;

        return true;
    }

    private IEnumerator _swapBalls()
    {
        // determino quais sao as bolas a serem trocadas, vendo quais estao na posicao em que o selector esta
        Vector3 ballA_worldPos = gridToWorldPosition(selector_gridPosition.x, selector_gridPosition.y);
        Vector3 ballB_worldPos = gridToWorldPosition(selector_gridPosition.x + 1, selector_gridPosition.y);
        float triggerDetectSize = .25f;

        Collider[] ballsInside = Physics.OverlapSphere(ballA_worldPos, triggerDetectSize);
        Collider ballACollider = ballsInside.Length > 0 ? ballsInside[0] : null;
        GridBall ballA = ballACollider != null ? ballACollider.GetComponentInParent<GridBall>() : null;

        // faco a mesma coisa, agora para a bola 'oposta'
        ballsInside = Physics.OverlapSphere(ballB_worldPos, triggerDetectSize);
        Collider ballBCollider = ballsInside.Length > 0 ? ballsInside[0] : null;
        GridBall ballB = ballBCollider != null ? ballBCollider.GetComponentInParent<GridBall>() : null;

        // realizo a troca de posicao
        Vector3 aOrigin = Vector3.zero;
        Vector3 aDest = Vector3.zero;
        Vector3 bOrigin = Vector3.zero;
        Vector3 bDest = Vector3.zero;

        if (ballA != null)
        {
            ballA.thisRigidbody.isKinematic = true;
            ballACollider.isTrigger = true;

            aOrigin = ballA.transform.position;
            aDest = ballB_worldPos;
        }
        if (ballB != null)
        {
            ballB.thisRigidbody.isKinematic = true;
            ballBCollider.isTrigger = true;
            bOrigin = ballB.transform.position;
            bDest = ballA_worldPos;
        }

        float swapDuration = .15f;
        float swapElapsed = 0;

        while (swapElapsed < swapDuration)
        {
            swapElapsed += Time.deltaTime;
            if (ballA != null)
                ballA.transform.position = Vector3.Lerp(aOrigin, aDest, swapElapsed / swapDuration);
            if (ballB != null)
                ballB.transform.position = Vector3.Lerp(bOrigin, bDest, swapElapsed / swapDuration);
            yield return 0;
        }

        if (ballA != null)
        {
            ballA.transform.position = aDest;

            ballA.thisRigidbody.isKinematic = false;
            ballACollider.isTrigger = false;

            findPhysicsMatches(aDest);
        }
        if (ballB != null)
        {
            ballB.transform.position = bDest;

            ballB.thisRigidbody.isKinematic = false;
            ballBCollider.isTrigger = false;

            findPhysicsMatches(bDest);
        }
    }
    #endregion

    #region Refill
    public float refillCountdown = 3;
    private float refillCountdownElapsed = 0;
    public float refillCooldown = 1;
    private float refillCooldownElapsed = 0;
    private int refillCol = 0;
    private void ballRefill()
    {
        if (isInitializing)
        {
            refillCountdownElapsed = 0;
            refillCooldownElapsed = 0;
            refillCol = 0;
            return;
        }

        if (refillCountdownElapsed < refillCountdown)
        {
            refillCountdownElapsed += Time.deltaTime;
            return;
        }

        if (refillCooldownElapsed < refillCooldown)
        {
            refillCooldownElapsed += Time.deltaTime;
            return;
        }

        float col_worldX = gridToWorldPosition(refillCol, 0).x;
        spawnBall(col_worldX, Random.Range(0, ballColors.Count));

        refillCooldownElapsed = 0;
        refillCol = (refillCol + 1) % gridSize.x;
    }
    #endregion

    private GridBall spawnBall(float xPos, int ballType)
    {
        GameObject newBallObject = Instantiate(ballPrefab, Vector3.up * 10 + Vector3.right * xPos, Quaternion.identity, gridParent);
        GridBall newBall = newBallObject.GetComponent<GridBall>();
        Renderer newRenderer = newBall.GetComponent<Renderer>();

        newRenderer.sharedMaterials = new Material[]
        {
            outlineMat,
            ballMat
        };
        newRenderer.materials[1].SetColor("_Color", ballColors[ballType]);

        newBall.type = ballType;

        return newBall;
    }

    private Vector3 gridToWorldPosition(float gridX, float gridY)
    {
        // calcular posX de forma centralizada, caso seja par ou impar
        float lerpBounds = 0;
        if (gridSize.x % 2 == 0) // se par
            lerpBounds = xSpacing / 2f + ((gridSize.x / 2f) - 1) * xSpacing;
        else // se impar
            lerpBounds = Mathf.Floor(gridSize.x / 2f) * xSpacing;
        float posX = Mathf.Lerp(-lerpBounds, lerpBounds, gridX / (gridSize.x - 1));

        float posY = ySpacing * gridY + yOffset;

        return new Vector3(posX, posY, 0);
    }

    private void findPhysicsMatches(Vector3 center)
    {
        // detecto a bola central e seu tipo, pra saber qual o tipo do match que estou procurando
        float triggerDetectSize = .25f;
        Collider[] ballsInside = new Collider[1];
        if (Physics.OverlapSphereNonAlloc(center, triggerDetectSize, ballsInside) <= 0)
            return;
        Collider ballCollider = ballsInside.Length > 0 ? ballsInside[0] : null;
        GridBall centerBall = ballCollider != null ? ballCollider.GetComponentInParent<GridBall>() : null;
        int matchType = centerBall.type;

        // pego todas as bolas em linha reta acima que correspondam ao mesmo tipo, parando quando encontrar uma quebra na sequencia
        List<GridBall> matchedBallsAbove = new List<GridBall>();
        int count = 1;
        do
        {
            Vector3 abovePos = center + Vector3.up * ySpacing * count;
            if (Physics.OverlapSphereNonAlloc(abovePos, triggerDetectSize, ballsInside) <= 0)
            {
                count = 0;
            }
            else
            {
                Collider aboveColl = ballsInside[0];
                GridBall aboveBall = aboveColl.GetComponentInParent<GridBall>();
                if (aboveBall.type == matchType && aboveBall.thisRigidbody.isKinematic == false)
                {
                    matchedBallsAbove.Add(aboveBall);
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
        } while (count > 0);

        // repito o mesmo, para as outras 3 direcoes
        List<GridBall> matchedBallsBelow = new List<GridBall>();
        count = 1;
        do
        {
            Vector3 belowPos = center + Vector3.down * ySpacing * count;

            if (Physics.OverlapSphereNonAlloc(belowPos, triggerDetectSize, ballsInside) <= 0)
            {
                count = 0;
            }
            else
            {
                Collider belowColl = ballsInside[0];
                GridBall belowBall = belowColl.GetComponentInParent<GridBall>();
                if (belowBall.type == matchType && belowBall.thisRigidbody.isKinematic == false)
                {
                    matchedBallsBelow.Add(belowBall);
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
        } while (count > 0);

        List<GridBall> matchedBallsRight = new List<GridBall>();
        count = 1;
        do
        {
            Vector3 rightPos = center + Vector3.right * xSpacing * count;
            if (Physics.OverlapSphereNonAlloc(rightPos, triggerDetectSize, ballsInside) <= 0)
            {
                count = 0;
            }
            else
            {
                Collider rightColl = ballsInside[0];
                GridBall rightBall = rightColl.GetComponentInParent<GridBall>();
                if (rightBall.type == matchType && rightBall.thisRigidbody.isKinematic == false)
                {
                    matchedBallsRight.Add(rightBall);
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
        } while (count > 0);

        List<GridBall> matchedBallsLeft = new List<GridBall>();
        count = 1;
        do
        {
            Vector3 leftPos = center + Vector3.left * xSpacing * count;
            if (Physics.OverlapSphereNonAlloc(leftPos, triggerDetectSize, ballsInside) <= 0)
            {
                count = 0;
            }
            else
            {
                Collider leftColl = ballsInside[0];
                GridBall leftBall = leftColl.GetComponentInParent<GridBall>();
                if (leftBall.type == matchType && leftBall.thisRigidbody.isKinematic == false)
                {
                    matchedBallsLeft.Add(leftBall);
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
        } while (count > 0);

        // vejo se as bolas acima e abaixo somam 3 ou mais. o "+ 1" eh a bola central
        bool removeVertical = matchedBallsAbove.Count + matchedBallsBelow.Count + 1 >= 3;
        bool removeHorizontal = matchedBallsLeft.Count + matchedBallsRight.Count + 1 >= 3;

        if (removeVertical)
        {
            for (int k = 0; k < matchedBallsAbove.Count; k++)
            {
                allBalls.Remove(matchedBallsAbove[k]);
                Destroy(matchedBallsAbove[k].gameObject);
            }
            for (int k = 0; k < matchedBallsBelow.Count; k++)
            {
                allBalls.Remove(matchedBallsBelow[k]);
                Destroy(matchedBallsBelow[k].gameObject);
            }
        }

        if (removeHorizontal)
        {
            for (int k = 0; k < matchedBallsLeft.Count; k++)
            {
                allBalls.Remove(matchedBallsLeft[k]);
                Destroy(matchedBallsLeft[k].gameObject);
            }
            for (int k = 0; k < matchedBallsRight.Count; k++)
            {
                allBalls.Remove(matchedBallsRight[k]);
                Destroy(matchedBallsRight[k].gameObject);
            }
        }

        // por ultimo destruo a bola central, caso tenha feito match horizontal ou vertical
        if (removeVertical || removeHorizontal)
        {
            allBalls.Remove(centerBall);
            Destroy(centerBall.gameObject);
        }
    }
}

public class GridMatch
{
    public Vector2Int origin;
    public int size;
    public int orientation;

    public GridMatch(int i, int j, int _size, int _orientation)
    {
        origin = new Vector2Int(i, j);
        size = _size;
        orientation = _orientation;
    }

    public override string ToString()
    {
        return origin.ToString() + ", " + size + ", " + (orientation == 0 ? "Horizontal" : "Vertical");
    }
}
