using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteFolower : MonoBehaviour {
    [SerializeField] private List<Transform> waypoints = new();
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float stopTime = 0.5f;
    [SerializeField] private bool looping;
    [SerializeField] private bool reverseDirection;
    
    private int routeDirection => reverseDirection ? -1 : 1;

    private int currentWaypoint = 0;
    private float waitTimer;

    private Vector3 xz = Vector3.right + Vector3.forward;
    
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (waitTimer > 0) {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 direction = (waypoints[currentWaypoint].position - transform.position).normalized;
        direction.y = 0;
        rb.AddForce(direction * moveSpeed * Time.deltaTime, ForceMode.Acceleration);
        
        if (Vector3.Distance(Vector3.Scale(transform.position, xz), Vector3.Scale(waypoints[currentWaypoint].position, xz)) < stoppingDistance) {
            waitTimer = stopTime;
            currentWaypoint += routeDirection;
            
            if (!(currentWaypoint < 0 || currentWaypoint >= waypoints.Count)) return;
            
            if (!looping) {
                reverseDirection = !reverseDirection;
                currentWaypoint += routeDirection;
                currentWaypoint += routeDirection;
            }
            else {
                if (reverseDirection) {
                    currentWaypoint = waypoints.Count - 1;
                }
                else {
                    currentWaypoint = 0;
                }
            }
        }
    }
}
