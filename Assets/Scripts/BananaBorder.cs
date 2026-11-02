using UnityEngine;

public class BananaBorder : MonoBehaviour
{
    public GameObject bananaPrefab;

    public float spacing = 80f;        // Distance between bananas
    public float rotationSpeed = 100f; // How fast bananas spin
    public float padding = 35f;        // Distance from screen edges

    private RectTransform border;
    private RectTransform[] bananas;

    void Start()
    {
        // Get the size of the border
        border = GetComponent<RectTransform>();

        // Create the bananas
        CreateBananas();
    }

    void Update()
    {
        // Spin every banana
        foreach (RectTransform banana in bananas)
        {
            banana.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    void CreateBananas()
    {
        float width = border.rect.width - padding * 2;
        float height = border.rect.height - padding * 2;

        int columns = Mathf.Max(2, Mathf.FloorToInt(width / spacing) + 1);
        int rows = Mathf.Max(2, Mathf.FloorToInt(height / spacing) + 1);

        // Total bananas around the border
        bananas = new RectTransform[columns * 2 + rows * 2 - 4];

        int index = 0;

        // Create bananas along the top and bottom
        for (int i = 0; i < columns; i++)
        {
            float x = -width / 2 + i * width / (columns - 1);

            bananas[index++] = MakeBanana(x, height / 2);
            bananas[index++] = MakeBanana(x, -height / 2);
        }

        // Create bananas along the left and right
        for (int i = 1; i < rows - 1; i++)
        {
            float y = -height / 2 + i * height / (rows - 1);

            bananas[index++] = MakeBanana(-width / 2, y);
            bananas[index++] = MakeBanana(width / 2, y);
        }
    }

    RectTransform MakeBanana(float x, float y)
    {
        // Create a banana inside the Canvas border
        GameObject banana = Instantiate(bananaPrefab, transform);

        RectTransform rect = banana.GetComponent<RectTransform>();

        // Position banana relative to the centre
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = new Vector2(x, y);

        return rect;
    }

    void OnRectTransformDimensionsChange()
    {
        // Update the border when the screen size changes
        if (!Application.isPlaying || border == null || bananas == null)
            return;

        foreach (RectTransform banana in bananas)
        {
            if (banana != null)
                Destroy(banana.gameObject);
        }

        CreateBananas();
    }
}