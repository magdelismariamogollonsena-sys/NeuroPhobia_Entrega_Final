using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level5HUD : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text taskProgressText;
    public TMP_Text completionText;
    public Button finishButton;

    private void Start()
    {
        if (taskProgressText == null) BuildHud();
        if (instructionText != null) instructionText.text = "NIVEL 5";
        if (completionText != null) completionText.gameObject.SetActive(false);
        TaskManager manager = FindFirstObjectByType<TaskManager>();
        if (manager != null) manager.RefreshHUD();
    }

    private void BuildHud()
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        GameObject panel = new GameObject("Nivel5_Tareas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(Image));
        panel.transform.SetParent(camera.transform, false);
        panel.transform.localPosition = new Vector3(-.42f, -.18f, 1.6f);
        panel.transform.localRotation = Quaternion.identity;
        panel.transform.localScale = Vector3.one * .001f;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(430, 420);
        panel.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        panel.GetComponent<Image>().color = new Color(.04f, .06f, .08f, .87f);
        taskProgressText = MakeText(panel.transform, "Tareas", new Vector2(0, 0), new Vector2(390, 245), 25);
        instructionText = MakeText(panel.transform, "Nivel", new Vector2(0, 170), new Vector2(390, 50), 32);
        completionText = MakeText(panel.transform, "Estado", new Vector2(0, -165), new Vector2(390, 60), 17);
    }

    private static TMP_Text MakeText(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        label.raycastTarget = false;
        return label;
    }

    public void SetTasks(HashSet<string> done, int total)
    {
        if (taskProgressText == null) return;
        taskProgressText.text = "Tareas:\n" + Mark(done, "TASK_LAMP", "Apagar lámpara") + "\n" +
            Mark(done, "TASK_DOOR", "Abrir puerta") + "\n" +
            Mark(done, "TASK_FRIDGE", "Abrir nevera") + "\n" +
            Mark(done, "TASK_WINDOW", "Observar ventana") + "\n\n" +
            $"Tareas completadas: {done.Count} / {total}";
    }

    private static string Mark(HashSet<string> done, string id, string name) => (done.Contains(id) ? "[✓] " : "[ ] ") + name;

    public void ShowAvoidanceLimit(int maximum)
    {
        if (completionText == null) return;
        completionText.gameObject.SetActive(true);
        completionText.text = $"Tareas completas. Se superó el límite de intentos de alejamiento ({maximum}). Puedes finalizar desde Pausa.";
    }

    // Compatibilidad con el componente doméstico anterior, que se desactiva en la escena.
    public void SetTaskProgress(int completed, int total) { }
    public void ShowCompleted(bool show) { }
    public void FinishSession() { if (SessionManager.Instance != null) SessionManager.Instance.EndSession(); }
}
