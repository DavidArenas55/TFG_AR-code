using UnityEngine;
using System;

public class WorldAnchor : MonoBehaviour
{
    [Header("Anchor GPS fijo (lat/lon de la esquina real)")]
    public double anchorLatitude;
    public double anchorLongitude;

    // GPS -> Unity local (metros): X=Este, Z=Norte
    public Vector3 GPSToUnity(double lat, double lon)
    {
        // Proyección equirectangular simplificada (flat Earth approximation)
        double latDiff = lat - anchorLatitude;
        double lonDiff = lon - anchorLongitude;

        double metersPerDegLat = 111111.0;
        double metersPerDegLon = 111111.0 * Math.Cos(anchorLatitude * Mathf.Deg2Rad);

        double x = lonDiff * metersPerDegLon; // Este
        double z = latDiff * metersPerDegLat; // Norte

        return new Vector3((float)x, 0f, (float)z);
    }

    // Unity -> GPS (para mostrar en Inspector / HUD)
    public (double latitude, double longitude, double altitude) UnityToGPS(Vector3 unityPos)
    {
        double metersPerDegLat = 111111.0;
        double metersPerDegLon = 111111.0 * Math.Cos(anchorLatitude * Mathf.Deg2Rad);

        double latOffset = unityPos.z / metersPerDegLat;
        double lonOffset = unityPos.x / metersPerDegLon;

        double lat = anchorLatitude + latOffset;
        double lon = anchorLongitude + lonOffset;

        double alt = unityPos.y;

        return (lat, lon, alt);
    }
}