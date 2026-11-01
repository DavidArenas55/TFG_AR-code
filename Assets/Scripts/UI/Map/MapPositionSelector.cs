using System;
using UnityEngine;
using UnityEngine.UI;

public class MapPositionSelector : MonoBehaviour
{
    public event Action<double, double> PositionSelected;

    [Header("Map")]
    public Button placeOnMapButton;
    public GameObject mapPanel;
    public RectTransform mapRect;
    public RectTransform marker;
    public Button savePositionButton;

    [Header("GPS corners")]
    // 4 esquinas del dronLab 
    public double latTopLeft = 41.2764282, lonTopLeft = 1.9882138;
    public double latTopRight = 41.2766189, lonTopRight = 1.9890228;
    public double latBottomRight = 41.2763519, lonBottomRight = 1.9891436;
    public double latBottomLeft = 41.2761573, lonBottomLeft = 1.9883241;

    private bool hasMapPoint = false;
    private double selectedLat;
    private double selectedLon;

    private void Start()
    {
        if (placeOnMapButton != null)
            placeOnMapButton.onClick.AddListener(OpenMap);

        if (savePositionButton != null)
            savePositionButton.onClick.AddListener(SaveMapPosition);

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (marker != null)
            marker.gameObject.SetActive(false);
    }

    private void OpenMap()
    {
        if (mapPanel != null)
            mapPanel.SetActive(true);
    }

    private void CloseMap()
    {
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }

    private void Update()
    {
        if (mapPanel == null || !mapPanel.activeSelf || mapRect == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 screenPos = Input.mousePosition;

        // Solo si el clic cae DENTRO del rectángulo del dronLab
        if (!RectTransformUtility.RectangleContainsScreenPoint(mapRect, screenPos, null))
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRect, screenPos, null, out Vector2 localPoint))
            return;

        Rect r = mapRect.rect;

        float u = Mathf.Clamp01(
            Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x));

        float v = Mathf.Clamp01(
            Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y));

        // Mover el marcador al punto clickado
        if (marker != null)
        {
            marker.gameObject.SetActive(true);
            marker.anchorMin = marker.anchorMax = new Vector2(u, v);
            marker.anchoredPosition = Vector2.zero;
        }

        // Interpolación bilineal de las 4 esquinas GPS
        selectedLat = BilinearMap(
            latBottomLeft,
            latBottomRight,
            latTopLeft,
            latTopRight,
            u,
            v);

        selectedLon = BilinearMap(
            lonBottomLeft,
            lonBottomRight,
            lonTopLeft,
            lonTopRight,
            u,
            v);

        hasMapPoint = true;
    }

    private double BilinearMap(
        double bl,
        double br,
        double tl,
        double tr,
        float u,
        float v)
    {
        double bottom = bl + (br - bl) * u;
        double top = tl + (tr - tl) * u;

        return bottom + (top - bottom) * v;
    }

    private void SaveMapPosition()
    {
        if (!hasMapPoint)
        {
            Debug.LogWarning("No se ha seleccionado ninguna posición en el mapa.");
            return; // No cerramos para forzar a colocar un punto
        }

        PositionSelected?.Invoke(selectedLat, selectedLon);
        CloseMap();
    }
}