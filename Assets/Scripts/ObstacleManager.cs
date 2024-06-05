using UnityEngine;
using RVO;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab; // 障碍物预设
    public float obstacleSize = 5.0f; // 障碍物的大小
    public float obstacleDistance = 25.0f; // 障碍物之间的距离

    public void AddObstacles(float spacing)
    {
        float obstacleOffset = obstacleDistance; // 障碍物离原点的距离，确保位于方阵之间
        Vector3[] obstaclePositions = {
            new Vector3(-obstacleOffset, 0, -obstacleOffset),
            new Vector3(obstacleOffset, 0, -obstacleOffset),
            new Vector3(-obstacleOffset, 0, obstacleOffset),
            new Vector3(obstacleOffset, 0, obstacleOffset)
        };

        // 生成并添加障碍物到场景和RVO模拟器中
        foreach (Vector3 pos in obstaclePositions)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(obstacleSize, 1.0f, obstacleSize);
            AddObstacleToSimulator(pos, obstacleSize / 2);
        }

        // 处理障碍物
        Simulator.Instance.processObstacles();
    }

    void AddObstacleToSimulator(Vector3 pos, float radius)
    {
        IList<RVO.Vector2> obstacle = new List<RVO.Vector2>();
        obstacle.Add(new RVO.Vector2(pos.x - radius, pos.z - radius));
        obstacle.Add(new RVO.Vector2(pos.x + radius, pos.z - radius));
        obstacle.Add(new RVO.Vector2(pos.x + radius, pos.z + radius));
        obstacle.Add(new RVO.Vector2(pos.x - radius, pos.z + radius));
        Simulator.Instance.addObstacle(obstacle);
    }
}
