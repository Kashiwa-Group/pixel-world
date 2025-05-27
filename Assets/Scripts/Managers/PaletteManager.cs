using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PaletteManager : MonoBehaviour
{
    public static PaletteManager Instance { get; private set; }

    public Tile[] ColorTiles { get; private set; }
    public Color[] ColorPalette { get; private set; }
    
    public event Action OnPaletteReady;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTiles(Tile[] tiles)
    {
        ColorTiles = tiles;
    }

    public void SetPalette(Color[] palette)
    {
        ColorPalette = palette;
        OnPaletteReady?.Invoke(); 
    }

    public bool IsReady => ColorTiles != null && ColorPalette != null;
}