using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PieceScripts : MonoBehaviour
{
    public BoardScripts board {  get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public int rorationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;
    public void Initialize(BoardScripts board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rorationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if(this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }
        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public void Update()
    {
        this.board.Clear(this);
        InputManager();
        this.board.Set(this);        
    }
    private void InputManager()
    {
        this.lockTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate(-1);
        } else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }

        if (Time.time >= this.stepTime)
        {
            Step();
        }
    }
    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        Move(Vector2Int.down);

        if(this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }
    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.Spawn();
    }
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPos = this.position;
        newPos.x += translation.x;
        newPos.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPos);

        if (valid)
        {
            this.position = newPos;
            this.lockTime = 0f;
        }
        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rorationIndex;
        this.rorationIndex = Wrap(this.rorationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction)  ;

        if (!TestWallKicks(this.rorationIndex, direction))
        {
            this.rorationIndex += originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        if (this.data.tetromino == Tetromino.O)
            return;

        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];
            float x, y;

            if (this.data.tetromino == Tetromino.I)
            {
                cell.x -= 0.5f;
                cell.y -= 0.5f;
            }

            x = cell.x * Data.RotationMatrix[0] * direction + cell.y * Data.RotationMatrix[1] * direction;
            y = cell.x * Data.RotationMatrix[2] * direction + cell.y * Data.RotationMatrix[3] * direction;

            if (this.data.tetromino == Tetromino.I)
            {
                x += 0.5f;
                y += 0.5f;
            }

            this.cells[i] = new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (wallKickIndex < 0)
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }
    private int Wrap (int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        } else
        {
            return min + (input - min) % (max - min);
        }
    }
}
