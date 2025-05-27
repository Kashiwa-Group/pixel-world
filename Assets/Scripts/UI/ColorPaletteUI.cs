using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ColorPaletteUI : MonoBehaviour
{
    public GameObject colorButtonPrefab;
    public Transform paletteContainer;

    public Action<int, Color> OnColorSelected;
    
    private void Awake()
    {
        PaletteManager.Instance.OnPaletteReady += GeneratePaletteUI;
    }
    
    private void Start()
    {
        var painter = FindObjectOfType<ColorPainter>();
        if (painter != null)
            OnColorSelected += painter.SetColor;
    }

    /// <summary>
    /// Generates UI for the color palette. This method is automatically
    /// called when the palette is set via <see cref="PaletteManager.SetPalette"/>.
    /// </summary>
    public void GeneratePaletteUI()
    {
        var palette = PaletteManager.Instance.ColorPalette;
        if (palette == null || palette.Length == 0) return;

        paletteContainer.DestroyAllChildren();

        for (int i = 1; i < palette.Length; i++) // skip transparent
        {
            int colorIndex = i;
            Color color = palette[i];

            GameObject buttonObj = Instantiate(colorButtonPrefab, paletteContainer);

            Image img = buttonObj.GetComponent<Image>();
            if (img != null)
                img.color = color;

            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = colorIndex.ToString();
                text.color = Color.yellow;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 24;
            }

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnColorSelected?.Invoke(colorIndex, color));
        }
    }
}