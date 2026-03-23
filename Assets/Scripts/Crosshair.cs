using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Size")]
    [Tooltip("Length of each arm of the cross")]
    public float lineLength = 10f;
    [Tooltip("Thickness of each arm")]
    public float lineThickness = 2f;
    [Tooltip("Gap between centre and start of each arm")]
    public float centreGap = 4f;

    [Header("Colours")]
    public Color defaultColor = new Color(1f, 1f, 1f, 0.8f);
    public Color interactColor = new Color(1f, 0.85f, 0f, 1f);    // Yellow when near interactable
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Grey when shrunk

    [Header("Interactable Spread")]
    [Tooltip("How far the arms spread when near an interactable")]
    public float spreadAmount = 5f;
    [Tooltip("How quickly the arms spread/return")]
    public float spreadSpeed = 8f;

    [Header("World Reference")]
    [Tooltip("Assign the ProjectileSpawnPoint transform from your Player")]
    public Transform projectileSpawnPoint;

    // The four arms: Up, Down, Left, Right
    private RectTransform[] arms = new RectTransform[4];
    private Image[] armImages = new Image[4];

    // Tracks current spread offset for smooth lerp
    private float currentSpread = 0f;

    private PlayerShrink playerShrink;
    private Camera mainCamera;
    private RectTransform canvasRect;
    private Canvas canvas;

    void Start()
    {
        playerShrink = FindObjectOfType<PlayerShrink>();
        mainCamera = Camera.main;

        // Walk up to find the root Canvas for coordinate conversion
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        BuildCrosshair();
    }

    void Update()
    {
        PositionOnSpawnPoint();

        bool nearInteractable = InteractableObject.PlayerNearInteractable;
        bool isShrunk = playerShrink != null && playerShrink.IsShrunk;

        // Determine target colour
        Color targetColor;
        if (isShrunk)
            targetColor = disabledColor;
        else if (nearInteractable)
            targetColor = interactColor;
        else
            targetColor = defaultColor;

        // Smooth spread lerp
        float targetSpread = nearInteractable ? spreadAmount : 0f;
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * spreadSpeed);

        // Apply colour and spread to each arm
        UpdateArm(0,  0,  1, targetColor);  // Up
        UpdateArm(1,  0, -1, targetColor);  // Down
        UpdateArm(2, -1,  0, targetColor);  // Left
        UpdateArm(3,  1,  0, targetColor);  // Right
    }

    private void PositionOnSpawnPoint()
    {
        if (projectileSpawnPoint == null || mainCamera == null) return;

        // Convert the spawn point's world position to screen space
        Vector3 screenPos = mainCamera.WorldToScreenPoint(projectileSpawnPoint.position);

        // If the spawn point is behind the camera, hide the crosshair
        if (screenPos.z < 0)
        {
            foreach (var img in armImages) img.enabled = false;
            return;
        }
        foreach (var img in armImages) img.enabled = true;

        // Convert screen position to Canvas local position
        // Handles all Canvas scale modes correctly
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            new Vector2(screenPos.x, screenPos.y),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out localPoint
        );

        // Move the crosshair root to that canvas position
        GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    private void UpdateArm(int index, float dirX, float dirY, Color color)
    {
        float offset = centreGap + currentSpread;

        arms[index].anchoredPosition = new Vector2(
            dirX * (offset + lineLength * 0.5f),
            dirY * (offset + lineLength * 0.5f)
        );

        armImages[index].color = color;
    }

    private void BuildCrosshair()
    {
        bool[] isVertical = { true, true, false, false };

        for (int i = 0; i < 4; i++)
        {
            GameObject arm = new GameObject("CrosshairArm_" + i);
            arm.transform.SetParent(transform, false);

            RectTransform rt = arm.AddComponent<RectTransform>();
            Image img = arm.AddComponent<Image>();

            if (isVertical[i])
                rt.sizeDelta = new Vector2(lineThickness, lineLength);
            else
                rt.sizeDelta = new Vector2(lineLength, lineThickness);

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            img.color = defaultColor;

            arms[i] = rt;
            armImages[i] = img;
        }
    }
}
