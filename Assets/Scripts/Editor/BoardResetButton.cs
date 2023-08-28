using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Board))]
public class BoardResetButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Board board = target as Board;
        if (GUILayout.Button("Reset Board Settings"))
        {
            board.ResetBoard();
            Debug.Log("Board Setting Reset");
        }
    }
}
