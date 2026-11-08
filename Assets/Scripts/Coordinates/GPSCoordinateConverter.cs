
using System;
using UnityEngine;

public static class GPSCoordinateConverter
{
    private const double MetersPerDegreeLatitude = 111111.0;

    // GPS -> Unity: X = Este, Z = Norte.
    public static Vector3 GPSToUnity(
        double latitude,
        double longitude,
        double anchorLatitude,
        double anchorLongitude)
    {
        // Proyección equirectangular simplificada (flat Earth approximation)
        double latDiff = latitude - anchorLatitude;
        double lonDiff = longitude - anchorLongitude;

        double metersPerDegreeLongitude =
            MetersPerDegreeLatitude *
            Math.Cos(anchorLatitude * Mathf.Deg2Rad);

        double x = lonDiff * metersPerDegreeLongitude;
        double z = latDiff * MetersPerDegreeLatitude;

        return new Vector3((float)x, 0f, (float)z);
    }

    // Unity -> GPS.
    public static (double latitude, double longitude, double altitude)
        UnityToGPS(
            Vector3 unityPosition,
            double anchorLatitude,
            double anchorLongitude)
    {
        double metersPerDegreeLongitude =
            MetersPerDegreeLatitude *
            Math.Cos(anchorLatitude * Mathf.Deg2Rad);

        double latOffset =
            unityPosition.z / MetersPerDegreeLatitude;

        double lonOffset =
            unityPosition.x / metersPerDegreeLongitude;

        double latitude = anchorLatitude + latOffset;
        double longitude = anchorLongitude + lonOffset;
        double altitude = unityPosition.y;

        return (latitude, longitude, altitude);
    }
}
