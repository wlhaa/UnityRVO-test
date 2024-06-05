using UnityEngine;
using RVO;
using System.Collections.Generic;
using RVOVector2 = RVO.Vector2;

public class SphereManager : MonoBehaviour
{
    public GameObject agentPrefab; // ������Ԥ��
    public int agentsPerQuadrant = 25; // ÿ�������е�����������
    public float quadrantSpacing = 40.0f; // ������ԭ��ľ��루�����Ա������ϰ����ص���
    public float agentSpacing = 2.0f; // ������֮��ļ��
    public float speed = 2.0f; // ��������ƶ��ٶ�

    private Simulator simulator; // RVOģ����
    private ObstacleManager obstacleManager; // �ϰ��������

    // �洢�ھӵĲ�Ƶ��Ϣ
    private Dictionary<int, (Vector3 previousPosition, Vector3 currentPosition)> neighborPositions = new Dictionary<int, (Vector3, Vector3)>();

    void Start()
    {
        // ��ʼ��RVOģ����
        simulator = Simulator.Instance;
        simulator.setTimeStep(0.25f);
        simulator.setAgentDefaults(15.0f, 10, 10.0f, 5.0f, 1.5f, 2.0f, new RVO.Vector2(0.0f, 0.0f));

        // ��ȡ�ϰ��������
        obstacleManager = FindObjectOfType<ObstacleManager>();
        if (obstacleManager == null)
        {
            Debug.LogError("δ�ҵ�ObstacleManager��");
            return;
        }

        int sideLength = Mathf.RoundToInt(Mathf.Sqrt(agentsPerQuadrant)); // ÿ������ı߳�
        float halfLength = (sideLength - 1) * agentSpacing / 2.0f;
        Vector3[,] initialPositions = new Vector3[4, agentsPerQuadrant]; // ��ʼλ��
        Vector3[,] targetPositions = new Vector3[4, agentsPerQuadrant]; // Ŀ��λ��

        // ��ɫ�б�ÿ������һ����ɫ
        Color[] maleColors = { new Color(0.4f, 0, 0), new Color(0, 0.4f, 0), new Color(0, 0, 0.4f), new Color(0.4f, 0.4f, 0) };
        Color[] femaleColors = { Color.red, Color.green, Color.blue, Color.yellow };

        // ��ʼ��ÿ�����޵ĳ�ʼλ�ú�Ŀ��λ��
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

                    // ����Ŀ��λ�ã�����˳���
                    int row = index / sideLength;
                    int col = index % sideLength;
                    float targetX = -xOffset + (col - halfLength / agentSpacing) * agentSpacing;
                    float targetZ = -zOffset + (row - halfLength / agentSpacing) * agentSpacing;
                    targetPositions[q, index] = new Vector3(targetX, 0, targetZ);
                }
            }
        }

        // ���������岢�������ʼλ�ú�Ŀ��λ��
        for (int q = 0; q < 4; q++)
        {
            for (int i = 0; i < agentsPerQuadrant; i++)
            {
                Vector3 initialPosition = initialPositions[q, i];
                Vector3 targetPosition = targetPositions[q, i];
                RVO.Vector2 initialPos2D = new RVO.Vector2(initialPosition.x, initialPosition.z);
                simulator.addAgent(initialPos2D);
                GameObject agent = Instantiate(agentPrefab, initialPosition, Quaternion.identity);

                // ������ɫ�͸߶�
                bool isMale = Random.value > 0.5f;
                Color agentColor = isMale ? maleColors[q] : femaleColors[q];
                Renderer renderer = agent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = agentColor;
                }

                float meanSpeed = 1.0f; // ƽ���ٶ�
                float stdDev = 0.3f; // ��׼��
                float initialSpeed = Mathf.Clamp(NormalDistribution(meanSpeed, stdDev), 0.1f, speed);

                // �������Բ����߶�
                float height = RandomHeight(isMale);
                agent.transform.localScale = new Vector3(1.0f, height / 1.0f, 1.0f); // /2.0f��ΪlocalScale�Ǵ����ĵ��ߵľ���

                // ��ȡAgent�������ʼ��
                Agent agentScript = agent.GetComponent<Agent>();
                if (agentScript != null)
                {
                    agentScript.Initialize(simulator.getNumAgents() - 1, targetPosition, initialSpeed, height, isMale);
                }
                else
                {
                    Debug.LogError("δ�ҵ�Agent�����");
                }
            }
        }

        // ����ϰ���
        obstacleManager.AddObstacles(quadrantSpacing / 1.5f); // �ϰ�����ԭ�����
    }

    void Update()
    {
        // �������������壬��¼���ǵ�λ�ò������ھӵ��ٶ�
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

        // �����ھ��ٶ�
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
                            // ʹ��estimatedVelocity���н�һ������
                        }
                    }
                }
            }
        }

        // ����RVOģ����
        simulator.doStep();

        // ����������λ��
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

    // ������ɸ߶�
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
