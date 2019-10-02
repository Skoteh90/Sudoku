using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sudoku))]
public class SudokuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Sudoku sudokuScript = (Sudoku)target;
        
        if(GUILayout.Button("Generate Previously Found Valid Sudoku"))
        {
            sudokuScript.GeneratePreviousValidSudoku();
        }
        
        if(GUILayout.Button("Search One Sudoku Seed"))
        {
            sudokuScript.SearchOneSudokuSeed();
        }
        
        if(GUILayout.Button("Find New Valid Sudoku"))
        {
            sudokuScript.FindNewValidSudoku();
        }
        
        GUILayout.BeginHorizontal();
        
            if(GUILayout.Button("Build Board"))
            {
                sudokuScript.BuildBoard();
            }
            
            if(GUILayout.Button("Deconstruct Board"))
            {
                sudokuScript.DeconstructBoard();
            }
            
        GUILayout.EndHorizontal();
        
        DrawDefaultInspector();
    }
}