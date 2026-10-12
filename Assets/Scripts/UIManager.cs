using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject notificationTextObj;
    private Text notificationTextComponent;
    private Coroutine currentNotificationCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (notificationTextObj != null)
        {
            notificationTextComponent = notificationTextObj.GetComponent<Text>();
            notificationTextObj.SetActive(false); // Nos aseguramos de que empiece apagado
        }
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        if (notificationTextObj == null || notificationTextComponent == null) return;

        // Si ya hay una notificación activa, detenemos esa corrutina para evitar conflictos
        if (currentNotificationCoroutine != null)
            StopCoroutine(currentNotificationCoroutine);

        currentNotificationCoroutine = StartCoroutine(ShowNotificationRoutine(message, duration));
    }

    private IEnumerator ShowNotificationRoutine(string message, float duration)
    {
        notificationTextObj.SetActive(true);
        notificationTextComponent.text = message;

        yield return new WaitForSeconds(duration);

        notificationTextComponent.text = "";
        notificationTextObj.SetActive(false);
    }
}