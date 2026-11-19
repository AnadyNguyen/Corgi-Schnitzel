
using System.Collections.Generic;
using UnityEngine;
public class SnakeMovement : MonoBehaviour {
    public float moveSpeed = GameParameters.SnakeMoveSpeed;
    public Transform bodyPrefab;
    public Transform startingBody;
    public Transform tail;
    public Transform treatPrefab;

    public Vector2 bottomLeft = new Vector2(-8, -4);
    public Vector2 topRight = new Vector2(8, 4);

    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;

    private Vector2Int headCell;
    private Vector2Int treatCell;

    private Vector2Int minCell;
    private Vector2Int maxCell;

    private float timer = 0f;
    private bool isDead = false;

    private Transform currentTreat;

    private List<Transform> body = new List<Transform>();
    private List<Vector2Int> bodyCells = new List<Vector2Int>();

    private List<Vector3> startPositions = new List<Vector3>();
    private List<Vector3> targetPositions = new List<Vector3>();

    private Vector3 headStart;
    private Vector3 headTarget;

    Vector2Int GetCell(Vector3 position)
    {
        Vector3Int cell = grid.WorldToCell(position);
        return new Vector2Int(cell.x, cell.y);
    }

    Vector3 GetWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(
            new Vector3Int(cell.x, cell.y, 0)
        );
    }

    void Start()
    {
        if (grid == null)
        {
            Debug.LogError("Assign the Grid in the Inspector!");
            enabled = false;
            return;
        }

        Vector2Int corner1 = GetCell(bottomLeft);
        Vector2Int corner2 = GetCell(topRight);

        minCell = Vector2Int.Min(corner1, corner2);
        maxCell = Vector2Int.Max(corner1, corner2);

        headCell = GetCell(transform.position);
        transform.position = GetWorld(headCell);

        if (startingBody != null)
        {
            body.Add(startingBody);
        }

        if (tail != null)
        {
            body.Add(tail);
        }

        foreach (Transform segment in body)
        {
            Vector2Int cell = GetCell(segment.position);
            bodyCells.Add(cell);
            segment.position = GetWorld(cell);
        }

        headStart = transform.position;
        headTarget = transform.position;

        if (treatPrefab != null)
        {
            currentTreat = Instantiate(treatPrefab);
            SpawnTreat();
        }

        MoveSnake();
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (direction != Vector2Int.down)
                nextDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) ||
                 Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (direction != Vector2Int.up)
                nextDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) ||
                 Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (direction != Vector2Int.right)
                nextDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (direction != Vector2Int.left)
                nextDirection = Vector2Int.right;
        }

        timer += Time.deltaTime;

        if (timer >= moveSpeed)
        {
            transform.position = headTarget;

            for (int i = 0; i < body.Count; i++)
            {
                body[i].position = targetPositions[i];
            }

            timer -= moveSpeed;
            MoveSnake();
        }

        float t = Mathf.Clamp01(timer / moveSpeed);

        if (Vector3.Distance(headStart, headTarget) > 1.5f)
        {
            transform.position = headTarget;
        }
        else
        {
            transform.position = Vector3.Lerp(
                headStart, headTarget, t
            );
        }

        for (int i = 0; i < body.Count; i++)
        {
            if (Vector3.Distance(
                startPositions[i],
                targetPositions[i]) > 1.5f)
            {
                body[i].position = targetPositions[i];
            }
            else
            {
                body[i].position = Vector3.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    t
                );
            }
        }
    }

    void MoveSnake()
    {
        if (isDead)
            return;

        direction = nextDirection;

        Vector2Int newHead = headCell + direction;

        if (newHead.x > maxCell.x)
            newHead.x = minCell.x;
        else if (newHead.x < minCell.x)
            newHead.x = maxCell.x;

        if (newHead.y > maxCell.y)
            newHead.y = minCell.y;
        else if (newHead.y < minCell.y)
            newHead.y = maxCell.y;

        bool collected = currentTreat != null &&
                         currentTreat.gameObject.activeSelf &&
                         newHead == treatCell;

        int collisionLimit = collected
            ? bodyCells.Count
            : bodyCells.Count - 1;

        for (int i = 0; i < collisionLimit; i++)
        {
            if (newHead == bodyCells[i])
            {
                Die();
                return;
            }
        }

        Vector2Int oldHead = headCell;
        List<Vector2Int> oldCells =
            new List<Vector2Int>(bodyCells);

        headStart = GetWorld(headCell);
        headCell = newHead;
        headTarget = GetWorld(headCell);

        for (int i = bodyCells.Count - 1; i > 0; i--)
        {
            bodyCells[i] = oldCells[i - 1];
        }

        if (bodyCells.Count > 0)
        {
            bodyCells[0] = oldHead;
        }

        if (collected && bodyPrefab != null)
        {
            GrowSegment(oldCells, oldHead);
        }

        startPositions.Clear();
        targetPositions.Clear();

        for (int i = 0; i < body.Count; i++)
        {
            startPositions.Add(body[i].position);
            targetPositions.Add(GetWorld(bodyCells[i]));
        }

        RotateSegment(transform, direction);

        for (int i = 0; i < body.Count; i++)
        {
            Vector2Int oldCell = i < oldCells.Count
                ? oldCells[i]
                : bodyCells[i];

            if (collected && bodyPrefab != null &&
                i >= body.Count - 2)
            {
                if (i == body.Count - 1)
                    oldCell = oldCells[oldCells.Count - 1];
                else
                    oldCell = bodyCells[i];
            }

            Vector2Int movement = GetMovementDirection(
                oldCell, bodyCells[i]
            );

            if (movement != Vector2Int.zero)
            {
                RotateSegment(body[i], movement);
            }
        }

        if (collected)
        {
            Debug.Log("Treat collected!");

            if (bodyPrefab != null)
                SpawnTreat();
            else
                Debug.LogWarning("Assign Body Prefab to enable growth.");
        }
    }

    Vector2Int GetMovementDirection(
        Vector2Int from, Vector2Int to)
    {
        if (from == to)
            return Vector2Int.zero;

        if (from.x == maxCell.x && to.x == minCell.x)
            return Vector2Int.right;

        if (from.x == minCell.x && to.x == maxCell.x)
            return Vector2Int.left;

        if (from.y == maxCell.y && to.y == minCell.y)
            return Vector2Int.up;

        if (from.y == minCell.y && to.y == maxCell.y)
            return Vector2Int.down;

        Vector2Int delta = to - from;

        return new Vector2Int(
            (int)Mathf.Sign(delta.x) * (delta.x != 0 ? 1 : 0),
            (int)Mathf.Sign(delta.y) * (delta.y != 0 ? 1 : 0)
        );
    }

    void GrowSegment(
        List<Vector2Int> oldCells,
        Vector2Int oldHead)
    {
        int count = oldCells.Count;

        int insertIndex = count > 0 ? count - 1 : 0;

        Vector2Int newBodyCell = count > 1
            ? oldCells[count - 2]
            : oldHead;

        Transform segment = Instantiate(
            bodyPrefab,
            GetWorld(newBodyCell),
            Quaternion.identity
        );

        body.Insert(insertIndex, segment);
        bodyCells.Insert(insertIndex, newBodyCell);

        if (count > 0)
        {
            bodyCells[bodyCells.Count - 1] =
                oldCells[count - 1];
        }
    }

    void SpawnTreat()
    {
        if (currentTreat == null)
            return;

        List<Vector2Int> available =
            new List<Vector2Int>();

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (cell == headCell)
                    continue;

                if (bodyCells.Contains(cell))
                    continue;

                available.Add(cell);
            }
        }

        if (available.Count == 0)
        {
            currentTreat.gameObject.SetActive(false);
            Debug.Log("YOU WIN!");
            return;
        }

        int random = Random.Range(0, available.Count);

        treatCell = available[random];
        currentTreat.position = GetWorld(treatCell);
        currentTreat.gameObject.SetActive(true);
    }

    void RotateSegment(
        Transform segment, Vector2Int moveDirection)
    {
        SpriteRenderer sprite =
            segment.GetComponent<SpriteRenderer>();

        if (sprite == null)
            return;

        if (moveDirection.x < 0)
        {
            segment.rotation = Quaternion.identity;
            sprite.flipX = true;
        }
        else if (moveDirection.x > 0)
        {
            segment.rotation = Quaternion.identity;
            sprite.flipX = false;
        }
        else if (moveDirection.y > 0)
        {
            segment.rotation = Quaternion.Euler(0, 0, 90);
            sprite.flipX = false;
        }
        else if (moveDirection.y < 0)
        {
            segment.rotation = Quaternion.Euler(0, 0, -90);
            sprite.flipX = false;
        }
    }

    public void Grow()
    {
        if (isDead || bodyPrefab == null)
            return;

        GrowSegment(
            new List<Vector2Int>(bodyCells),
            headCell
        );

        startPositions.Clear();
        targetPositions.Clear();

        for (int i = 0; i < body.Count; i++)
        {
            startPositions.Add(body[i].position);
            targetPositions.Add(GetWorld(bodyCells[i]));
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("GAME OVER!");
    }
}
