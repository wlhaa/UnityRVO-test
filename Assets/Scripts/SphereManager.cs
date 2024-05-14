using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour
{
    public GameObject spherePrefab;
    public int gridSize = 5;
    public float spacing = 2.0f;
    public float distanceFromOrigin = 10.0f;  // 调整方阵距离原点的距离
    public float moveDuration = 30.0f;        // 调整运动持续时间（增加至30秒）
    private List<GameObject> spheres = new List<GameObject>();

    void Start()
    {
        GenerateSpheres();
        StartCoroutine(MoveSpheres());
    }

    void GenerateSpheres()
    {
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        Vector3 position = new Vector3(i * (x * spacing + distanceFromOrigin), 0, j * (z * spacing + distanceFromOrigin));
                        GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                        spheres.Add(sphere);
                    }
                }
            }
        }
    }

    IEnumerator MoveSpheres()
    {
        Vector3[] targets = new Vector3[spheres.Count];
        for (int i = 0; i < spheres.Count; i++)
        {
            Vector3 position = spheres[i].transform.position;
            targets[i] = new Vector3(-position.x, position.y, -position.z);
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < moveDuration)
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                spheres[i].transform.position = Vector3.Lerp(spheres[i].transform.position, targets[i], elapsedTime / moveDuration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < spheres.Count; i++)
        {
            spheres[i].transform.position = targets[i];
        }
    }
}
