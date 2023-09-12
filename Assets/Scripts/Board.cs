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
    [SerializeField] private GameObject tileNormalPrefab;
    [SerializeField] private GameObject tileObstaclePrefab;
    
    [Tooltip("게임 플레이 할 때 움직이는 오브젝트\n위치: Assets/Prefabs/Dots/ 전부")]
    [SerializeField] private GameObject[] gamePiecePrefabs;
    #endregion
    
    #region 배열
    // [Header("Array")] : 인스펙터에서 보여질 이름
    [Header("Array")]
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePiece;
    #endregion

    private Tile m_clickedTile;
    private Tile m_targetTile;
    
    bool m_playerInputEnabled = true;
    int falseYOffset = 10;
    private float moveTime = 0.5f;
    
    [SerializeField] ParticleManager particleManager;
    
    
    public StartingObject[] startingTiles;  
    public StartingObject[] startingGamePieces;

    [Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePiece = new GamePiece[width, height];
        SetUpTiles();
        SetUpGamePieces();
        SetUpCamera();
        FillBoard(falseYOffset, moveTime);
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
    
    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int interactions = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePiece[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j);
                        interactions++;

                        if (interactions >= maxIterations) break;
                    }
                }
            }
        }
    }
    private GamePiece FillRandomAt(int i, int j, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (!IsWithInBounds(i, j)) return null;
        
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
        if (randomPiece != null)
        {
            MakeGamePiece(randomPiece, i, j, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    private void RePlaceWithRandom(List<GamePiece> gamePieces, int x, int y)
    {
        foreach (var piece in gamePieces)
        {
            ClearPieceAt(piece.xIndex, piece.yIndex);
            if (falseYOffset == 0)
            {
                FillRandomAt(piece.xIndex, piece.yIndex);
            }
            else
            {
                FillRandomAt(piece.xIndex, piece.yIndex, falseYOffset, moveTime);
            }
        }
    }

    private void MakeGamePiece(GameObject piece, int i, int j, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if(piece != null && IsWithInBounds(i, j))
        {

            piece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(piece.GetComponent<GamePiece>(), i, j);

            if (falseYOffset != 0)
            {
                piece.transform.position = new Vector3(i, j + falseYOffset, 0);
                piece.GetComponent<GamePiece>().Move(i, j, moveTime);
            }

            piece.transform.parent = transform;
        }
    }

    private bool HasMatchOnFill(int x, int y, int minLenght = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLenght);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLenght);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if(downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }
    private void SetUpGamePieces()
    {
        foreach (var sPiece in startingGamePieces)
        {
            if (sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity);
                MakeGamePiece(piece, sPiece.x, sPiece.y, falseYOffset, moveTime);
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
        foreach (var sTiles in startingTiles)
        {
            if (sTiles != null)
            {
                MakeTile(sTiles.prefab, sTiles.x, sTiles.y);
            }
        }
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allTiles[i, j] == null)
                    MakeTile(tileNormalPrefab, i, j);
            }
        }
    }

    private void MakeTile(GameObject prefab, int i, int j, int k = 0)
    {
        if(prefab == null) return;
        GameObject tile = Instantiate(prefab, new Vector3(i, j, k), Quaternion.identity, transform);
        tile.name = "tile (" + i + "," + j + ")";
        m_allTiles[i, j] = tile.GetComponent<Tile>();
        m_allTiles[i, j].Init(i, j, this);
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
        if (m_playerInputEnabled)
        {
            var clickedPiece = m_allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
            var targetPiece = m_allGamePiece[targetTile.xIndex, targetTile.yIndex];

            if (targetTile != null && clickedTile != null && targetPiece != null && clickedPiece != null)
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
                    CleanAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                }
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
            if(nextPiece == null) break;
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue)
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
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
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (var piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLenght)).ToList();
        }

        return matches;
    }
    List<GamePiece> FindAllMatchesAt(int minLenght = 3)
    {
        List<GamePiece> combineMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j, minLenght);
                combineMatches = combineMatches.Union(matches).ToList();
            }
        }
        return combineMatches;
    }
    
    void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color =
                new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        }
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
    void HighlightMatchesAt(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if(piece != null)
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
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
    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePiece[x, y];
        
        if(pieceToClear != null)
        {
            m_allGamePiece[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighlightTileOff(x, y);
    }
    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                if(particleManager != null)
                    particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex, 0);
            }
        }
    }
    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColum(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if(m_allGamePiece[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if(m_allGamePiece[column, j] != null)
                    {
                        m_allGamePiece[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePiece[column, i] = m_allGamePiece[column, j];
                        m_allGamePiece[column, i].SetCoord(column, i);

                        if(!movingPieces.Contains(m_allGamePiece[column, i]))
                            movingPieces.Add(m_allGamePiece[column, i]);
                        
                        m_allGamePiece[column, j] = null;
                        break;
                    }
                }
            }
        }

        return movingPieces;
    }
    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();
        foreach (var piece in gamePieces)
        {
            if(!columns.Contains(piece.xIndex))
                columns.Add(piece.xIndex);
        }

        return columns;
    }
    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (var column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColum(column)).ToList();
        }

        return movingPieces;
    }
    
    void CleanAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(CleanAndRefillBoardRoutine(gamePieces));
    }

    private IEnumerator CleanAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_playerInputEnabled = false;
        List<GamePiece> matches = gamePieces;
        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;
            //refill the board
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatchesAt();
            yield return new WaitForSeconds(0.5f);
        } while (matches.Count != 0);
        m_playerInputEnabled = true;
    }

    private IEnumerator RefillRoutine()
    {
        FillBoard(10, 0.5f);
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();
        //HighlightMatchesAt(gamePieces);

        yield return new WaitForSeconds(0.25f);
        bool isFinished = false;
        while (!isFinished)
        {
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();
            ClearPieceAt(gamePieces);
            BreakTileAt(gamePieces);
            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePieces);
            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.25f);
            
            matches = FindMatchesAt(movingPieces);
            if(matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];

        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            tileToBreak.BreakTile();
            if(particleManager != null)
                particleManager.BreakTileFXAt(tileToBreak.breakableValue, tileToBreak.xIndex, tileToBreak.yIndex, 0);
        }
    }

    private void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                if(piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if(m_allGamePiece[i, row] != null)
                gamePieces.Add(m_allGamePiece[i, row]);
        }

        return gamePieces;
    }
    
    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if(m_allGamePiece[column, i] != null)
                gamePieces.Add(m_allGamePiece[column, i]);
        }

        return gamePieces;
    }
    
    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if(IsWithInBounds(i, j))
                {
                    gamePieces.Add(m_allGamePiece[i, j]);
                }
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();
        foreach (var piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();
                
                Bomb bomb = piece.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            break;
                    }
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                }
            }
        }

        return allPiecesToClear;
    }

    [ContextMenu("Reset Board")]
    public void ResetBoard()
    {
        width = 7;
        height = 9;
        borderSize = 2;
    }

}
