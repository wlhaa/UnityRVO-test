using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    private IList<Vector2> goals = new List<Vector2>();
    private // Random generation using UnityEngine.Random


    // Initialize the scenario in Unity's Start method
    void Start()
    {
        //        setupScenario();
    }

    // Regularly update the simulation in Unity's Update method
    void Update()
    {
        if (Simulator.Instance == null)
        {
            Debug.LogError("Simulator instance is null.");
            return;
        }

        // 确保 setupScenario 只被调用一次
        if (!scenarioSetup)
        {
            setupScenario();
            scenarioSetup = true;
        }

        setPreferredVelocities();
        Simulator.Instance.DoStep();

        if (reachedGoal())
        {
            Debug.Log("All agents have reached their goals.");
        }
    }

    bool scenarioSetup = false;  // 新增一个标志变量确保 setupScenario 只调用一次

    // Sets up the initial scenario
    private void setupScenario()
    {
        // Example: setup agents and their goals
        // This will depend on how Simulator and Vector2 are defined in your project
        //for (int i = 0; i < 100; i++)
        //{
        //Vector2 startPos = new Vector2(UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
        //Vector2 goalPos = new Vector2(UnityEngine.Random.Range(10, 20), UnityEngine.Random.Range(10, 20));
        //Simulator.Instance.AddAgent(startPos);//按初始位置添加代理
        //goals.Add(goalPos);

        //Vector2 StarP1 = new Vector2(55.0f + i * 1.0f, 55.0f + j * 1.0f);
        //Simulator.Instance.AddAgent(StarP1);
        //goals.Add(new Vector2(-75.0f, -75.0f));
        //}
        for (int i = 0; i < 250; ++i)
        {
            Simulator.Instance.AddAgent(2.00f *
                new Vector3((float)Math.Cos(i * 2.0f * Math.PI / 250.0f),0,
                    (float)Math.Sin(i * 2.0f * Math.PI / 250.0f)));
            goals.Add(-Simulator.Instance.GetAgentPosition(i));
        }
    }

    // Sets preferred velocities for agents
    private void setPreferredVelocities()
    {
        if (goals == null || goals.Count == 0)
        {
            Debug.LogError("Goals list is not initialized or empty.");
            return;
        }

        int numAgents = Simulator.Instance.GetNumAgents();
        if (goals.Count < numAgents)
        {
            Debug.LogError("Mismatch between number of agents and number of goals.");
            return;
        }

        for (int i = 0; i < numAgents; i++)
        {
            if (Simulator.Instance.GetAgentPosition(i) == null)
            {
                Debug.LogError("Agent position is null.");
                continue;  // Skip to next iteration
            }

            Vector2 goalVector = goals[i] - Simulator.Instance.GetAgentPosition(i);
            if (goalVector.sqrMagnitude > 1.0f)
            {
                goalVector = goalVector.normalized;
            }

            Simulator.Instance.SetAgentPrefVelocity(i, goalVector);
        }
    }

    // Checks if all agents reached their goals
    private bool reachedGoal()
    {
        for (int i = 0; i < Simulator.Instance.GetNumAgents(); i++)
        {
            if ((Simulator.Instance.GetAgentPosition(i) - goals[i]).sqrMagnitude > 1.0f)
            {
                return false;
            }
        }
        return true;
    }
}
