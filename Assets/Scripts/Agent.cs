using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public Vector2 position;
    public Vector2 velocity;
    private IList<Vector2> neighbors = new List<Vector2>();
    private Vector2 goal;
    private float neighborDist = 15.0f;
    //private int maxNeighbors = 10;
    //private float timeHorizon = 5.0f;
    //private float radius = 0.5f;
    private float maxSpeed = 2.0f;

    private void Awake()
    {
        //GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        gameObject.transform.parent = transform;
        gameObject.transform.localScale = Vector3.one * 0.1f;
    }

    void Start()
    {
        position = new Vector2(transform.position.x, transform.position.y);
        goal = position;  // Assume goal is initially set to current position
    }

    void Update()
    {
        ComputeNewVelocity();
        Move();
    }

    public void ComputeNewVelocity()
    {
        Vector2 newVelocity = Vector2.zero;
        if (neighbors.Count > 0)
        {
            foreach (var neighbor in neighbors)
            {
                Vector2 dirToNeighbor = neighbor - position;
                if (dirToNeighbor.sqrMagnitude < neighborDist * neighborDist)
                {
                    newVelocity -= dirToNeighbor.normalized * maxSpeed;
                }
            }
        }
        Vector2 dirToGoal = (goal - position).normalized;
        newVelocity += dirToGoal * maxSpeed;

        velocity = Vector2.ClampMagnitude(newVelocity, maxSpeed);
    }

    private void Move()
    {
        transform.position += new Vector3(velocity.x, 0, velocity.y) * Time.deltaTime;
    }

    public void SetGoal(Vector2 newGoal)
    {
        goal = newGoal;
    }

    // Adding missing methods
    public void UpdatePosition()
    {
        transform.position += new Vector3(velocity.x, 0,velocity.y) * Time.deltaTime;
    }

    public void Initialize(Vector2 initialPosition)
    {
        position = initialPosition;
        transform.position = new Vector3(position.x, position.y, 0);
    }
}
