using UnityEngine;
using UnityEngine.UI;

public class CrossController : MonoBehaviour
{
    public Color aimedColor = Color.green;

    private Image crossImage;
    private Color originalColor;

    void Start()
    {
        crossImage = GetComponent<Image>();
        if (crossImage != null)
            originalColor = crossImage.color;
    }

    void Update()
    {
        if (crossImage == null) return;

        GemProximityDestroy[] gems = FindObjectsByType<GemProximityDestroy>(FindObjectsSortMode.None);

        bool needsCrosshair = (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search);
        bool anyGemAimed = false;

        foreach (GemProximityDestroy gem in gems)
        {
            if (gem.requireShooting)
                needsCrosshair = true;

            if (gem.IsBeingAimedAt())
                anyGemAimed = true;
        }

        // Mostramos u ocultamos la cruz según el nivel/escenario actual
        crossImage.enabled = needsCrosshair;

        if (needsCrosshair)
            crossImage.color = anyGemAimed ? aimedColor : originalColor;
    }
}