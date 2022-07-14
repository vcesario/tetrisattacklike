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

    private GridBall[,] grid;
    public Vector2Int selector_gridPosition;
    private int selectorOrientation; // 0 = horizontal, 1 = vertical

    private bool lockInput;

    IEnumerator Start()
    {
        examples.SetActive(false);

        lockInput = true;
        selector.gameObject.SetActive(false);

        grid = new GridBall[(int)gridSize.x, (int)gridSize.y];

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
    }

    void Update()
    {
        if (lockInput)
            return;

        inputs();
        updateSelectorWorldPosition();
    }

    private void spawnBall(float xPos, int gridI, int gridJ)
    {
        GameObject newBallObject = Instantiate(ballPrefab, Vector3.up * 10 + Vector3.right * xPos, Quaternion.identity, gridParent);
        GridBall newBall = newBallObject.GetComponent<GridBall>();
        Renderer newRenderer = newBall.GetComponent<Renderer>();

        int materialIndex = Random.Range(0, ballMaterials.Count);
        newRenderer.sharedMaterial = ballMaterials[materialIndex];

        grid[gridI, gridJ] = newBall;
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
    }

    private void moveSelectorUp()
    {
        selector_gridPosition.y++;

        if (selector_gridPosition.y >= gridSize.y) selector_gridPosition.y = 0;
    }
    private void moveSelectorDown()
    {
        selector_gridPosition.y--;

        if (selector_gridPosition.y < 0) selector_gridPosition.y = (int)gridSize.y - 1;
    }
    private void moveSelectorLeft()
    {
        selector_gridPosition.x--;

        if (selector_gridPosition.x < 0) selector_gridPosition.x = (int)gridSize.x - 2;
    }
    private void moveSelectorRight()
    {
        selector_gridPosition.x++;

        // se estiver no sentido horizontal, o selector nao pode ficar na ultima coluna pois ele ja considera a bola a direita
        if (selector_gridPosition.x >= (int)gridSize.x - 1) selector_gridPosition.x = 0;
    }

    private void updateSelectorWorldPosition()
    {
        GridBall selectedBall = grid[selector_gridPosition.x, selector_gridPosition.y];
        Vector3 selectedBall_worldPosition = selectedBall.transform.position;

        // para casos horizontais, a posicao do seletor sera a media entre a bola selecionada e a bola a direita
        GridBall rightBall = grid[selector_gridPosition.x + 1, selector_gridPosition.y];
        Vector3 rightBall_worldPosition = rightBall.transform.position;

        selector.transform.position = (selectedBall_worldPosition + rightBall_worldPosition) / 2;
    }
}
