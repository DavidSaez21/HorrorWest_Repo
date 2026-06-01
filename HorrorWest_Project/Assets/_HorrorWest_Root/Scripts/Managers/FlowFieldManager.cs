using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and maintains a Dijkstra-based flow field centered on the player.
/// 
/// How it works:
///   Every time the player moves more than _recalcThreshold units, we run a
///   Dijkstra flood-fill from the player's cell outward. Each cell stores the
///   best direction to move in order to reach the player while avoiding walls.
///   Enemies read their cell's direction from GetDirection().
///
/// Cost: O(W×H) per recalc. With a 0.5-unit grid and maps up to ~50×50 units
/// that's 10,000 cells — completes in <1ms on any modern CPU.
///
/// Setup:
///   1. Add this component to an empty GameObject in the scene.
///   2. Set _cellSize (0.5 is a good default for most sprite sizes).
///   3. Set _obstacleLayerMask to your wall/obstacle layers.
///   4. Hit Play — the field recalculates automatically as the player moves.
///      You can also see the grid in the Scene view (OnDrawGizmos).
/// </summary>
public class FlowFieldManager : MonoBehaviour
{
    public static FlowFieldManager Instance { get; private set; }

    [Header("Grid")]
    [Tooltip("Size of each cell in world units. Should be <= smallest enemy sprite width.")]
    [Range(0.25f, 2f)]
    [SerializeField] private float _cellSize = 0.5f;

    [Tooltip("How many cells to generate around the player. 60 cells * 0.5 = 30 unit radius.")]
    [Range(20, 120)]
    [SerializeField] private int _gridRadius = 60;

    [Header("Obstacles")]
    [SerializeField] private LayerMask _obstacleLayerMask;

    [Tooltip("Radius of the overlap check per cell. Slightly smaller than cellSize/2.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _obstacleCheckRadius = 0.2f;

    [Header("Recalculation")]
    [Tooltip("Recalculate when player moves this many units. Keep >= cellSize.")]
    [Range(0.25f, 3f)]
    [SerializeField] private float _recalcThreshold = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;

    // ── Grid data ──────────────────────────────────────────────────────────────

    // Bottom-left world position of the grid
    private Vector2 _gridOrigin;

    // Grid dimensions (cells)
    private int _gridWidth;
    private int _gridHeight;

    // Per-cell data
    private Vector2[] _directions;   // best move direction (zero = wall or unreachable)
    private bool[] _isWall;       // true = impassable
    private int[] _cost;         // Dijkstra cost (int.MaxValue = unvisited)

    // ── State ──────────────────────────────────────────────────────────────────

    private Transform _player;
    private Vector2 _lastRecalcPosition;
    private bool _fieldReady;

    // Reusable queue for Dijkstra
    private readonly Queue<Vector2Int> _queue = new Queue<Vector2Int>(512);

    // 8-directional neighbours (including diagonals for smooth movement)
    private static readonly Vector2Int[] _neighbours = {
        new Vector2Int( 1,  0), new Vector2Int(-1,  0),
        new Vector2Int( 0,  1), new Vector2Int( 0, -1),
        new Vector2Int( 1,  1), new Vector2Int(-1,  1),
        new Vector2Int( 1, -1), new Vector2Int(-1, -1),
    };

    // Diagonal moves cost more (√2 ≈ 1.414, scaled to int: 14 vs 10)
    private static readonly int[] _neighbourCosts = { 10, 10, 10, 10, 14, 14, 14, 14 };

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            AllocateGrid();
            Recalculate();
        }
    }

