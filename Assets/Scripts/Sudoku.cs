using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Sudoku : MonoBehaviour
{
    //Used to determine the probability of an empty space for player to solve.
    // TODO Difficulty could also include the number of possible solutions to any given board. 
    [Range(0,1)]
    public float difficulty = 0.2f;
    
    public int seed;
    public int attemptCount = 0;
    public List<int> previousValidSeeds;
    
    private int size = 9;
    private bool isBuilt;
    private bool helpActive;
    private bool validSudokuGenerated;
    private bool searchUntilSeedFound;

    [SerializeField] private GameObject boardGameObject;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private List<ParticleSystem> particleSystems;
    
    // Sections of cells are used for interactions and references to game objects.
    public List<Cell[]> rowsOfCells = new List<Cell[]>();
    public List<Cell[]> columnsOfCells = new List<Cell[]>();
    public List<Cell[]> squaresOfCells = new List<Cell[]>();
    
    // Sections of values are used for quick lookup while deciding a valid
    // random int to add in the next cell during generation
    public  List<HashSet<int>> rowsOfValues = new List<HashSet<int>>();
    public  List<HashSet<int>> columnsOfValues = new List<HashSet<int>>();
    public  List<HashSet<int>> squaresOfValues = new List<HashSet<int>>();
    
    // Stores player specified values while game is being played.
    public  List<HashSet<int>> rowsOfPlayerValues = new List<HashSet<int>>();
    public  List<HashSet<int>> columnsOfPlayerValues = new List<HashSet<int>>();
    public  List<HashSet<int>> squaresOfPlayerValues = new List<HashSet<int>>();

    private List<Cell> cells = new List<Cell>();
    private Cell selectedCell;

    void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        DeactivateVictoryScreen();
        DeconstructBoard();
        if (!GeneratePreviousValidSudoku()) FindNewValidSudoku();
        MakeSudokuPlayable();
    }

    public void ResetGame()
    {
        DeactivateVictoryScreen();
        DeconstructBoard();
        if (!SearchSudokuSeed())
            if (!GeneratePreviousValidSudoku()) FindNewValidSudoku();
        MakeSudokuPlayable();
    }

    public void ToggleHelp()
    {
        if (helpActive)
        {
            helpActive = false;
            foreach (Cell[] arrayOfCells in rowsOfCells)
            {
                foreach (Cell cell in arrayOfCells)
                {
                    if (cell.Playable)
                    {
                        cell.SetHelpDisabled();
                    }
                }
            }
        }
        else
        {
            helpActive = true;
            foreach (Cell[] arrayOfCells in rowsOfCells)
            {
                foreach (Cell cell in arrayOfCells)
                {
                    if (cell.Playable)
                    {
                        cell.SetHelpAbled();
                    }
                }
            }
        }
    }
    
    public void Update()
    {
        if (selectedCell != null)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            {
                UpdateValue(0);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                UpdateValue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            {
                UpdateValue(2);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                UpdateValue(3);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
            {
                UpdateValue(4);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
            {
                UpdateValue(5);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
            {
                UpdateValue(6);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
            {
                UpdateValue(7);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
            {
                UpdateValue(8);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
            {
                UpdateValue(9);
            }
        }
    }

    private void UpdateValue(int newValue)
    {
        if (newValue == selectedCell.GetPlayerValue()) return;
        
        bool invalidValue = false;
        if (rowsOfPlayerValues[selectedCell.GetRow()].Contains(newValue))
        {
            foreach (Cell cell in rowsOfCells[selectedCell.GetRow()])
            {
                if (cell.GetValue() == newValue) cell.FlashWarning();
            }

            invalidValue = true;
        }

        if (columnsOfPlayerValues[selectedCell.GetColumn()].Contains(newValue))
        {
            foreach (Cell cell in columnsOfCells[selectedCell.GetColumn()])
            {
                if (cell.GetValue() == newValue) cell.FlashWarning();
            }
            invalidValue = true;
        }

        if (squaresOfPlayerValues[selectedCell.GetSquare()].Contains(newValue))
        {
            foreach (Cell cell in squaresOfCells[selectedCell.GetSquare()])
            {
                if (cell.GetValue() == newValue) cell.FlashWarning();
            }
            invalidValue = true;
        }
        if (invalidValue) return;
        
        int currentValue = selectedCell.GetPlayerValue();
        if (currentValue > 0)
        {
            rowsOfPlayerValues[selectedCell.GetRow()].Remove(currentValue);
            columnsOfPlayerValues[selectedCell.GetColumn()].Remove(currentValue);
            squaresOfPlayerValues[selectedCell.GetSquare()].Remove(currentValue);
        }

        if (newValue > 0)
        {
            rowsOfPlayerValues[selectedCell.GetRow()].Add(newValue);
            columnsOfPlayerValues[selectedCell.GetColumn()].Add(newValue);
            squaresOfPlayerValues[selectedCell.GetSquare()].Add(newValue);
        }

        if (selectedCell.SetPlayerValue(newValue)) CheckWinCondition();
        
        UpdatePossibleValues();
    }

//    private void RemovePossibleValues()
//    {
//        foreach (Cell[] arrayOfCells in rowsOfCells)
//        {
//            foreach (Cell cell in arrayOfCells)
//            {
//                if (cell.Playable)
//                {
//                    cell.RemoveAllPossibleValues();
//                }
//            }
//        }
//    }

    private void UpdatePossibleValues()
    {
        foreach (Cell[] arrayOfCells in rowsOfCells)
        {
            foreach (Cell cell in arrayOfCells)
            {
                if (cell.Playable)
                {
                    cell.RemoveAllPossibleValues();
                    List<int> possibleValues = GetPossibleValues(cell);
                    foreach (int possibleValue in possibleValues)
                    {
                        cell.AddPossibleValue(possibleValue);
                    }
                }
            }
        }
    }

    public void MakeSudokuPlayable()
    {
        if (validSudokuGenerated)
        {
            Random.InitState(seed);
            
            foreach (Cell[] arrayOfCells in rowsOfCells)
            {
                foreach (Cell cell in arrayOfCells)
                {
                    if(Random.value > 1-difficulty) cell.SetPlayable();
                    else
                    {
                        rowsOfPlayerValues[cell.GetRow()].Add(cell.GetValue());
                        columnsOfPlayerValues[cell.GetColumn()].Add(cell.GetValue());
                        squaresOfPlayerValues[cell.GetSquare()].Add(cell.GetValue());
                    }
                }
            }

            UpdatePossibleValues();
        }
        else
        {
            if(GeneratePreviousValidSudoku()) MakeSudokuPlayable();
        }
    }

    private List<int> GetPossibleValues(Cell cell)
    {
        List<int> possibleValues = new List<int>();
    
        for (int i = 0; i < size; i++)
        {
            if(!rowsOfPlayerValues[cell.GetRow()].Contains(i+1))
                if(!columnsOfPlayerValues[cell.GetColumn()].Contains(i+1))
                    if (!squaresOfPlayerValues[cell.GetSquare()].Contains(i+1))
                        possibleValues.Add(i+1);
        }

        return possibleValues;
    }

    public void BuildBoard()
    {
        if (!isBuilt)
        {
            SetupLists();
            
            for (int i = 0; i < size * size; i++)
            {
                GameObject newCellGameObject = Instantiate(cellPrefab, boardGameObject.transform);
                Cell newCell = newCellGameObject.GetComponent<Cell>();
                newCell.onClickCellDelegate += OnCellClick;
                
                newCellGameObject.name = "Cell " + (i + 1);

                int row = (int) Mathf.Floor(1f * i / size);
                int column = (int) (1f * i % size);
                int square = (int) (Mathf.Floor(1f * column / Mathf.Sqrt(size)) + Mathf.Floor(1f * row / Mathf.Sqrt(size)) * Mathf.Sqrt(size));

                if (row >= 0 && row < size) rowsOfCells[row][column]=newCell;
                if (column >= 0 && column < size) columnsOfCells[column][row]=newCell;
                if (square >= 0 && square < size) squaresOfCells[square][(int)(1f * row % Mathf.Sqrt(size) + column % Mathf.Sqrt(size) * Mathf.Sqrt(size))]=newCell;
                
                newCell.SetLocation(row, column, square);
                newCell.BuildCell();
                
                cells.Add(newCell);
            }

            
            isBuilt = true;
        }
    }

    private void SetupLists()
    {
        for (int i = 0; i < size; i++) rowsOfCells.Add(new Cell[size]);
        for (int i = 0; i < size; i++) columnsOfCells.Add(new Cell[size]);
        for (int i = 0; i < size; i++) squaresOfCells.Add(new Cell[size]);
        
        
        for (int i = 0; i < size; i++) rowsOfValues.Add(new HashSet<int>());
        for (int i = 0; i < size; i++) columnsOfValues.Add(new HashSet<int>());
        for (int i = 0; i < size; i++) squaresOfValues.Add(new HashSet<int>());
        
        
        for (int i = 0; i < size; i++) rowsOfPlayerValues.Add(new HashSet<int>());
        for (int i = 0; i < size; i++) columnsOfPlayerValues.Add(new HashSet<int>());
        for (int i = 0; i < size; i++) squaresOfPlayerValues.Add(new HashSet<int>());
    }

    private void ResetLists()
    {
        rowsOfCells = new List<Cell[]>();
        columnsOfCells = new List<Cell[]>();
        squaresOfCells = new List<Cell[]>();
        
        rowsOfValues = new List<HashSet<int>>();
        columnsOfValues = new List<HashSet<int>>();
        squaresOfValues = new List<HashSet<int>>();
        
        rowsOfPlayerValues = new List<HashSet<int>>();
        columnsOfPlayerValues = new List<HashSet<int>>();
        squaresOfPlayerValues = new List<HashSet<int>>();
    }

    public void DeconstructBoard()
    {
        ResetLists();
        
        var tempList = boardGameObject.transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            Cell newCell = child.GetComponent<Cell>();
            if (newCell != null) newCell.onClickCellDelegate -= OnCellClick;
            if(Application.isPlaying)Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        cells = new List<Cell>();
        
        isBuilt = false;
    }

    public void FindNewValidSudoku()
    {
        if (!CellsAreStillPresent()) DeconstructBoard(); // Allows rebuild of cells
        
        searchUntilSeedFound = true;
        
        Random.InitState((int)DateTime.Now.Ticks);
        seed = Random.Range(Int32.MinValue, Int32.MaxValue); // Sets New Random Seed
        
        while (!SearchSudokuSeed())
        {
            if (attemptCount > 100000)
            {
                searchUntilSeedFound = false;
                Debug.Log("Attempt Count Exceeded: " + attemptCount);
                return;
            }
            Random.InitState((int)DateTime.Now.Ticks);
            seed = Random.Range(Int32.MinValue, Int32.MaxValue); // Sets New Random Seed
        }

        if(!previousValidSeeds.Contains(seed))previousValidSeeds.Add(seed);
        
        searchUntilSeedFound = false;
        SearchSudokuSeed();
        
        Debug.Log("Sudoku Possible! attempts: " + attemptCount + ", seed: " + seed);
        attemptCount = 0;

    }

    public bool GeneratePreviousValidSudoku()
    {
        if (!CellsAreStillPresent()) DeconstructBoard(); // Allows rebuild of cells
        
        searchUntilSeedFound = false;
        
        if (previousValidSeeds.Count > 0)
        {
            Random.InitState((int)DateTime.Now.Ticks);
            seed = previousValidSeeds[Random.Range(0, previousValidSeeds.Count - 1)]; // Sets New Random Seed
            SearchSudokuSeed();
            return true;
        } else Debug.Log("No Seeds Available");
        
        attemptCount = 0;
        return false;
    }

    public void SearchOneSudokuSeed()
    {
        if (!CellsAreStillPresent()) DeconstructBoard(); // Allows rebuild of cells
        
        searchUntilSeedFound = false;
        
        Random.InitState((int)DateTime.Now.Ticks);
        seed = Random.Range(Int32.MinValue, Int32.MaxValue); // Sets New Random Seed
        if (SearchSudokuSeed())
        {
            if(!previousValidSeeds.Contains(seed))previousValidSeeds.Add(seed);
            Debug.Log("Sudoku Possible! attempts: " + attemptCount + ", seed: " + seed);
            attemptCount = 0;

        } else {
            Debug.Log("Sudoku Not Possible");
        }
    }

    private bool CellsAreStillPresent()
    {
        if (rowsOfCells.Count > 0) return true;
        
//        Debug.Log("Cells got garbage collected, reset board.");
        return false;
    }

    private bool SearchSudokuSeed()
    {
        validSudokuGenerated = false;
        bool sudokuPossible = true;
        
        BuildBoard();

        Random.InitState(seed);

        foreach (HashSet<int> set in rowsOfValues) {
            set.Clear();
            set.TrimExcess();
        }
        foreach (HashSet<int> set in columnsOfValues)
        {
            set.Clear();
            set.TrimExcess();
        }
        foreach (HashSet<int> set in squaresOfValues)
        {
            set.Clear();
            set.TrimExcess();
        }

        foreach (Cell[] arrayOfCells in rowsOfCells)
        {
            foreach (Cell cell in arrayOfCells)
            {
                if (!sudokuPossible)
                {
                    if (searchUntilSeedFound) { cell.SetValue(0); return false; }
                    
                    cell.SetValue(0, true);
                    continue;
                }

                List<int> possibleValues = new List<int>();
        
                for (int i = 0; i < size; i++)
                {
                    if(!rowsOfValues[cell.GetRow()].Contains(i+1))
                        if(!columnsOfValues[cell.GetColumn()].Contains(i+1))
                            if (!squaresOfValues[cell.GetSquare()].Contains(i+1))
                                possibleValues.Add(i+1);
                }

                if (possibleValues.Count <= 0)
                {
                    sudokuPossible = false;
                    
                    continue;
                }
                
                int newValue = possibleValues[Random.Range(0, possibleValues.Count)];
                
                rowsOfValues[cell.GetRow()].Add(newValue);
                columnsOfValues[cell.GetColumn()].Add(newValue);
                squaresOfValues[cell.GetSquare()].Add(newValue);
                
                if(searchUntilSeedFound)cell.SetValue(newValue);
                else cell.SetValue(newValue, true);
                
            }
        }

        if (!sudokuPossible)
        {
            attemptCount++;
        }
        else
        {
            validSudokuGenerated = true;
            attemptCount = 0;
        }
        
        return sudokuPossible;
    }
    
    
    public void OnCellClick(Cell cell)
    {
        if (selectedCell != null)
        {
            selectedCell.UnSelect();

            UnhighlightCells(rowsOfCells[selectedCell.GetRow()]);
            UnhighlightCells(columnsOfCells[selectedCell.GetColumn()]);
            UnhighlightCells(squaresOfCells[selectedCell.GetSquare()]);
        }

        selectedCell = cell;
        selectedCell.Select();
        
        HighlightCells(rowsOfCells[selectedCell.GetRow()]);
        HighlightCells(columnsOfCells[selectedCell.GetColumn()]);
        HighlightCells(squaresOfCells[selectedCell.GetSquare()]);
        
        Debug.Log("Cell: "+(cell.GetRow()+1)+","+(cell.GetColumn()+1)+" Value: "+cell.GetValue());
    }

    public void UnhighlightAllCells()
    {       
        if (selectedCell != null)
        {
            selectedCell.UnSelect();

            UnhighlightCells(rowsOfCells[selectedCell.GetRow()]);
            UnhighlightCells(columnsOfCells[selectedCell.GetColumn()]);
            UnhighlightCells(squaresOfCells[selectedCell.GetSquare()]);
        }

        selectedCell = null;
//        foreach (Cell[] arrayOfCells in rowsOfCells)
//        {
//            foreach (Cell cell in arrayOfCells)
//            {
//                cell.Unhighlight();
//            }
//        }
    }

    private void UnhighlightCells(Cell[] arrayOfCells)
    {
        foreach (Cell cell in arrayOfCells)
        {
            cell.Unhighlight();
        }
    }


    private void HighlightCells(Cell[] arrayOfCells)
    {
        foreach (Cell cell in arrayOfCells)
        {
            cell.Highlight();
        }
    }


    private void CheckWinCondition()
    {
        foreach (Cell[] arrayOfCells in rowsOfCells)
        {
            foreach (Cell cell in arrayOfCells)
            {
                if (cell.GetPlayerValue()<=0)
                {
                    return;
                }
            }
        }

        SudokuComplete();
    }

    public void DeactivateVictoryScreen()
    {
        victoryScreen.SetActive(false);
    }
    

    private void SudokuComplete()
    {
        victoryScreen.SetActive(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play();
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
}