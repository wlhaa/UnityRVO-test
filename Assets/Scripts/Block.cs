using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
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

        // ȷ�� setupScenario ֻ������һ��
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

    bool scenarioSetup = false;  // ����һ����־����ȷ�� setupScenario ֻ����һ��

    // Sets up the initial scenario
    private void setupScenario()
    {
        // Example: setup agents and their goals
        // This will depend on how Simulator and Vector2 are defined in your project
        //for (int i = 0; i < 100; i++)
        //{
            //Vector2 startPos = new Vector2(UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
            //Vector2 goalPos = new Vector2(UnityEngine.Random.Range(10, 20), UnityEngine.Random.Range(10, 20));
            //Simulator.Instance.AddAgent(startPos);//����ʼλ����Ӵ���
            //goals.Add(goalPos);

            //Vector2 StarP1 = new Vector2(55.0f + i * 1.0f, 55.0f + j * 1.0f);
            //Simulator.Instance.AddAgent(StarP1);
            //goals.Add(new Vector2(-75.0f, -75.0f));
        //}
        for (int i = 0; i < 15; ++i)
        {
            for (int j = 0; j < 15; ++j)
            {
                Vector2 StarP1 = new Vector3(5.50f + i * 1.0f,0, 5.50f + j * 1.0f);
                Simulator.Instance.AddAgent(StarP1);
                goals.Add(new Vector2(-20.50f, -20.50f));

                Vector2 StarP2 = new Vector3(-5.50f - i * 1.0f,0, 5.50f + j * 1.0f);
                Simulator.Instance.AddAgent(StarP2);
                goals.Add(new Vector2(20.50f, -20.0f));

                Vector2 StarP3 = new Vector3(5.50f + i * 1.0f,0, -5.50f - j * 1.0f);
                Simulator.Instance.AddAgent(StarP3);
                goals.Add(new Vector2(20.0f, 20.0f));

                Vector2 StarP4 = new Vector3(-5.50f - i * 1.0f,0, -5.50f - j * 1.0f);
                Simulator.Instance.AddAgent(StarP4);
                goals.Add(new Vector2(20.0f, 20.0f));
            }
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
