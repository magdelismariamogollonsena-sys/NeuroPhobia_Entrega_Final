using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeuroPhobia.Level45
{
    public sealed class NPLevel5Controller : MonoBehaviour
    {
        [Header("Desafío final")]
        public Transform[] orbs;
        public Transform finalZone;
        public Transform centralSpider;
        public NPNaturalSpiderPatrol[] spiderPatrols;
        public float orbCollectionRadius = 2.25f;
        public float finalZoneRadius = 3f;
        public float viewportLookRadius = 0.21f;
        public float finalLookSeconds = 15f;
        public float lostLookTolerance = 1f;

        [Header("Interfaz fija")]
        public TMP_Text titleText;
        public TMP_Text objectiveText;
        public TMP_Text timerText;
        public TMP_Text promptText;
        public Image progressFill;

        [Header("Pantalla final")]
        public GameObject completionPanel;
        public TMP_Text completionTitle;
        public TMP_Text completionSubtitle;
        public Button menuButton;
        public Button restartButton;
        public string menuSceneName = "Nivel1";

        [Header("Audio")]
        public AudioSource uiAudio;
        public AudioClip orbClip;
        public AudioClip completionClip;

        private static readonly Vector3[] ExtraSpiderPositions =
        {
            new Vector3(-20f, .06f, -4f), new Vector3(-20f, .06f, 6f),
            new Vector3(-12f, .06f, 13f), new Vector3(8f, .06f, -10f),
            new Vector3(18f, .06f, -3f), new Vector3(18f, .06f, 9f),
            new Vector3(6f, .06f, 17f), new Vector3(-8f, .06f, 18f),
            new Vector3(0f, .06f, -12f)
        };

        private Camera playerCamera;
        private Camera desktopCamera;
        private Camera xrCamera;
        private NeuroPhobiaControlModeSelector modeSelector;
        private NeuroPhobiaControlModeSelector.ControlMode lastMode;
        private Canvas hudCanvas;
        private Renderer[] targetRenderers;
        private int collectedOrbs;
        private float finalLookTime;
        private float lostLookTime;
        private float currentLookAngle = 180f;
        private bool finalExposureActivated;
        private bool completed;

        private void Start()
        {
            CacheReferences();
            CreateStaticInfestation();
            ConfigureHud(true);

            if (titleText != null) titleText.text = "NIVEL 5 · DESAFÍO FINAL";
            if (completionPanel != null) completionPanel.SetActive(false);
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(LoadMenu);
                menuButton.onClick.AddListener(LoadMenu);
            }
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartLevel);
                restartButton.onClick.AddListener(RestartLevel);
            }

            ActivateOnlyCurrentOrb();
            if (finalZone != null) finalZone.gameObject.SetActive(false);
            RefreshUI();
        }

        private void Update()
        {
            if (completed) return;
            ConfigureHud(false);
            SelectActiveCamera();
            if (playerCamera == null) return;

            if (orbs != null && collectedOrbs < orbs.Length) UpdateOrbCollection();
            else UpdateFinalExposure();
        }

        private void CacheReferences()
        {
            modeSelector = FindFirstObjectByType<NeuroPhobiaControlModeSelector>();
            lastMode = (NeuroPhobiaControlModeSelector.ControlMode)(-1);
            if (modeSelector != null)
            {
                desktopCamera = modeSelector.desktopCamera;
                if (modeSelector.xrOrigin != null)
                    xrCamera = modeSelector.xrOrigin.GetComponentInChildren<Camera>(true);
            }
            hudCanvas = titleText != null ? titleText.GetComponentInParent<Canvas>(true) : null;
            targetRenderers = centralSpider != null
                ? centralSpider.GetComponentsInChildren<Renderer>(true)
                : new Renderer[0];
            SelectActiveCamera();
        }

        private void SelectActiveCamera()
        {
            if (modeSelector == null)
            {
                if (desktopCamera != null && desktopCamera.isActiveAndEnabled) playerCamera = desktopCamera;
                else if (xrCamera != null && xrCamera.isActiveAndEnabled) playerCamera = xrCamera;
                return;
            }

            if (modeSelector.CurrentMode == NeuroPhobiaControlModeSelector.ControlMode.Computer)
                playerCamera = desktopCamera;
            else if (modeSelector.CurrentMode == NeuroPhobiaControlModeSelector.ControlMode.VirtualReality)
                playerCamera = xrCamera;
            else
                playerCamera = null;
        }

        private void ConfigureHud(bool force)
        {
            if (hudCanvas == null || modeSelector == null) return;
            NeuroPhobiaControlModeSelector.ControlMode mode = modeSelector.CurrentMode;
            if (!force && mode == lastMode) return;
            lastMode = mode;

            RectTransform rect = hudCanvas.transform as RectTransform;
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            bool vr = mode == NeuroPhobiaControlModeSelector.ControlMode.VirtualReality;
            hudCanvas.renderMode = vr ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            hudCanvas.worldCamera = vr ? xrCamera : null;
            if (vr) hudCanvas.planeDistance = 1f;
        }

        private void CreateStaticInfestation()
        {
            if (centralSpider == null) return;
            Transform template = centralSpider.Find("Visual_WolfSpider");
            if (template == null) return;

            GameObject root = new GameObject("NP_Level5_StaticInfestation");
            for (int index = 0; index < ExtraSpiderPositions.Length; index++)
            {
                GameObject spider = Instantiate(template.gameObject, ExtraSpiderPositions[index],
                    Quaternion.Euler(0f, (index * 47f) % 360f, 0f), root.transform);
                spider.name = "Visual_WolfSpider_Extra_" + (index + 1).ToString("00");
                Animator[] animators = spider.GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < animators.Length; i++) animators[i].enabled = index % 3 != 0;
            }
        }

        private void UpdateOrbCollection()
        {
            Transform orb = orbs[collectedOrbs];
            if (orb == null || HorizontalDistance(playerCamera.transform.position, orb.position) <= orbCollectionRadius)
            {
                CollectOrb();
                return;
            }

            float distance = HorizontalDistance(playerCamera.transform.position, orb.position);
            if (objectiveText != null) objectiveText.text = "Orbes recogidos: " + collectedOrbs + " / " + orbs.Length;
            if (timerText != null) timerText.text = "Siguiente orbe: " + distance.ToString("0.0") + " m";
            if (promptText != null) promptText.text = "Recorre la fábrica y acércate al orbe luminoso.";
            if (progressFill != null) progressFill.fillAmount = orbs.Length > 0 ? (float)collectedOrbs / orbs.Length : 0f;
        }

        private void CollectOrb()
        {
            if (orbs != null && collectedOrbs < orbs.Length && orbs[collectedOrbs] != null)
                orbs[collectedOrbs].gameObject.SetActive(false);
            collectedOrbs++;
            PlayClip(orbClip);

            if (orbs != null && collectedOrbs >= orbs.Length)
            {
                if (finalZone != null) finalZone.gameObject.SetActive(true);
            }
            else ActivateOnlyCurrentOrb();
            RefreshUI();
        }

        private void ActivateOnlyCurrentOrb()
        {
            if (orbs == null) return;
            for (int i = 0; i < orbs.Length; i++)
                if (orbs[i] != null) orbs[i].gameObject.SetActive(i == collectedOrbs);
        }

        private void UpdateFinalExposure()
        {
            if (finalZone == null || centralSpider == null) return;
            float distance = HorizontalDistance(playerCamera.transform.position, finalZone.position);
            if (!finalExposureActivated && distance <= finalZoneRadius) finalExposureActivated = true;

            bool looking = finalExposureActivated && IsLookingAtTarget(out currentLookAngle);
            if (looking)
            {
                finalLookTime += Time.deltaTime;
                lostLookTime = 0f;
                if (finalLookTime >= finalLookSeconds) { CompleteLevel(); return; }
            }
            else if (finalExposureActivated)
            {
                lostLookTime += Time.deltaTime;
                if (lostLookTime > lostLookTolerance) finalLookTime = 0f;
            }

            if (objectiveText != null) objectiveText.text = finalExposureActivated
                ? "Exposición final activada." : "Zona final desbloqueada.";
            if (timerText != null) timerText.text = "Mirada final: " + finalLookTime.ToString("0.0") +
                " / " + finalLookSeconds.ToString("0") + " s";
            if (progressFill != null) progressFill.fillAmount = finalLookSeconds > 0f
                ? Mathf.Clamp01(finalLookTime / finalLookSeconds) : 0f;
            if (promptText != null) promptText.text = !finalExposureActivated
                ? "Entra en la zona final.\nDistancia: " + distance.ToString("0.0") + " m"
                : "Mantén la mirada sobre la araña.\nAraña objetivo: " + centralSpider.name +
                  " · Ángulo de mirada: " + currentLookAngle.ToString("0.0") + "°";
        }

        private bool IsLookingAtTarget(out float smallestAngle)
        {
            smallestAngle = 180f;
            if (playerCamera == null || targetRenderers == null) return false;
            bool looking = false;
            Vector3 cameraPosition = playerCamera.transform.position;
            Vector3 cameraForward = playerCamera.transform.forward;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer renderer = targetRenderers[i];
                if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;
                Vector3 center = renderer.bounds.center;
                float angle = Vector3.Angle(cameraForward, center - cameraPosition);
                if (angle < smallestAngle) smallestAngle = angle;
                Vector3 viewport = playerCamera.WorldToViewportPoint(center);
                float dx = viewport.x - .5f;
                float dy = viewport.y - .5f;
                if (viewport.z > 0f && dx * dx + dy * dy <= viewportLookRadius * viewportLookRadius)
                    looking = true;
            }
            return looking;
        }

        private void RefreshUI()
        {
            int total = orbs != null ? orbs.Length : 0;
            if (objectiveText != null) objectiveText.text = "Orbes recogidos: " + collectedOrbs + " / " + total;
            if (timerText != null) timerText.text = collectedOrbs < total ? "Busca el siguiente orbe." : "Zona final desbloqueada.";
            if (progressFill != null) progressFill.fillAmount = total > 0 ? (float)collectedOrbs / total : 0f;
            if (promptText != null) promptText.text = collectedOrbs < total ? "Busca el siguiente orbe." : "Entra en la zona final.";
        }

        private void CompleteLevel()
        {
            if (completed) return;
            completed = true;
            if (spiderPatrols != null)
                foreach (NPNaturalSpiderPatrol patrol in spiderPatrols)
                    if (patrol != null) patrol.PausePatrol();
            if (completionPanel != null) completionPanel.SetActive(true);
            if (completionTitle != null) completionTitle.text = "¡Nivel completado!";
            if (completionSubtitle != null) completionSubtitle.text = "Has completado NeuroPhobia.";
            PlayClip(completionClip);
        }

        public void LoadMenu()
        {
            if (!string.IsNullOrWhiteSpace(menuSceneName)) SceneManager.LoadSceneAsync(menuSceneName);
        }

        public void RestartLevel() { SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name); }

        private void PlayClip(AudioClip clip)
        {
            if (uiAudio != null && clip != null) uiAudio.PlayOneShot(clip);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
