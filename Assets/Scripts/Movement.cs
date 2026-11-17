using System.Collections.Generic;
using UnityEngine;
public class SnakeMovement : MonoBehaviour {
    public float moveSpeed = 0.15f;
    public Transform bodyPrefab;
    private Vector2 direction = Vector2.right;
    private Vector2 nextDirection = Vector2.right;
    private float timer = 0f;
    private bool isDead = false;
    private List<Transform> body = new List<Transform>();
    void Update() {
        if (isDead) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            if (direction != Vector2.down) {
                nextDirection = Vector2.up;
            }
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            if (direction != Vector2.up) {
                nextDirection = Vector2.down;
            }
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (direction != Vector2.right) {
                nextDirection = Vector2.left;
            }
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            if (direction != Vector2.left) {
                nextDirection = Vector2.right;
            }
        }
        timer += Time.deltaTime;
        if (timer >= moveSpeed) {
            timer = 0f;
            MoveSnake();
        }
    }
    void MoveSnake() {
        direction = nextDirection;
        Vector3 oldPosition = transform.position;
        Vector3 newPosition = oldPosition + (Vector3)direction;
        for (int i = 0; i < body.Count; i++) {
            if (body[i].position == newPosition) {
                if (i != body.Count - 1) {
                    Die();
                    return;
                }
            }
        }
        for (int i = body.Count - 1; i > 0; i--) {
            body[i].position = body[i - 1].position;
        }
        if (body.Count > 0) {
            body[0].position = oldPosition;
        }
        transform.position = newPosition;
    }
    public void Grow() {
        if (isDead || bodyPrefab == null) {
            return;
        }
        Vector3 spawnPosition = body.Count > 0
            ? body[body.Count - 1].position
            : transform.position - (Vector3)direction;
        Transform segment = Instantiate(bodyPrefab, spawnPosition, Quaternion.identity);
        body.Add(segment);
    }
    void Die() {
        isDead = true;
        Debug.Log("GAME OVER! Snake hit itself.");
    }
}