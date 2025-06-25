using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardScripts : MonoBehaviour
{
    public TetrominoData[] tetrominoses;
    public Tilemap tilemap { get; private set; }
    public PieceScripts activePiece {  get; private set; }
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y /2);
            return new RectInt(position, this.boardSize);
        }
    }
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<PieceScripts>();

        for (int i = 0; i < this.tetrominoses.Length; i++)
        {
            this.tetrominoses[i].Initialize();
        }
    }

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        int randomIndex = Random.Range(0, this.tetrominoses.Length);
        TetrominoData data = this.tetrominoses[randomIndex];

        this.activePiece.Initialize(this, this.spawnPosition, data);        

        if(IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        } 
        else 
        {
            GameOver();
        }        
        
    }
    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
    }
    public void Set(PieceScripts pieces)
    {
        for (int i = 0; i < pieces.cells.Length; i++)
        {
            Vector3Int tilePosition = pieces.cells[i] + pieces.position;
            this.tilemap.SetTile(tilePosition, pieces.data.tile);
        }
    }
    public void Clear(PieceScripts pieces)
    {
        for (int i = 0; i < pieces.cells.Length; i++)
        {
            Vector3Int tilePosition = pieces.cells[i] + pieces.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(PieceScripts piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public int ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;


        while (row < bounds.yMax)
        {
            if (isLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }
        return linesCleared;
    }  

    private bool isLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col< bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position)) 
            {
                return false; 
            }
        }
        return true;
    }
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);            
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row +1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
