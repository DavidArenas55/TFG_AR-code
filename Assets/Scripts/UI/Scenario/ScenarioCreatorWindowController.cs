using UnityEngine;
using UnityEngine.UI;

public class ScenarioCreatorWindowController : MonoBehaviour
{
    public GameObject createScenarioWindow;
    public Button backButton;

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(false);
    }

    public void Open()
    {
        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(true);
    }
    public event System.Action Closed;

    public void Close()
    {
        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(false);

        Closed?.Invoke();
    }
}