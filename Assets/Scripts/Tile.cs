using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_Board;
    void Start()
    {
        
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
}
