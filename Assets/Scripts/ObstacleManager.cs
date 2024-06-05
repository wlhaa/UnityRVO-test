using UnityEngine;
using RVO;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab; // �ϰ���Ԥ��
    public float obstacleSize = 5.0f; // �ϰ���Ĵ�С
    public float obstacleDistance = 25.0f; // �ϰ���֮��ľ���

    public void AddObstacles(float spacing)
    {
        float obstacleOffset = obstacleDistance; // �ϰ�����ԭ��ľ��룬ȷ��λ�ڷ���֮��
        Vector3[] obstaclePositions = {
            new Vector3(-obstacleOffset, 0, -obstacleOffset),
            new Vector3(obstacleOffset, 0, -obstacleOffset),
            new Vector3(-obstacleOffset, 0, obstacleOffset),
            new Vector3(obstacleOffset, 0, obstacleOffset)
        };

        // ���ɲ�����ϰ��ﵽ������RVOģ������
        foreach (Vector3 pos in obstaclePositions)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(obstacleSize, 1.0f, obstacleSize);
            AddObstacleToSimulator(pos, obstacleSize / 2);
        }

        // �����ϰ���
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
