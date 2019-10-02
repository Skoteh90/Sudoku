using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
//    [Range(0,9)]
//    public int Value = 1;

    private int playerValue = 0;
    private int value = 0;

    private int row = 0;
    private int column = 0;
    private int square = 0;

    private int min = 1;
    private int max = 9;

    
    private bool playable;

    public bool Playable => playable;

    private bool selected;
    private bool pointerDown;
    private bool hovered;
    private bool isBuilt;

    [SerializeField] private TextMeshProUGUI ValueText;
    [SerializeField] private GameObject PossibleValuePrefab;

    [SerializeField] private GameObject PossibleValuesGameObject;
    [SerializeField] private Image SelectImage;
    [SerializeField] private Image HighlightImage;
    [SerializeField] private Image Background;

    List<TextMeshProUGUI> PossibleValueTexts = new List<TextMeshProUGUI>();

    public Color minColor;
    public Color maxColor;

    public float warningFlashTime = 0.6f;
    public float hoverAlpha = 0.6f;
    public float mouseDownAlpha = 0.2f;
    public Color selectedColor;
    public Color hoverColor;
    public Color emptyColor;

    public delegate void CellDelegate(Cell cell);

    public CellDelegate onClickCellDelegate;
    private bool helpActive;
    private bool highlighted;

    public int GetValue()
    {
        return value;
    }
    
    public int GetPlayerValue()
    {
        return playerValue;
    }

    public int GetRow()
    {
        return row;
    }

    public int GetColumn()
    {
        return column;
    }

    public int GetSquare()
    {
        return square;
    }

    public void BuildCell()
    {
        if (!isBuilt)
        {
            for (int i = 0; i < max; i++)
            {
                GameObject newPossibleValueGameObject = Instantiate(PossibleValuePrefab, PossibleValuesGameObject.transform);
                newPossibleValueGameObject.name = "Possible Value " + (i + 1);
                TextMeshProUGUI newPossibleValueText = newPossibleValueGameObject.GetComponent<TextMeshProUGUI>();
                newPossibleValueText.text = (i + 1).ToString();
                PossibleValueTexts.Add(newPossibleValueText);
            }

            isBuilt = true;
        }
    }

    public void DeconstructCell()
    {
        var tempList = PossibleValuesGameObject.transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        PossibleValueTexts = new List<TextMeshProUGUI>();

        isBuilt = false;
    }

    public void SetValue(int setValue, bool updateVisual = false)
    {
        if (setValue < min)
        {
            ResetValue();
        }
        else if (setValue <= max)
        {
            value = setValue;
            playerValue = value;
            ValueText.text = playerValue.ToString();
        }

        if (updateVisual)
        {
            UpdateVisuals();
        }
    }

    public void ShowValue()
    {
        ValueText.enabled = true;
    }

    public void HideValue()
    {
        ValueText.enabled = false;
    }

    public void SetLocation(int setRow, int setColumn, int setSquare)
    {
        row = setRow;
        column = setColumn;
        square = setSquare;
    }

    public void SetColor()
    {
        if(playerValue==0) Background.color = emptyColor;
        else Background.color = Color.Lerp(minColor, maxColor, 1f * playerValue / max);

//        ColorUtility.TryParseHtmlString("#F7FFF7", out colorWhite);
//        ColorUtility.TryParseHtmlString("#343E3D", out colorBlack);
//        ColorUtility.TryParseHtmlString("#D62828", out colorRed);
//        ColorUtility.TryParseHtmlString("#FCD71E", out colorYellow);
//        ColorUtility.TryParseHtmlString("#45BA2E", out colorGreen);
//        ColorUtility.TryParseHtmlString("#3781C6", out colorBlue);

//        ColorUtility.TryParseHtmlString("#003049", out colorPrussianBlue);
//        ColorUtility.TryParseHtmlString("#D62828", out colorFire);
//        ColorUtility.TryParseHtmlString("#F77F00", out colorOrange);
    }

    public void ResetValue()
    {
        value = 0;
        ResetPlayerValue();
    }
    
    public void ResetPlayerValue()
    {
        playerValue = 0;
        ValueText.text = "";

        UpdateVisuals();
    }

    public void AddPossibleValue(int possibleValue)
    {
        PossibleValueTexts[possibleValue-1].enabled = true;
    }

    public void RemovePossibleValue(int possibleValue)
    {
        PossibleValueTexts[possibleValue-1].enabled = false;
    }

    public void HideAllPossibleValues()
    {
        PossibleValuesGameObject.SetActive(false);
    }

    public void ShowAllPossibleValues()
    {
        if(helpActive)
        PossibleValuesGameObject.SetActive(true);
    }

    public void AddAllPossibleValues()
    {
        foreach (TextMeshProUGUI textMeshProUgui in PossibleValueTexts)
        {
            textMeshProUgui.enabled = true;
        }
    }

    public void RemoveAllPossibleValues()
    {
//        bool hidePossibleValues = false;
//        if (!PossibleValuesGameObject.activeSelf)
//        {
//            ShowAllPossibleValues();
//            hidePossibleValues = true;
//        }
        
        foreach (TextMeshProUGUI textMeshProUgui in PossibleValueTexts)
        {
            textMeshProUgui.enabled = false;
        }
        
//        if (hidePossibleValues)
//        {
//            HideAllPossibleValues();
//        }
        
    }

    public void Highlight()
    {
        if (!selected)
        {
            highlighted = true;
            HighlightImage.enabled = true;

            if (playable)
            {
                Color highlightColor = Background.color;
                highlightColor.a = hoverAlpha;

                Background.color = highlightColor;
            }
        }
    }

    public void Unhighlight()
    {
        if (!selected)
        {
            highlighted = false;
            HighlightImage.enabled = false;
            SetColor();
        }
    }

    public void Select()
    {
        selected = true;
        SelectImage.enabled = true;
        Background.color = selectedColor;
    }

    public void UnSelect()
    {
        selected = false;
        SelectImage.enabled = false;
        SetColor();
    }

    public void SetPlayable()
    {
        playable = true;
        ResetPlayerValue();
    }

    public bool SetPlayerValue(int setValue)
    {
        if (setValue == 0) ResetPlayerValue();
        else
        {
            playerValue = setValue;
            ValueText.text = playerValue.ToString();
            UpdateVisuals();
        }

        return setValue==value;
    }

    private void UpdateVisuals()
    {
        if (playerValue < min || playerValue > max) ShowAllPossibleValues();
        else HideAllPossibleValues();

        if(!selected)SetColor();

        HideValue();
        ShowValue(); // Attempt to refresh sceneview
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!playable) return;

        pointerDown = false;
        if (!selected)
        if (hovered)
        {
            if (onClickCellDelegate != null) onClickCellDelegate(this);
        }
        else
        {
            SetColor();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!playable) return;

        pointerDown = true;
        if (!selected)
        {
//            Color mouseDownColor = Color.Lerp(minColor, maxColor, 1f * playerValue / max);
            Color mouseDownColor = hoverColor;
            mouseDownColor.a = mouseDownAlpha;
            Background.color = mouseDownColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playable) return;

        hovered = true;
        if(!pointerDown)
            if (!selected&&!highlighted)
            {
                SelectImage.enabled = true;
//                Color newHoverColor;
//                if(playerValue==0)newHoverColor=hoverColor;
//                    else newHoverColor = Color.Lerp(minColor, maxColor, 1f * playerValue / max);
//                newHoverColor.a = hoverAlpha;
//                Background.color = newHoverColor;
            }
//        if(!selected)Background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!playable||highlighted) return;
        
        hovered = false;
        if(!pointerDown)
            if (!selected)
            {
                SelectImage.enabled = false;
//                SetColor();
            }
    }

    public void FlashWarning()
    {
        StartCoroutine(FlashWarningCoroutine());
    }

    IEnumerator FlashWarningCoroutine()
    {
        bool resetPlayable = false;
        if (playable)
        {
            playable = false;
            resetPlayable = true;
        }
        
        Background.color = maxColor;
        yield return new WaitForSeconds(warningFlashTime/4);
        Background.color = minColor;
        yield return new WaitForSeconds(warningFlashTime/4);
        Background.color = maxColor;
        yield return new WaitForSeconds(warningFlashTime/4);
        Background.color = minColor;
        yield return new WaitForSeconds(warningFlashTime/4);
        SetColor();
        
        if (resetPlayable)
        {
            playable = true;
        }
    }

    public void SetHelpDisabled()
    {
        helpActive = false;
        HideAllPossibleValues();
    }

    public void SetHelpAbled()
    {
        helpActive = true;
        ShowAllPossibleValues();
    }
}
