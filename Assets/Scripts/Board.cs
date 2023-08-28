using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    #region 숫자
    [Header("Board Setting")]
    [Tooltip("가로")] [Range(1, 20)]
    [SerializeField] private int width;
    
    [Tooltip("세로")] [Range(1, 20)]
    [SerializeField] private int height;
    
    [Tooltip("보더 사이즈")] [Range(2, 10)]
    [SerializeField] private int borderSize;
    
    [SerializeField] private float swapTime = 0.5f;
    #endregion
    [Space(10)]
    #region GameObject
    [Header("GameObject")]
    [Tooltip("타일\n위치: Assets/Prefabs/Tile.prefab")]
    [SerializeField] private GameObject tilePrefab;
    
    [Tooltip("게임 플레이 할 때 움직이는 오브젝트\n위치: Assets/Prefabs/Dots/ 전부")]
    [SerializeField] private GameObject[] gamePiecePrefabs;
    #endregion
    
    #region 배열
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePiece;
    #endregion

    private Tile m_clickedTile;
    private Tile m_targetTile;

    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePiece = new GamePiece[width, height];
        SetUpTiles();
        SetUpCamera();
        FillRandom();
    }

    GameObject GetRandomGamePiece()
    {
        int randIndex = UnityEngine.Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[randIndex] == null) 
        {
            Debug.LogWarning("Board: " + randIndex + " dose not contain a valid GamePiece Prefab."); 
        }
        return gamePiecePrefabs[randIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if(gamePiece == null) 
        {
            Debug.LogWarning("Board : Invalid GamePiece.");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        if(IsWithInBounds(x, y))
            m_allGamePiece[x, y] = gamePiece;
        gamePiece.SetCoord(x, y);
    }

    private bool IsWithInBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
        
    }

    void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
                if(randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);

                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    private void SetUpCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2, (float)(height-1) / 2, -10f);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    void SetUpTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i ,j , 0), Quaternion.identity, transform);
                tile.name = "tile (" + i + "," + j + ")";
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                m_allTiles[i, j].Init(i, j, this);
            }
        }
    }
    
    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTileRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTileRoutine(Tile clickedTile, Tile targetTile)
    {
        var clickedPiece = m_allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
        var targetPiece = m_allGamePiece[targetTile.xIndex, targetTile.yIndex];

        if (targetTile != null && clickedTile != null)
        {
            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

            yield return new WaitForSeconds(swapTime);
        
            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
            {
                clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            }
            else
            {
                yield return new WaitForSeconds(swapTime);
                HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
            }
        }
    }

    bool IsNextTo(Tile start, Tile end)
    {
        //if (Vector3.Distance(start.transform.position, end.transform.position) == 1)
        //    return true;
        //else
        //    return false;
        if(Mathf.Abs(start.xIndex - end.xIndex) == 1 & start.yIndex == end.yIndex)
            return true;
        
        if(start.xIndex == end.xIndex && Mathf.Abs(start.yIndex - end.yIndex) == 1)
            return true;
        
        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithInBounds(startX, startY))
        {
            startPiece = m_allGamePiece[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }
        
        int nextX, nextY;
        int maxValue = (width > height) ? width : height;
        for (var i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;
            
            if(!IsWithInBounds(nextX, nextY))
                break;
            
            GamePiece nextPiece = m_allGamePiece[nextX, nextY];
            if (nextPiece.matchValue == startPiece.matchValue)
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
                
        }
        if (matches.Count >= minLenght)
        {
            return matches;
        }
        
        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> rightwardMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftwardMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightwardMatches == null)
        {
            rightwardMatches = new List<GamePiece>();
        }
        if (leftwardMatches == null)
        {
            leftwardMatches = new List<GamePiece>();
        }

        var combinedMatches = rightwardMatches.Union(leftwardMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLenght = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLenght);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLenght);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }
        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        
        return combinedMatches;
    }
    
    void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
    }
    
    void HighlightTileOn(int x, int y, Color color)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }
    
    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        
        if (combinedMatches.Count > 0)
        {
            foreach (var piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    
    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }
    
    [ContextMenu("Reset Board")]
    public void ResetBoard()
    {
        width = 7;
        height = 9;
        borderSize = 2;
    }

}
