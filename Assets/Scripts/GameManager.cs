using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // largura, altura, e espacamento horizontal
    public Vector3 gridSize;
    public GameObject ballPrefab;
    [Space]
    public List<Material> ballMaterials = new List<Material>();
    [Space]
    public GameObject examples;

    IEnumerator Start()
    {
        examples.SetActive(false);

        for (int i = 0; i < gridSize.y; i++)
        {
            for (int j = 0; j < gridSize.x; j++)
            {
                // calcular posX de forma centralizada, caso seja par ou impar
                float lerpBounds = 0;
                if (gridSize.x % 2 == 0) // se par
                    lerpBounds = gridSize.z / 2f + ((gridSize.x / 2f) - 1) * gridSize.z;
                else // se impar
                    lerpBounds = Mathf.Floor(gridSize.x / 2f) * gridSize.z;
                float posX = Mathf.Lerp(-lerpBounds, lerpBounds, j / (gridSize.x - 1));

                dropBall(posX);

                yield return new WaitForSeconds(.1f);
            }
        }
    }

    void Update()
    {

    }

    private void dropBall(float xPos)
    {
        GameObject newBall = Instantiate(ballPrefab, Vector3.up * 10 + Vector3.right * xPos, Quaternion.identity, null);
        Renderer newRend = newBall.GetComponent<Renderer>();

        int materialIndex = Random.Range(0, ballMaterials.Count);
        newRend.sharedMaterial = ballMaterials[materialIndex];
    }
}
