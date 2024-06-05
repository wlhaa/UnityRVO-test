using UnityEngine;
using RVO;
using RVOVector2 = RVO.Vector2; // 为RVO的Vector2添加别名
using UnityVector2 = UnityEngine.Vector2; // 为Unity的Vector2添加别名
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    private int agentIndex;
    private Vector3 targetPosition;
    private float speed;
    private bool hasReachedTarget = false;
    private bool isFinalPosition = false;

    // 视野相关参数
    public float visionAngle = 170f; // 视野角度
    public float visionRadius = 5f; // 视野半径
    private List<Vector3> neighborsPositions = new List<Vector3>(); // 记录邻居的位置

    private Vector3 neighborPreviousPosition; // 邻居之前的位置
    private Vector3 neighborCurrentPosition; // 邻居当前的位置
    private float timeInterval; // 时间间隔

    // 身高和性别
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
                // 继续使用RVO进行避碰，但速度很低
                RVOVector2 desiredVelocity = Normalize(target2D - position2D) * (speed * 0.1f);
                SetAgentPrefVelocity(desiredVelocity);

                // 检查智能体是否稳定在目标位置
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f && RVOVector2Magnitude(desiredVelocity) < 0.01f)
                {
                    isFinalPosition = true;
                    SetAgentPrefVelocity(new RVOVector2(0, 0));
                }
            }

            // 更新视野
            UpdateVision();
        }
    }

    void UpdateVision()
    {
        if (isFinalPosition)
            return;

        // 获取视野范围内的邻居
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

        // 可视化运动的正前方
        RVOVector2 velocity2D = GetAgentVelocity();
        Vector3 velocity = new Vector3(velocity2D.x(), 0, velocity2D.y());

        Gizmos.color = Color.green;
        Vector3 forward = velocity.normalized * 2.0f; // 使用一条短的射线表示正前方
        Gizmos.DrawLine(transform.position, transform.position + forward);
    }

    // 计算步长的函数
    public float CalculateStepLength(float height, bool isMale)
    {
        float k = isMale ? 0.415f : 0.413f;
        return k * height;
    }

    // 估算步频的函数
    public float EstimateStepFrequency(Vector3 previousPosition, Vector3 currentPosition, float stepLength, float timeInterval)
    {
        float distance = Vector3.Distance(previousPosition, currentPosition);
        return distance / (stepLength * timeInterval);
    }

    // 计算邻居速度的函数
    public Vector3 EstimateNeighborVelocity(Vector3 previousPosition, Vector3 currentPosition, float height, bool isMale, float timeInterval)
    {
        float stepLength = CalculateStepLength(height, isMale);
        float stepFrequency = EstimateStepFrequency(previousPosition, currentPosition, stepLength, timeInterval);
        return stepFrequency * stepLength * (currentPosition - previousPosition).normalized;
    }
}
