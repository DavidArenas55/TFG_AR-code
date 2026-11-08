using UnityEngine;

public class WorldAnchor : MonoBehaviour
{
    [Header("Anchor GPS fijo (lat/lon de la esquina real)")]
    public double anchorLatitude;
    public double anchorLongitude;

    public Vector3 GPSToUnity(double lat, double lon)
    {
        return GPSCoordinateConverter.GPSToUnity(
            lat,
            lon,
            anchorLatitude,
            anchorLongitude);
    }

    public (double latitude, double longitude, double altitude)
        UnityToGPS(Vector3 unityPos)
    {
        return GPSCoordinateConverter.UnityToGPS(
            unityPos,
            anchorLatitude,
            anchorLongitude);
    }
}
