using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Defective.JSON;

public class ImageToJsonImporterWindow : EditorWindow
{
    private Texture2D previewTexture;
    private string imagePath;
    private string outputFileName = "pixel_image";

    [MenuItem("Tools/Pixel Image Importer")]
    public static void ShowWindow()
    {
        GetWindow<ImageToJsonImporterWindow>("Pixel Image Importer");
    }

    private Texture2D checkerTex;

    private void OnGUI()
    {
        GUILayout.Label("Importing a pixel image", EditorStyles.boldLabel);

        if (GUILayout.Button("Select an image"))
        {
            string path = EditorUtility.OpenFilePanel("Select a pixel image", "", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                imagePath = path;
                byte[] fileData = File.ReadAllBytes(imagePath);
                previewTexture = new Texture2D(2, 2);
                previewTexture.LoadImage(fileData);
                outputFileName = Path.GetFileNameWithoutExtension(path);
            }
        }

        if (previewTexture != null)
        {
            GUILayout.Label("Preview:");

            float maxWidth = position.width - 20f;
            float maxHeight = position.height - 200f;
            float aspect = (float)previewTexture.width / previewTexture.height;

            float previewWidth = maxWidth;
            float previewHeight = previewWidth / aspect;

            if (previewHeight > maxHeight)
            {
                previewHeight = maxHeight;
                previewWidth = previewHeight * aspect;
            }

            Rect textureRect = GUILayoutUtility.GetRect(previewWidth, previewHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            float offsetX = (position.width - previewWidth) / 2f;
            textureRect.x = offsetX;

            DrawCheckerBackground(textureRect);
        
            // Отрисовка текстуры без сглаживания
            FilterMode prevFilter = previewTexture.filterMode;
            previewTexture.filterMode = FilterMode.Point;

            GUI.DrawTexture(textureRect, previewTexture, ScaleMode.ScaleToFit, true);

            previewTexture.filterMode = prevFilter;
        }

        GUILayout.Space(10);
        outputFileName = EditorGUILayout.TextField("Имя файла:", outputFileName);

        GUI.enabled = previewTexture != null;
        if (GUILayout.Button("Convert and save JSON"))
        {
            ConvertAndSave(previewTexture, outputFileName);
        }
        GUI.enabled = true;
    }

    private void DrawCheckerBackground(Rect area)
    {
        if (checkerTex == null)
        {
            checkerTex = new Texture2D(2, 2);
            Color c0 = new Color(0.8f, 0.8f, 0.8f);
            Color c1 = new Color(0.6f, 0.6f, 0.6f);
            checkerTex.SetPixels(new Color[] { c0, c1, c1, c0 });
            checkerTex.filterMode = FilterMode.Point;
            checkerTex.wrapMode = TextureWrapMode.Repeat;
            checkerTex.Apply();
        }

        GUI.DrawTextureWithTexCoords(
            area,
            checkerTex,
            new Rect(0, 0, area.width / 16f, area.height / 16f)
        );
    }

    private void ConvertAndSave(Texture2D texture, string filename)
{
    int width = texture.width;
    int height = texture.height;
    Color32[] pixels = texture.GetPixels32();

    Dictionary<Color32, int> paletteMap = new Dictionary<Color32, int>(new Color32Comparer());
    List<string> palette = new List<string>();
    int[,] pixelIndices = new int[height, width];

    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        Color32 color = pixels[(height - 1 - y) * width + x]; // Flip Y
        if (!paletteMap.TryGetValue(color, out int index))
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            index = palette.Count;
            paletteMap[color] = index;
            palette.Add("#" + hex);
        }
        pixelIndices[y, x] = paletteMap[color];
    }

    JSONObject json = new JSONObject(JSONObject.Type.Object);
    json.AddField("name", filename);
    json.AddField("width", width);
    json.AddField("height", height);

    JSONObject paletteArray = new JSONObject(JSONObject.Type.Array);
    foreach (string hex in palette)
        paletteArray.Add(hex);
    json.AddField("palette", paletteArray);

    // TEMP: add an empty array to "pixels" so that it will be in print()
    json.AddField("pixels", new JSONObject(JSONObject.Type.Array));

    // is pretty print with everything else
    string prettyJson = json.Print(true);

    // assemble a nice string of "pixels":[ [1,2,3,...], [4,5,6,... ] ]
    System.Text.StringBuilder pixelsBuilder = new System.Text.StringBuilder();
    pixelsBuilder.Append("\"pixels\": [\n");

    for (int y = 0; y < height; y++)
    {
        pixelsBuilder.Append("    [");
        for (int x = 0; x < width; x++)
        {
            pixelsBuilder.Append(pixelIndices[y, x]);
            if (x < width - 1)
                pixelsBuilder.Append(",");
        }
        pixelsBuilder.Append("]");
        if (y < height - 1)
            pixelsBuilder.Append(",");
        pixelsBuilder.Append("\n");
    }

    pixelsBuilder.Append("  ]");

    // substitute "pixels" for "pixels. [] for a manually assembled block
    string result = System.Text.RegularExpressions.Regex.Replace(
        prettyJson,
        "\"pixels\"\\s*:\\s*\\[\\s*\\]",
        pixelsBuilder.ToString()
    );

    // Сохраняем
    string folderPath = "Assets/PixelImages";
    if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);

    string path = Path.Combine(folderPath, filename + ".json");
    File.WriteAllText(path, result);
    AssetDatabase.Refresh();
    EditorUtility.DisplayDialog("Success", "File saved:\n" + path, "OK");
}

    private static List<List<int>> Convert2DArray(int[,] array)
    {
        int height = array.GetLength(0);
        int width = array.GetLength(1);
        var result = new List<List<int>>();
        for (int y = 0; y < height; y++)
        {
            var row = new List<int>();
            for (int x = 0; x < width; x++)
                row.Add(array[y, x]);
            result.Add(row);
        }
        return result;
    }

    // To correctly compare Color32 as dictionary keys
    public class Color32Comparer : IEqualityComparer<Color32>
    {
        public bool Equals(Color32 a, Color32 b) =>
            a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

        public int GetHashCode(Color32 c) =>
            c.r ^ (c.g << 8) ^ (c.b << 16) ^ (c.a << 24);
    }
}
