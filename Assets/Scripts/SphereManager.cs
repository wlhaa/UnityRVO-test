using UnityEngine;
using RVO;
using System.Collections.Generic;
using RVOVector2 = RVO.Vector2;

public class SphereManager : MonoBehaviour
{
    public GameObject agentPrefab; // 智能体预设
    public int agentsPerQuadrant = 25; // 每个象限中的智能体数量
    public float quadrantSpacing = 40.0f; // 方阵离原点的距离（增大以避免与障碍物重叠）
    public float agentSpacing = 2.0f; // 智能体之间的间距
    public float speed = 2.0f; // 智能体的移动速度

    private Simulator simulator; // RVO模拟器
    private ObstacleManager obstacleManager; // 障碍物管理器

    // 存储邻居的步频信息
    private Dictionary<int, (Vector3 previousPosition, Vector3 currentPosition)> neighborPositions = new Dictionary<int, (Vector3, Vector3)>();

    void Start()
    {
        // 初始化RVO模拟器
        simulator = Simulator.Instance;
        simulator.setTimeStep(0.25f);
        simulator.setAgentDefaults(15.0f, 10, 10.0f, 5.0f, 1.5f, 2.0f, new RVO.Vector2(0.0f, 0.0f));

        // 获取障碍物管理器
        obstacleManager = FindObjectOfType<ObstacleManager>();
        if (obstacleManager == null)
        {
            Debug.LogError("未找到ObstacleManager！");
            return;
        }

        int sideLength = Mathf.RoundToInt(Mathf.Sqrt(agentsPerQuadrant)); // 每个方阵的边长
        float halfLength = (sideLength - 1) * agentSpacing / 2.0f;
        Vector3[,] initialPositions = new Vector3[4, agentsPerQuadrant]; // 初始位置
        Vector3[,] targetPositions = new Vector3[4, agentsPerQuadrant]; // 目标位置

        // 颜色列表，每个象限一种颜色
        Color[] maleColors = { new Color(0.4f, 0, 0), new Color(0, 0.4f, 0), new Color(0, 0, 0.4f), new Color(0.4f, 0.4f, 0) };
        Color[] femaleColors = { Color.red, Color.green, Color.blue, Color.yellow };

        // 初始化每个象限的初始位置和目标位置
        for (int q = 0; q < 4; q++)
        {
            float xOffset = (q % 2 == 0 ? -1 : 1) * (quadrantSpacing + halfLength);
            float zOffset = (q < 2 ? 1 : -1) * (quadrantSpacing + halfLength);
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    float x = xOffset + (i - halfLength / agentSpacing) * agentSpacing;
                    float z = zOffset + (j - halfLength / agentSpacing) * agentSpacing;
                    int index = i * sideLength + j;
                    initialPositions[q, index] = new Vector3(x, 0, z);

                    // 计算目标位置，按行顺序填补
                    int row = index / sideLength;
                    int col = index % sideLength;
                    float targetX = -xOffset + (col - halfLength / agentSpacing) * agentSpacing;
                    float targetZ = -zOffset + (row - halfLength / agentSpacing) * agentSpacing;
                    targetPositions[q, index] = new Vector3(targetX, 0, targetZ);
                }
            }
        }

        // 生成智能体并设置其初始位置和目标位置
        for (int q = 0; q < 4; q++)
        {
            for (int i = 0; i < agentsPerQuadrant; i++)
            {
                Vector3 initialPosition = initialPositions[q, i];
                Vector3 targetPosition = targetPositions[q, i];
                RVO.Vector2 initialPos2D = new RVO.Vector2(initialPosition.x, initialPosition.z);
                simulator.addAgent(initialPos2D);
                GameObject agent = Instantiate(agentPrefab, initialPosition, Quaternion.identity);

                // 分配颜色和高度
                bool isMale = Random.value > 0.5f;
                Color agentColor = isMale ? maleColors[q] : femaleColors[q];
                Renderer renderer = agent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = agentColor;
                }

                float meanSpeed = 1.0f; // 平均速度
                float stdDev = 0.3f; // 标准差
                float initialSpeed = Mathf.Clamp(NormalDistribution(meanSpeed, stdDev), 0.1f, speed);

                // 随机设置圆柱体高度
                float height = RandomHeight(isMale);
                agent.transform.localScale = new Vector3(1.0f, height / 1.0f, 1.0f); // /2.0f因为localScale是从中心到边的距离

                // 获取Agent组件并初始化
                Agent agentScript = agent.GetComponent<Agent>();
                if (agentScript != null)
                {
                    agentScript.Initialize(simulator.getNumAgents() - 1, targetPosition, initialSpeed, height, isMale);
                }
                else
                {
                    Debug.LogError("未找到Agent组件！");
                }
            }
        }

        // 添加障碍物
        obstacleManager.AddObstacles(quadrantSpacing / 1.5f); // 障碍物离原点更近
    }

    void Update()
    {
        // 遍历所有智能体，记录他们的位置并计算邻居的速度
        foreach (var agent in FindObjectsOfType<Agent>())
        {
            int agentId = agent.GetInstanceID();
            if (!neighborPositions.ContainsKey(agentId))
            {
                neighborPositions[agentId] = (agent.transform.position, agent.transform.position);
            }
            else
            {
                var positions = neighborPositions[agentId];
                neighborPositions[agentId] = (positions.currentPosition, agent.transform.position);
            }
        }

        // 估算邻居速度
        foreach (var agent in FindObjectsOfType<Agent>())
        {
            var neighborsPositions = agent.GetNeighborsPositions();
            foreach (var neighborPos in neighborsPositions)
            {
                Collider[] colliders = Physics.OverlapSphere(neighborPos, 0.1f);
                foreach (var collider in colliders)
                {
                    Agent neighborAgent = collider.GetComponent<Agent>();
                    if (neighborAgent != null)
                    {
                        int neighborId = neighborAgent.GetInstanceID();
                        if (neighborPositions.ContainsKey(neighborId))
                        {
                            var positions = neighborPositions[neighborId];
                            float timeInterval = Time.deltaTime;

                            Vector3 estimatedVelocity = agent.EstimateNeighborVelocity(positions.previousPosition, positions.currentPosition, neighborAgent.height, neighborAgent.isMale, timeInterval);
                            // 使用estimatedVelocity进行进一步处理
                        }
                    }
                }
            }
        }

        // 更新RVO模拟器
        simulator.doStep();

        // 更新智能体位置
        foreach (var agent in FindObjectsOfType<Agent>())
        {
            RVOVector2 newPosition2D = agent.GetAgentPosition();
            agent.transform.position = new Vector3(newPosition2D.x(), agent.transform.position.y, newPosition2D.y());
        }
    }

    float NormalDistribution(float mean, float stdDev)
    {
        float u1 = Random.value;
        float u2 = Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    // 随机生成高度
    float RandomHeight(bool isMale)
    {
        if (isMale)
        {
            return Random.Range(1.68f, 1.85f);
        }
        else
        {
            return Random.Range(1.60f, 1.75f);
        }
    }
}
