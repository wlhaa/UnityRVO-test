using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public static Simulator Instance { get; private set; }
    private List<Agent> agents;
    private KDTree kdTree;
    public GameObject circle;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        agents = new List<Agent>();
        kdTree = new KDTree();
    }

    void Update()
    {
        foreach (Agent agent in agents)
        {
            agent.ComputeNewVelocity();
            agent.UpdatePosition();
        }
    }

    public void AddAgent(Vector2 position)
    {
        //Instantiate(circle);
        //GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject gameObject = GameObject.Instantiate(circle,position,transform.rotation);
        gameObject.name = "Agent";
        Agent newAgent = gameObject.AddComponent<Agent>();
        newAgent.Initialize(position);
        agents.Add(newAgent);
    }

    public int GetNumAgents()
    {
        return agents.Count;
    }

    public Vector2 GetAgentPosition(int index)
    {
        return agents[index].transform.position;
    }

    public void SetAgentPrefVelocity(int index, Vector2 velocity)
    {
        agents[index].SetGoal(velocity);
    }

    // Adding the DoStep method to update simulation steps
    public void DoStep()
    {
        // Simulate one step in the environment
        foreach (var agent in agents)
        {
            // Assume UpdatePosition processes movement based on current velocity
            agent.UpdatePosition();
        }
    }
}
