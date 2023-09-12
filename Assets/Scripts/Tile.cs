using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_Board;
    
    public TileType tileType = TileType.Normal; 
    
    SpriteRenderer m_SpriteRenderer;
    
    public int breakableValue = 0;
    [SerializeField] Sprite[] m_BreakableSprites;
    public Color nomalColor;
    
    
    void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_Board = board;
    }

    private void OnMouseDown()
    {
        if (m_Board != null)
        {
            m_Board.ClickTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (m_Board != null)
        {
            m_Board.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (m_Board != null)
        {
            m_Board.ReleaseTile();
        }
    }
    
    public void BreakTile()
    {
        if (tileType != TileType.Breakable) return;
        
        StartCoroutine(BreakTileRoutine());
    }

    private IEnumerator BreakTileRoutine()
    {
        breakableValue = Mathf.Clamp(--breakableValue, 0, m_BreakableSprites.Length - 1);
        yield return new WaitForSeconds(0.25f);
        if (m_BreakableSprites[breakableValue] != null)
        {
            m_SpriteRenderer.sprite = m_BreakableSprites[breakableValue];
        }
        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            m_SpriteRenderer.color = nomalColor;
        }

        yield return null;
    }
}