    private void Update()
    {
        if (_player == null) return;

        // Recalc only when the player has moved enough — very cheap check.
        if (Vector2.Distance(_player.position, _lastRecalcPosition) >= _recalcThreshold)
            Recalculate();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the flow field direction at <paramref name="worldPos"/>.
    /// Falls back to the direct direction toward the player if the field isn't ready
    /// or the position is outside the grid.
    /// </summary>
    public Vector2 GetDirection(Vector2 worldPos)
    {
        if (!_fieldReady || _player == null)
            return (_player != null)
                ? ((Vector2)_player.position - worldPos).normalized
                : Vector2.zero;

        Vector2Int cell = WorldToCell(worldPos);

        if (!InBounds(cell))
            return ((Vector2)_player.position - worldPos).normalized; // outside grid

        Vector2 dir = _directions[CellIndex(cell)];

        // If cell has no direction (wall or isolated), aim directly at player
        return dir == Vector2.zero
            ? ((Vector2)_player.position - worldPos).normalized
            : dir;
    }

    // ── Grid helpers ───────────────────────────────────────────────────────────

    private void AllocateGrid()
    {
        int diameter = _gridRadius * 2 + 1;
        _gridWidth = diameter;
        _gridHeight = diameter;

        int total = _gridWidth * _gridHeight;
        _directions = new Vector2[total];
        _isWall = new bool[total];
        _cost = new int[total];
    }

    private void Recalculate()
    {
        if (_player == null) return;

        _lastRecalcPosition = _player.position;

        // Re-centre the grid on the player
        _gridOrigin = (Vector2)_player.position
            - new Vector2(_gridRadius * _cellSize, _gridRadius * _cellSize);

        int total = _gridWidth * _gridHeight;

        // ── 1. Scan walls ─────────────────────────────────────────────────────
        for (int i = 0; i < total; i++)
        {
            Vector2 worldPos = CellIndexToWorld(i);
            _isWall[i] = Physics2D.OverlapCircle(worldPos, _obstacleCheckRadius, _obstacleLayerMask) != null;
            _cost[i] = int.MaxValue;
            _directions[i] = Vector2.zero;
        }

        // ── 2. Dijkstra flood from player cell ────────────────────────────────
        Vector2Int playerCell = WorldToCell(_player.position);
        if (!InBounds(playerCell)) return; // player outside grid — skip

        _queue.Clear();
        int playerIdx = CellIndex(playerCell);
        _cost[playerIdx] = 0;
        _queue.Enqueue(playerCell);

        while (_queue.Count > 0)
        {
            Vector2Int current = _queue.Dequeue();
            int currentCost = _cost[CellIndex(current)];

            for (int n = 0; n < _neighbours.Length; n++)
            {
                Vector2Int neighbour = current + _neighbours[n];
                if (!InBounds(neighbour)) continue;

                int nIdx = CellIndex(neighbour);
                if (_isWall[nIdx]) continue;

                int newCost = currentCost + _neighbourCosts[n];
                if (newCost >= _cost[nIdx]) continue;

                _cost[nIdx] = newCost;
                _queue.Enqueue(neighbour);
            }
        }

        // ── 3. Derive flow directions from cost gradient ───────────────────────
        // Each cell points toward whichever neighbour has the lowest cost.
        for (int i = 0; i < total; i++)
        {
            if (_isWall[i] || _cost[i] == int.MaxValue) continue;

            Vector2Int cell = CellIndexToCell(i);
            int bestCost = _cost[i];
            Vector2Int bestNeighbour = cell;

            for (int n = 0; n < _neighbours.Length; n++)
            {
                Vector2Int neighbour = cell + _neighbours[n];
                if (!InBounds(neighbour)) continue;

                int nIdx = CellIndex(neighbour);
                if (_isWall[nIdx]) continue;

                if (_cost[nIdx] < bestCost)
                {
                    bestCost = _cost[nIdx];
                    bestNeighbour = neighbour;
                }
            }

            if (bestNeighbour != cell)
                _directions[i] = ((Vector2)(bestNeighbour - cell)).normalized;
        }

        _fieldReady = true;
    }

    // ── Coordinate utilities ───────────────────────────────────────────────────

    private Vector2Int WorldToCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - _gridOrigin.x) / _cellSize),
            Mathf.FloorToInt((worldPos.y - _gridOrigin.y) / _cellSize)
        );
    }

    private Vector2 CellToWorld(Vector2Int cell)
        => _gridOrigin + new Vector2(cell.x + 0.5f, cell.y + 0.5f) * _cellSize;

    private Vector2 CellIndexToWorld(int idx)
        => CellToWorld(CellIndexToCell(idx));

    private Vector2Int CellIndexToCell(int idx)
        => new Vector2Int(idx % _gridWidth, idx / _gridWidth);

    private int CellIndex(Vector2Int cell)
        => cell.y * _gridWidth + cell.x;

    private bool InBounds(Vector2Int cell)
        => cell.x >= 0 && cell.x < _gridWidth && cell.y >= 0 && cell.y < _gridHeight;

    // ── Debug ──────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || !_fieldReady) return;

        for (int i = 0; i < _gridWidth * _gridHeight; i++)
        {
            Vector2 worldPos = CellIndexToWorld(i);

            if (_isWall[i])
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Gizmos.DrawCube(worldPos, Vector3.one * _cellSize * 0.9f);
            }
            else if (_directions[i] != Vector2.zero)
            {
                // Color by cost: green = close to player, blue = far
                float t = Mathf.Clamp01(_cost[i] / 200f);
                Gizmos.color = Color.Lerp(Color.green, Color.blue, t) * new Color(1, 1, 1, 0.4f);
                Gizmos.DrawRay(worldPos, (Vector3)_directions[i] * _cellSize * 0.4f);
            }
        }
    }
}