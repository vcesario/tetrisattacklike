using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector3 gridSize; // largura, altura, e espacamento horizontal
    public Transform gridParent;
    public Transform selector;
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
        if (lockInput)
            return;

        inputs();
    }

    private void spawnBall(float xPos, int gridI, int gridJ)
    {
        GameObject newBallObject = Instantiate(ballPrefab, Vector3.up * 10 + Vector3.right * xPos, Quaternion.identity, gridParent);
        GridBall newBall = newBallObject.GetComponent<GridBall>();
        Renderer newRenderer = newBall.GetComponent<Renderer>();

        int materialIndex = Random.Range(0, ballMaterials.Count);
        newRenderer.sharedMaterial = ballMaterials[materialIndex];

        grid[gridI, gridJ] = new GridCell(newBall, materialIndex);
    }

    private void inputs()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveSelectorUp();
            updateSelectorWorldPosition();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveSelectorLeft();
            updateSelectorWorldPosition();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveSelectorDown();
            updateSelectorWorldPosition();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveSelectorRight();
            updateSelectorWorldPosition();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            changeSelectorOrientation();
            updateSelectorWorldPosition();
            updateSelectorWorldRotation();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            swapBalls();
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
    private void updateSelectorWorldRotation()
    {
        if (selectorOrientation == 0)
            selector.localRotation = Quaternion.identity;
        else
            selector.localRotation = Quaternion.Euler(0, 0, 90);
    }

    private void updateSelectorWorldPosition()
    {
        GridCell selectedCell = grid[selector_gridPosition.x, selector_gridPosition.y];
        Vector3 selectedBall_worldPosition = selectedCell.ball.transform.position;

        // para casos horizontais, a posicao do seletor sera a media entre a bola selecionada e a bola a direita
        if (selectorOrientation == 0)
        {
            GridCell rightCell = grid[selector_gridPosition.x + 1, selector_gridPosition.y];
            Vector3 rightBall_worldPosition = rightCell.ball.transform.position;

            selector.transform.position = (selectedBall_worldPosition + rightBall_worldPosition) / 2;
        }
        else // para casos verticais, e a media entre a selecionada e a de cima
        {
            GridCell topCell = grid[selector_gridPosition.x, selector_gridPosition.y + 1];
            Vector3 topBall_worldPosition = topCell.ball.transform.position;

            selector.transform.position = (selectedBall_worldPosition + topBall_worldPosition) / 2;
        }
    }

    // trocar bolas de posicao
    private void swapBalls()
    {
        StartCoroutine(_swapBalls());
    }

    private IEnumerator _swapBalls()
    {
        lockInput = true;

        // travar todas as fisicas para nao acontecer acidentes
        foreach (var cell in grid)
            cell.ball.thisRigidbody.isKinematic = true;

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

        // destravar fisicas
        foreach (var cell in grid)
            cell.ball.thisRigidbody.isKinematic = false;

        lockInput = false;
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
                // calcular posX de forma centralizada, caso seja par ou impar
                float lerpBounds = 0;
                if (gridSize.x % 2 == 0) // se par
                    lerpBounds = gridSize.z / 2f + ((gridSize.x / 2f) - 1) * gridSize.z;
                else // se impar
                    lerpBounds = Mathf.Floor(gridSize.x / 2f) * gridSize.z;
                float posX = Mathf.Lerp(-lerpBounds, lerpBounds, i / (gridSize.x - 1));

                spawnBall(posX, i, j);

                yield return new WaitForSeconds(.1f);
            }
        }

        selector.gameObject.SetActive(true);
        lockInput = false;
        updateSelectorWorldPosition();
        updateSelectorWorldRotation();
    }

    private void initializeGrid()
    {
        grid = new GridCell[(int)gridSize.x, (int)gridSize.y];
    }
}
