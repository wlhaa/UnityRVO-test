using UnityEngine;
using RVO;
using RVOVector2 = RVO.Vector2; // ΪRVO��Vector2��ӱ���
using UnityVector2 = UnityEngine.Vector2; // ΪUnity��Vector2��ӱ���
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    private int agentIndex;
    private Vector3 targetPosition;
    private float speed;
    private bool hasReachedTarget = false;
    private bool isFinalPosition = false;

    // ��Ұ��ز���
    public float visionAngle = 170f; // ��Ұ�Ƕ�
    public float visionRadius = 5f; // ��Ұ�뾶
    private List<Vector3> neighborsPositions = new List<Vector3>(); // ��¼�ھӵ�λ��

    private Vector3 neighborPreviousPosition; // �ھ�֮ǰ��λ��
    private Vector3 neighborCurrentPosition; // �ھӵ�ǰ��λ��
    private float timeInterval; // ʱ����

    // ��ߺ��Ա�
    public float height;
    public bool isMale;

    public void Initialize(int index, Vector3 target, float agentSpeed, float agentHeight, bool agentIsMale)
    {
        agentIndex = index;
        targetPosition = target;
        speed = agentSpeed;
        height = agentHeight;
        isMale = agentIsMale;

        RVOVector2 direction = Normalize(new RVOVector2(targetPosition.x, targetPosition.z) - Simulator.Instance.getAgentPosition(agentIndex));
        RVOVector2 initialVelocity = direction * speed;
        Simulator.Instance.setAgentVelocity(agentIndex, initialVelocity);
    }

    public List<Vector3> GetNeighborsPositions()
    {
        return neighborsPositions;
    }

    public Vector3 GetPreferredVelocity()
    {
        RVOVector2 prefVelocity = Simulator.Instance.getAgentPrefVelocity(agentIndex);
        return new Vector3(prefVelocity.x(), 0, prefVelocity.y());
    }

    public RVOVector2 GetAgentPosition()
    {
        return Simulator.Instance.getAgentPosition(agentIndex);
    }

    public RVOVector2 GetAgentVelocity()
    {
        return Simulator.Instance.getAgentVelocity(agentIndex);
    }

    public void SetAgentPrefVelocity(RVOVector2 prefVelocity)
    {
        Simulator.Instance.setAgentPrefVelocity(agentIndex, prefVelocity);
    }

    RVOVector2 Normalize(RVOVector2 v)
    {
        float magnitude = Mathf.Sqrt(v.x() * v.x() + v.y() * v.y());
        if (magnitude > 0)
        {
            return new RVOVector2(v.x() / magnitude, v.y() / magnitude);
        }
        return new RVOVector2(0, 0);
    }

    float RVOVector2Magnitude(RVOVector2 v)
    {
        return Mathf.Sqrt(v.x() * v.x() + v.y() * v.y());
    }

    void Update()
    {
        if (!isFinalPosition)
        {
            RVOVector2 target2D = new RVOVector2(targetPosition.x, targetPosition.z);
            RVOVector2 position2D = Simulator.Instance.getAgentPosition(agentIndex);

            if (!hasReachedTarget)
            {
                // Compute desired velocity towards target
                RVOVector2 desiredVelocity = Normalize(target2D - position2D) * speed;
                SetAgentPrefVelocity(desiredVelocity);

                // Check if the agent reached its target
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    hasReachedTarget = true;
                }
            }
            else
            {
                // ����ʹ��RVO���б��������ٶȺܵ�
                RVOVector2 desiredVelocity = Normalize(target2D - position2D) * (speed * 0.1f);
                SetAgentPrefVelocity(desiredVelocity);

                // ����������Ƿ��ȶ���Ŀ��λ��
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f && RVOVector2Magnitude(desiredVelocity) < 0.01f)
                {
                    isFinalPosition = true;
                    SetAgentPrefVelocity(new RVOVector2(0, 0));
                }
            }

            // ������Ұ
            UpdateVision();
        }
    }

    void UpdateVision()
    {
        if (isFinalPosition)
            return;

        // ��ȡ��Ұ��Χ�ڵ��ھ�
        neighborsPositions.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRadius);
        RVOVector2 velocity2D = GetAgentVelocity();
        Vector3 velocity = new Vector3(velocity2D.x(), 0, velocity2D.y());

        foreach (var hit in hits)
        {
            if (hit.gameObject != this.gameObject)
            {
                Vector3 directionToNeighbor = hit.transform.position - transform.position;
                float angle = Vector3.Angle(velocity, directionToNeighbor);
                if (angle <= visionAngle / 2)
                {
                    neighborsPositions.Add(hit.transform.position);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Simulator.Instance == null || agentIndex >= Simulator.Instance.getNumAgents())
        {
            return;
        }

        // ���ӻ��˶�����ǰ��
        RVOVector2 velocity2D = GetAgentVelocity();
        Vector3 velocity = new Vector3(velocity2D.x(), 0, velocity2D.y());

        Gizmos.color = Color.green;
        Vector3 forward = velocity.normalized * 2.0f; // ʹ��һ���̵����߱�ʾ��ǰ��
        Gizmos.DrawLine(transform.position, transform.position + forward);
    }

    // ���㲽���ĺ���
    public float CalculateStepLength(float height, bool isMale)
    {
        float k = isMale ? 0.415f : 0.413f;
        return k * height;
    }

    // ���㲽Ƶ�ĺ���
    public float EstimateStepFrequency(Vector3 previousPosition, Vector3 currentPosition, float stepLength, float timeInterval)
    {
        float distance = Vector3.Distance(previousPosition, currentPosition);
        return distance / (stepLength * timeInterval);
    }

    // �����ھ��ٶȵĺ���
    public Vector3 EstimateNeighborVelocity(Vector3 previousPosition, Vector3 currentPosition, float height, bool isMale, float timeInterval)
    {
        float stepLength = CalculateStepLength(height, isMale);
        float stepFrequency = EstimateStepFrequency(previousPosition, currentPosition, stepLength, timeInterval);
        return stepFrequency * stepLength * (currentPosition - previousPosition).normalized;
    }
}
