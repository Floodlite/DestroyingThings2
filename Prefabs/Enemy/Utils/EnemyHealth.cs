using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private int health = 8;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private bool isDead = false;
    public bool IsDead => isDead;

    private Reanimator reanimator;
    private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();
    private readonly List<NavMeshAgent> pausedAgents = new List<NavMeshAgent>();
    private readonly Dictionary<Collider, ColliderState> colliderStates = new Dictionary<Collider, ColliderState>();
    private readonly Dictionary<Rigidbody, RigidbodyState> rigidbodyStates = new Dictionary<Rigidbody, RigidbodyState>();

    private readonly Dictionary<MeshRenderer, MeshRendererState> meshRendererStates = new Dictionary<MeshRenderer, MeshRendererState>();
    private readonly Dictionary<SkinnedMeshRenderer, SkinnedMeshRendererState> skinnedMeshRendererStates = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRendererState>();    

    private struct ColliderState
    {
        public bool enabled;
        public bool isTrigger;

        public ColliderState(bool enabled, bool isTrigger)
        {
            this.enabled = enabled;
            this.isTrigger = isTrigger;
        }
    }

    private struct RigidbodyState
    {
        public bool isKinematic;
        public bool useGravity;
        public RigidbodyConstraints constraints;

        public RigidbodyState(bool isKinematic, bool useGravity, RigidbodyConstraints constraints)
        {
            this.isKinematic = isKinematic;
            this.useGravity = useGravity;
            this.constraints = constraints;
        }
    }

    private struct MeshRendererState
    {
        public bool enabled;

        public MeshRendererState(bool enabled)
        {
            this.enabled = enabled;
        }
    }

    private struct SkinnedMeshRendererState
    {
        public bool enabled;

        public SkinnedMeshRendererState(bool enabled)
        {
            this.enabled = enabled;
        }
    }

    private void Awake()
    {
        if (enemyRoot == null)
        {
            enemyRoot = transform.parent != null ? transform.parent : transform;
        }

        reanimator = GetComponentInParent<Reanimator>();
    }

    private void Start()
    {
        maxHealth = enemy.enemyHealth;
        if (maxHealth < 1)
        {
            maxHealth = 2;
        }
        ResetHP();
    }

    public int RetrieveHP(bool returnHP)
    {
        if (returnHP)
        {
            return health;
        }

        return maxHealth;
    }

    private void ResetHP()
    {
        health = maxHealth;
    }

    public void LoseHP(int healthLoss)
    {
        if (isDead)
        {
            return;
        }

        health -= healthLoss;
        Debug.Log("Enemy " + this.name + " " + health);
        if (health <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        health = 0;

        if (reanimator != null)
        {
            reanimator.SetDeathStatus(true);
        }

        StopMovement();
        MakeIntangible();
        StopEnemyBehaviours();
    }

    private void StopMovement()
    {
        pausedAgents.Clear();
        foreach (NavMeshAgent agent in enemyRoot.GetComponentsInChildren<NavMeshAgent>(true))
        {
            if (agent == null || !agent.enabled)
            {
                continue;
            }

            agent.isStopped = true;
            agent.ResetPath();
            pausedAgents.Add(agent);
        }

        rigidbodyStates.Clear();
        foreach (Rigidbody rb in enemyRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            if (rb == null)
            {
                continue;
            }

            rigidbodyStates[rb] = new RigidbodyState(rb.isKinematic, rb.useGravity, rb.constraints);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void MakeIntangible()
    {
        colliderStates.Clear();
        foreach (Collider collider in enemyRoot.GetComponentsInChildren<Collider>(true))
        {
            if (collider == null)
            {
                continue;
            }

            colliderStates[collider] = new ColliderState(collider.enabled, collider.isTrigger);
            if (collider.enabled)
            {
                collider.isTrigger = true;
            }
        }

        meshRendererStates.Clear();
        foreach (MeshRenderer renderer in enemyRoot.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            meshRendererStates[renderer] = new MeshRendererState(renderer.enabled);
            if (renderer.enabled)
            {
                renderer.enabled = false;
            }
        }

        skinnedMeshRendererStates.Clear();
        foreach (SkinnedMeshRenderer renderer in enemyRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            skinnedMeshRendererStates[renderer] = new SkinnedMeshRendererState(renderer.enabled);
            if (renderer.enabled)
            {
                renderer.enabled = false;
            }
        }
    }

    private void StopEnemyBehaviours()
    {
        pausedBehaviours.Clear();
        foreach (Behaviour behaviour in enemyRoot.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null || !behaviour.enabled || ShouldRemainEnabled(behaviour))
            {
                continue;
            }

            if (behaviour is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.StopAllCoroutines();
            }

            pausedBehaviours.Add(behaviour);
            behaviour.enabled = false;
        }
    }

    private bool ShouldRemainEnabled(Behaviour behaviour)
    {
        if (behaviour == this)
        {
            return true;
        }

        if (reanimator != null && behaviour == reanimator)
        {
            return true;
        }

        return false;
    }

    public void Resurrect()
    {
        if (!isDead)
        {
            return;
        }

        if (reanimator != null)
        {
            reanimator.SetDeathStatus(false);
        }

        RestoreRigidbodies();
        RestoreColliders();
        RestoreBehaviours();
        RestoreMeshRenderers();
        RestoreSkinnedMeshRenderers();

        isDead = false;
        maxHealth = Mathf.Max(1, maxHealth-=1);
        ResetHP();
    }

    private void RestoreRigidbodies()
    {
        foreach (KeyValuePair<Rigidbody, RigidbodyState> pair in rigidbodyStates)
        {
            Rigidbody rb = pair.Key;
            if (rb == null)
            {
                continue;
            }

            RigidbodyState state = pair.Value;
            rb.isKinematic = state.isKinematic;
            rb.useGravity = state.useGravity;
            rb.constraints = state.constraints;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rigidbodyStates.Clear();
    }

    private void RestoreColliders()
    {
        foreach (KeyValuePair<Collider, ColliderState> pair in colliderStates)
        {
            Collider collider = pair.Key;
            if (collider == null)
            {
                continue;
            }

            ColliderState state = pair.Value;
            collider.enabled = state.enabled;
            collider.isTrigger = state.isTrigger;
        }
        colliderStates.Clear();
    }

    private void RestoreBehaviours()
    {
        foreach (Behaviour behaviour in pausedBehaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = true;
        }
        pausedBehaviours.Clear();

        foreach (NavMeshAgent agent in pausedAgents)
        {
            if (agent == null || !agent.enabled)
            {
                continue;
            }

            agent.isStopped = false;
        }
        pausedAgents.Clear();
    }

    private void RestoreMeshRenderers()
    {
        foreach (KeyValuePair<MeshRenderer, MeshRendererState> pair in meshRendererStates)
        {
            MeshRenderer renderer = pair.Key;
            if (renderer == null)
            {
                continue;
            }

            MeshRendererState state = pair.Value;
            renderer.enabled = state.enabled;
        }
        meshRendererStates.Clear();
    }

    private void RestoreSkinnedMeshRenderers()
    {
        foreach (KeyValuePair<SkinnedMeshRenderer, SkinnedMeshRendererState> pair in skinnedMeshRendererStates)
        {
            SkinnedMeshRenderer renderer = pair.Key;
            if (renderer == null)
            {
                continue;
            }

            SkinnedMeshRendererState state = pair.Value;
            renderer.enabled = state.enabled;
        }
        skinnedMeshRendererStates.Clear();
    }


    //makes this enemy lose health when getting punched
    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

        if (!other.CompareTag("Punch"))
        {
            return;
        }

        if (player != null)
        {
            LoseHP(player.GetDamage());
        }
    }
}
