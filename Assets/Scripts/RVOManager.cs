using UnityEngine;
using RVO;

public class RVOManager : MonoBehaviour
{
    public float timeStep = 0.25f;
    public float neighborDist = 15.0f;
    public int maxNeighbors = 10;
    public float timeHorizon = 10.0f;
    public float radius = 1.5f;
    public float maxSpeed = 2.0f;

    void Start()
    {
        Simulator.Instance.setTimeStep(timeStep);
        Simulator.Instance.setAgentDefaults(neighborDist, maxNeighbors, timeHorizon, timeHorizon, radius, maxSpeed, new RVO.Vector2(0.0f, 0.0f));
    }
}
