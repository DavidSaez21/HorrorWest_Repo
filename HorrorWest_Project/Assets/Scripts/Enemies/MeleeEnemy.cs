using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeleeEnemy : EnemyBase
{
    // ── Estados ──────────────────────────────────────────────────────────────
    private enum State { Idle, Pursuing, Attacking }
    private State currentState = State.Idle;

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("LOS Settings")]
    [SerializeField] private LayerMask obstacleLayer;           // Layers que bloquean la visión (paredes, props)
    [SerializeField] private float losCheckInterval = 0.15f;    // Cada cuántos segundos chequea LOS

    [Header("Corner Rounding")]
    [SerializeField] private float nodeSpacing = 0.8f;          // Separación entre nodos alrededor del player
    [SerializeField] private int nodeGridSize = 3;              // Radio de la grid (3 = grid 7x7)
    [SerializeField] private float nodeSearchInterval = 0.2f;   // Cada cuántos segundos busca nuevo nodo

    [Header("Movement")]
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float separationStrength = 2f;
    [SerializeField] private float stoppingDistance = 0.1f;     // Distancia mínima al nodo para considerarlo alcanzado

    // ── Runtime ───────────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Vector2 currentDestination;
    private bool hasLOS = false;
    private float losTimer = 0f;
    private float nodeSearchTimer = 0f;

    // ── Init ──────────────────────────────────────────────────────────────────
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        currentDestination = transform.position;
        currentState = State.Pursuing;      // Siempre persigue desde el inicio
    }

    // ── Loop ──────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (isDead || player == null) return;

        // Timers
        losTimer += Time.deltaTime;
        nodeSearchTimer += Time.deltaTime;

        if (losTimer >= losCheckInterval)
        {
            hasLOS = CheckLOS(transform.position, player.position);
            losTimer = 0f;
        }

        UpdateState();
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;
        MoveTowardsDestination();
    }

    // ── FSM ───────────────────────────────────────────────────────────────────
    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Pursuing:
                UpdatePursuing();
                break;

            case State.Attacking:
                UpdateAttacking();
                break;
        }
    }

    private void UpdatePursuing()
    {
        float dist = DistanceToPlayer();

        // Si está en rango de ataque y tiene LOS -> ataca
        if (dist <= data.attackRange && hasLOS)
        {
            currentState = State.Attacking;
            return;
        }

        // Si tiene LOS directa -> va directo al player
        if (hasLOS)
        {
            currentDestination = player.position;
        }
        else
        {
            // Sin LOS -> busca nodo intermedio para rodear la esquina
            if (nodeSearchTimer >= nodeSearchInterval)
            {
                Vector2 cornerNode = FindCornerNode();
                if (cornerNode != Vector2.zero)
                    currentDestination = cornerNode;

                nodeSearchTimer = 0f;
            }
        }
    }

    private void UpdateAttacking()
    {
        // Si el player se aleja o pierde LOS -> vuelve a perseguir
        if (DistanceToPlayer() > data.attackRange || !hasLOS)
        {
            currentState = State.Pursuing;
            return;
        }

        TryAttack();
    }

    // ── Movimiento ────────────────────────────────────────────────────────────
    private void MoveTowardsDestination()
    {
        if (currentState == State.Attacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)currentDestination - (Vector2)transform.position);

        if (dir.magnitude <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 moveDir = dir.normalized + GetSeparationForce();
        rb.linearVelocity = moveDir.normalized * data.moveSpeed;
    }

    // ── LOS ───────────────────────────────────────────────────────────────────
    private bool CheckLOS(Vector2 origin, Vector2 target)
    {
        Vector2 dir = target - origin;
        float dist = dir.magnitude;

        RaycastHit2D hit = Physics2D.CircleCast(origin, 0.1f, dir.normalized, dist, obstacleLayer);
        return hit.collider == null;
    }

    // ── Corner Rounding ───────────────────────────────────────────────────────
    // Busca el nodo más cercano al enemigo que tenga LOS tanto con el enemigo como con el player
    private Vector2 FindCornerNode()
    {
        Vector2 bestNode = Vector2.zero;
        float bestDistance = Mathf.Infinity;

        for (int x = -nodeGridSize; x <= nodeGridSize; x++)
        {
            for (int y = -nodeGridSize; y <= nodeGridSize; y++)
            {
                Vector2 node = (Vector2)player.position + new Vector2(x * nodeSpacing, y * nodeSpacing);

                // El nodo debe tener LOS con el player Y con el enemigo
                bool nodeSeesPlayer = CheckLOS(node, player.position);
                bool enemySeesNode = CheckLOS(transform.position, node);

                if (nodeSeesPlayer && enemySeesNode)
                {
                    float dist = Vector2.Distance(transform.position, node);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestNode = node;
                    }
                }
            }
        }

        return bestNode;
    }

    // ── Ataque ────────────────────────────────────────────────────────────────
    private void TryAttack()
    {
        if (!CanAttack()) return;
        ResetAttackCooldown();

        if (player.TryGetComponent(out PlayerHealth health))
            health.TakeDamage(data.damage);
    }

    // ── Separación ────────────────────────────────────────────────────────────
    private Vector2 GetSeparationForce()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);

        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent(out EnemyBase _)) continue;

            Vector2 pushDir = (Vector2)transform.position - (Vector2)col.transform.position;
            float distance = pushDir.magnitude;

            if (distance > 0)
                separation += pushDir.normalized / distance;
        }

        return separation * separationStrength;
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    protected override void OnDamageReceived(float damage) { }
    protected override void OnDeath() { }

    // ── Debug ─────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        // Radio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);

        // Destino actual
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(currentDestination, 0.15f);

        // Línea al destino
        Gizmos.color = hasLOS ? Color.green : Color.red;
        if (player != null)
            Gizmos.DrawLine(transform.position, player.position);
    }
}