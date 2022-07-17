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
    public Renderer selectorRenderer;
    public Color colorSelectorEnabled, colorSelectorDisabled;
    [Space]
    public GameObject ballPrefab;
    public List<Material> ballMaterials = new List<Material>();
    [Space]
    public GameObject examples;

    private GridCell[,] grid;
    private Vector2Int selector_gridPosition;
    private int selectorOrientation; // 0 = horizontal, 1 = vertical
    private bool lockInput;

    private void Start()
    {
        examples.SetActive(false);

        resetGrid();
    }

    void Update()
    {
        inputs();
    }

    private void resetGrid()
    {
        // deletar grid anterior, se houver
        if (grid != null)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                for (int i = 0; i < gridSize.x; i++)
                {
                    if (grid[i, j] != null)
                    {
                        Destroy(grid[i, j].ball.gameObject);
                        grid[i, j] = null;
                    }
                }
            }
        }

        StartCoroutine(_spawnNewGrid());
    }

    private IEnumerator _spawnNewGrid()
    {
        lockInput = true;
        selector.gameObject.SetActive(false);

        initializeGrid();

        for (int j = 0; j < gridSize.y; j++)
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                Vector3 worldPos = gridToWorldPosition(i, j);

                grid[i, j].ball = spawnBall(worldPos.x, grid[i, j].ballType);

                yield return new WaitForSeconds(.1f);
            }
        }

        selector.gameObject.SetActive(true);
        lockInput = false;
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
                for (int k = 0; k < ballMaterials.Count; k++)
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
            // se encontrar celula nula, ou encontrar um tipo diferente, finalizar match
            if (grid[col, pickedRow] == null || grid[col, pickedRow].ballType != previousType)
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
            if (grid[pickedCol, row] == null || grid[pickedCol, row].ballType != previousType)
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

    private GridBall spawnBall(float xPos, int ballType)
    {
        GameObject newBallObject = Instantiate(ballPrefab, Vector3.up * 10 + Vector3.right * xPos, Quaternion.identity, gridParent);
        GridBall newBall = newBallObject.GetComponent<GridBall>();
        Renderer newRenderer = newBall.GetComponent<Renderer>();

        newRenderer.sharedMaterial = ballMaterials[ballType];

        return newBall;
    }

    private void inputs()
    {
        if (lockInput)
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            changeSelectorOrientation();
            updateSelectorGraphics();
        }
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

    private Vector3 gridToWorldPosition(float gridX, float gridY)
    {
        // calcular posX de forma centralizada, caso seja par ou impar
        float lerpBounds = 0;
        if (gridSize.x % 2 == 0) // se par
            lerpBounds = xSpacing / 2f + ((gridSize.x / 2f) - 1) * xSpacing;
        else // se impar
            lerpBounds = Mathf.Floor(gridSize.x / 2f) * xSpacing;
        float posX = Mathf.Lerp(-lerpBounds, lerpBounds, gridX / (gridSize.x - 1));

        float posY = gridY + ySpacing * gridY + yOffset;

        return new Vector3(posX, posY, 0);
    }

    // trocar bolas de posicao
    private void trySwapBalls()
    {
        if (areSelectedBallsSwappable())
            StartCoroutine(_swapBalls());
    }

    private bool areSelectedBallsSwappable()
    {
        GridCell cellA = grid[selector_gridPosition.x, selector_gridPosition.y];

        if (cellA == null || cellA.ball == null || ballsCurrentlyAnimating.Contains(cellA.ball))
            return false;

        GridCell cellB;

        Vector2Int ballB_gridPosition;
        if (selectorOrientation == 0)
            ballB_gridPosition = new Vector2Int(selector_gridPosition.x + 1, selector_gridPosition.y);
        else
            ballB_gridPosition = new Vector2Int(selector_gridPosition.x, selector_gridPosition.y + 1);
        cellB = grid[ballB_gridPosition.x, ballB_gridPosition.y];

        if (cellB == null || cellB.ball == null || ballsCurrentlyAnimating.Contains(cellB.ball))
            return false;

        return true;
    }

    private IEnumerator _swapBalls()
    {
        lockInput = true;

        // travar todas as fisicas para nao acontecer acidentes
        foreach (var cell in grid)
        {
            if (cell == null)
                continue;
            cell.ball.thisRigidbody.isKinematic = true;
        }

        // determino quais sao as duas bolas a serem trocadas
        GridCell cellA = grid[selector_gridPosition.x, selector_gridPosition.y];
        GridCell cellB;

        Vector2Int ballB_gridPosition;
        if (selectorOrientation == 0)
            ballB_gridPosition = new Vector2Int(selector_gridPosition.x + 1, selector_gridPosition.y);
        else
            ballB_gridPosition = new Vector2Int(selector_gridPosition.x, selector_gridPosition.y + 1);
        cellB = grid[ballB_gridPosition.x, ballB_gridPosition.y];

        // calculo para qual posicao elas vao
        Vector3 aOrigin = cellA.ball.transform.position;
        Vector3 bOrigin = cellB.ball.transform.position;
        Vector3 aDest = cellB.ball.transform.position;
        Vector3 bDest = cellA.ball.transform.position;

        // realizo a troca logica
        grid[selector_gridPosition.x, selector_gridPosition.y] = cellB;
        grid[ballB_gridPosition.x, ballB_gridPosition.y] = cellA;

        // realizo a troca de posicao
        float swapDuration = .18f;
        float swapElapsed = 0;
        while (swapElapsed < swapDuration)
        {
            swapElapsed += Time.deltaTime;
            cellA.ball.transform.position = Vector3.Lerp(aOrigin, aDest, swapElapsed / swapDuration);
            cellB.ball.transform.position = Vector3.Lerp(bOrigin, bDest, swapElapsed / swapDuration);
            yield return 0;
        }
        cellA.ball.transform.position = aDest;
        cellB.ball.transform.position = bDest;

        // verificar novos matches nas posicoes trocadas
        List<GridMatch> matches = new List<GridMatch>();
        matches.AddRange(getMatchesAtCoord(selector_gridPosition.x, selector_gridPosition.y));
        matches.AddRange(getMatchesAtCoord(ballB_gridPosition.x, ballB_gridPosition.y));

        // marca todas as celulas encontradas nos matches para serem removidas
        HashSet<Vector2Int> matchedCoords = new HashSet<Vector2Int>();
        foreach (var match in matches)
            getCoordsFromMatch(match, matchedCoords);
        // remove todas as celulas (deixando buracos no grid)
        foreach (var coord in matchedCoords)
        {
            GridCell removedCell = grid[coord.x, coord.y];
            grid[coord.x, coord.y] = null;
            Destroy(removedCell.ball.gameObject);
        }
        // preenche os buracos no grid com as celulas de cima
        for (int col = 0; col < gridSize.x; col++)
        {
            for (int row = 0; row < gridSize.y - 1; row++)
            {
                if (grid[col, row] == null)
                {
                    for (int k = row + 1; k < gridSize.y; k++)
                    {
                        if (grid[col, k] != null)
                        {
                            grid[col, row] = grid[col, k];
                            grid[col, k] = null;
                            break;
                        }
                    }
                }
            }
        }

        // destravar fisicas
        foreach (var cell in grid)
        {
            if (cell == null)
                continue;
            cell.ball.thisRigidbody.isKinematic = false;
        }

        lockInput = false;
        updateSelectorGraphics();
    }

    private void getCoordsFromMatch(GridMatch match, HashSet<Vector2Int> matchedCoords)
    {
        for (int k = 0; k < match.size; k++)
        {
            Vector2Int coord = match.origin;
            if (match.orientation == 0)
                coord.x += k;
            else
                coord.y += k;

            matchedCoords.Add(coord);
        }
    }

    /*
     * um for que vai checando todas as bolas, todos os frames, pra ver se elas estao se movimentando ou nao
     * se elas estiverem se movimentando, nao pode interagir com elas. muda a cor do seletor pra quando tiver nelas
     * se elas nao estiverem se movimentando, pode interagir com elas
     * se elas estavam se movimentando, e acabaram de terminar seu movimento, verificar se parou em uma combinacao
     */
    private List<GridBall> ballsCurrentlyAnimating = new List<GridBall>();
    private void FixedUpdate()
    {
        bool ballChangedState = false;
        for (int col = 0; col < gridSize.x; col++)
        {
            for (int row = 0; row < gridSize.y; row++)
            {
                if (grid[col, row] == null || grid[col, row].ball == null)
                    continue;

                if (grid[col, row].ball.thisRigidbody.velocity.sqrMagnitude > .001f)
                {
                    if (!ballsCurrentlyAnimating.Contains(grid[col, row].ball))
                    {
                        ballsCurrentlyAnimating.Add(grid[col, row].ball);
                        if (!grid[col, row].ball.textPause.activeSelf)
                            grid[col, row].ball.textPause.SetActive(true);

                        ballChangedState = true;
                    }
                }
                else
                {
                    if (ballsCurrentlyAnimating.Contains(grid[col, row].ball))
                    {
                        ballsCurrentlyAnimating.Remove(grid[col, row].ball);
                        if (grid[col, row].ball.textPause.activeSelf)
                            grid[col, row].ball.textPause.SetActive(false);

                        ballChangedState = true;
                    }
                }
            }
        }

        if (ballChangedState)
            updateSelectorGraphics();
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
