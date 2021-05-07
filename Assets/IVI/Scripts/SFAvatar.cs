using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SFAvatar : MonoBehaviour
{
    //PARAMETERS
    //[SerializeField]
    private bool        DRAW_DESIRED_VELOCITY = false;
    private float       RADIUS = 0.2f;
    private const float MIN_GOAL_DIST = 0.1f;
    private const float MASS = 80;
    private const float PERCEPTION_RADIUS = 5;
    private const float ANGULAR_SPEED = 60;
    
    private CapsuleCollider collisionCapsule;
    private SphereCollider  perceptionSphere;
    private Animator        animator;

    //NAVIGATION
    private List<Vector3>   path;
    private NavMeshAgent    nma;
    private Rigidbody       rb;

    //ANIMATION
    private Vector3 velocity = Vector3.zero;
    private float   animationScale = 1.0f;
    private float   idleSpeed = 0.5f;
    private bool    applyRootMotion = true;

    //NEIGHBORS
    private HashSet<GameObject> neighbors = new HashSet<GameObject>();
    private HashSet<GameObject> obstacles = new HashSet<GameObject>();
    
    //Static Fields
    private static Dictionary<GameObject, SFAvatar> GO2Agent = new Dictionary<GameObject, SFAvatar>();

    #region Unity Functions

    void Start()
    {
        GO2Agent.Add(gameObject, this);
        gameObject.tag = "Agent";

        path = new List<Vector3>();
        nma = gameObject.AddComponent<NavMeshAgent>();
        nma.radius = RADIUS;
        nma.enabled = false;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = MASS;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        var agentMeshBounds = GetComponentInChildren<SkinnedMeshRenderer>().bounds;
        var agentHeight = agentMeshBounds.extents.y * 2;
        collisionCapsule = gameObject.AddComponent<CapsuleCollider>();
        collisionCapsule.radius = RADIUS;
        collisionCapsule.height = agentHeight;
        collisionCapsule.center = Vector3.up * agentHeight / 2f;

        perceptionSphere = gameObject.AddComponent<SphereCollider>();
        perceptionSphere.isTrigger = true;
        perceptionSphere.radius = PERCEPTION_RADIUS;

        animator = GetComponent<Animator>();
        animator.applyRootMotion = applyRootMotion;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    void Update()
    {
        UpdateAnimator();
        
        if (path.Count > 1 && GroundDistance(transform.position, path[0]) < MIN_GOAL_DIST)
        {
            path.RemoveAt(0);
        }
        else if (path.Count == 1 && GroundDistance(transform.position, path[0]) < MIN_GOAL_DIST)
        {
            path.RemoveAt(0);

            StopAnimator();

            //GetComponent<SFAvatarTarget>().InitDest();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.gameObject.tag.Equals("Agent"))
        {
            neighbors.Add(other.gameObject);
        }
        else if (other.gameObject.GetComponent<BoxCollider>() != null)
        {
            obstacles.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        if (neighbors.Contains(other.gameObject))
        {
            neighbors.Remove(other.gameObject);
        }
        if (obstacles.Contains(other.gameObject))
        {
            obstacles.Remove(other.gameObject);
        }
    }

    #endregion

    #region Public Functions

    public void StopAnimator()
    {
        path.Clear();
        animator.SetBool("Idling", true);
        animator.SetFloat("Forward", 0);
        animator.SetFloat("Strafe", 0);
    }

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        if (nma.isOnNavMesh)
            nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        nma.enabled = false;
    }
    
    #endregion

    #region Private Functions

    private void UpdateAnimator()
    {
        var accel = ComputeForce() / MASS;
        velocity += accel * Time.deltaTime;
        
        if (path.Count > 0)
        {
            var goalDir = path[0] - transform.position;
            goalDir.y = 0;
            var angle = -Vector3.SignedAngle(goalDir, transform.forward, Vector3.up);
            if (Mathf.Abs(angle) > ANGULAR_SPEED * Time.deltaTime)
            {
                angle = Mathf.Sign(angle) * ANGULAR_SPEED * Time.deltaTime;
            }
            transform.RotateAround(transform.position, Vector3.up, angle);

            var animParams = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * velocity;
            animParams *= animationScale;
            var idle = animParams.magnitude < idleSpeed && !applyRootMotion;

            animator.SetBool("Idling", idle);
            animator.SetFloat("Forward", animParams.z);
            animator.SetFloat("Strafe", animParams.x);
            if (!idle && !applyRootMotion)
            {
                transform.position += velocity * Time.deltaTime;
            }

            if (DRAW_DESIRED_VELOCITY)
            {
                Debug.DrawLine(transform.position, transform.position + velocity, Color.red);
            }
        }
    }
    
    private float GroundDistance(Vector3 a, Vector3 b)
    {
        var dir = a - b;
        dir.y = 0;

        return dir.magnitude;
    }

    private Vector3 TangentVector3(Vector3 a)
    {
        var temp = new Vector3(-a.z, 0, a.x);

        return temp;
    }

    #endregion

    #region Forces

    private Vector3 ComputeForce()
    {
        var force = CalculateGoalForce()
                  + CalculateAgentForce()
                  + CalculateWallForce();

        return force.normalized * Mathf.Min(force.magnitude, Parameters.MAX_ACCEL);
    }

    private Vector3 CalculateGoalForce()
    {
        if (path.Count == 0)
        {
            return Vector3.zero;
        }

        var temp = path[0] - transform.position;
        temp.y = 0;
        var desiredVel = temp.normalized * Parameters.DESIRED_SPEED;
        return MASS * (desiredVel - velocity) / Parameters.T;
    }

    private Vector3 CalculateAgentForce()
    {
        var agentForce = Vector3.zero;
        
        foreach (var n in neighbors)
        {
            if (GO2Agent.ContainsKey(n))
            {
                var neighbor = GO2Agent[n];

                var dir = transform.position - neighbor.transform.position;
                dir.y = 0;
                var overlap = (RADIUS + neighbor.RADIUS) - dir.magnitude;
                dir = dir.normalized;

                agentForce += Parameters.A * Mathf.Exp(overlap / Parameters.B) * dir;
                if (Vector3.Dot(dir, velocity) < 0)
                {
                    if (Vector3.Dot(dir, TangentVector3(velocity)) < 0)
                    {
                        agentForce += Parameters.TAN_A * Mathf.Exp(overlap / Parameters.TAN_B) * TangentVector3(dir);
                    }
                    else
                    {
                        agentForce += Parameters.TAN_A * Mathf.Exp(overlap / Parameters.TAN_B) * TangentVector3(-dir);
                    }
                }

                //agentForce += Parameters.K * (overlap > 0f ? 1 : 0) * dir;

                //var tangent = new Vector3(-dir.z, 0, dir.x);
                //agentForce += Parameters.KAPPA * (overlap > 0f ? overlap : 0) * Vector3.Dot(velocity - neighbor.GetVelocity(), tangent) * tangent;
            }
        }

        return agentForce;
    }

    private Vector3 CalculateWallForce()
    {
        var wallForce = Vector3.zero;

        foreach (var obstacle in obstacles)
        {
            var bounds = obstacle.GetComponent<Renderer>().bounds;
            var agentCenter = transform.position.y + 1;
            var boundVolume = (bounds.max.x - bounds.min.x) * (bounds.max.y - bounds.min.y) * (bounds.max.z - bounds.min.z);
            var valid = bounds.min.y < agentCenter && bounds.max.y > agentCenter && boundVolume > 4;
            if (!valid)
            {
                continue;
            }

            Vector3 closestPoint = obstacle.GetComponent<BoxCollider>().ClosestPoint(transform.position);

            var wallNorm = transform.position - closestPoint;
            wallNorm.y = 0;
            var overlap = RADIUS - wallNorm.magnitude;

            wallForce += Parameters.WALL_A * Mathf.Exp(overlap / Parameters.WALL_B) * wallNorm;
            //wallForce += Parameters.WALL_K * (overlap > 0f ? 1 : 0) * wallNorm;

            //var tangent = new Vector3(-wallNorm.z, 0, wallNorm.x);
            //wallForce += Parameters.WALL_KAPPA * (overlap > 0f ? overlap : 0) * Vector3.Dot(GetVelocity(), tangent) * tangent;
        }

        return wallForce;
    }

    #endregion
}
