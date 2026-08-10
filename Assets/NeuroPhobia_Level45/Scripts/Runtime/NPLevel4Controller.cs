using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeuroPhobia.Level45
{
    public sealed class NPLevel4Controller : MonoBehaviour
    {
        [Header("Exposición")]
        public Transform spiderTarget;
        public NPNaturalSpiderPatrol spiderPatrol;
        public Transform[] exposurePoints;
        public float activationRadius = 1.45f;
        public float viewportLookRadius = 0.20f;
        public float lostLookTolerance = 0.55f;
        public float secondsPerPoint = 10f;

        [Header("Interfaz fija")]
        public TMP_Text titleText;
        public TMP_Text objectiveText;
        public TMP_Text timerText;
        public TMP_Text bestText;
        public TMP_Text promptText;
        public Image progressFill;

        [Header("Final")]
        public GameObject completionPanel;
        public TMP_Text completionTitle;
        public TMP_Text completionSubtitle;
        public TMP_Text countdownText;
        public string nextSceneName = "Nivel5";
        public float transitionDelay = 5f;

        [Header("Audio")]
        public AudioSource uiAudio;
        public AudioClip checkpointClip;
        public AudioClip completionClip;

        private Camera playerCamera;
        private int currentPoint;
        private float currentLookTime;
        private float bestAttempt;
        private float lostLookTime;
        private bool completing;
        private float completionRemaining;

        private void Start()
        {
            playerCamera = NPLevel45Camera.Find();

            if (titleText != null)
            {
                titleText.text = "NIVEL 4 · EXPOSICIÓN CERCANA";
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }

            ActivateOnlyCurrentPoint();
            RefreshUI();
        }

        private void Update()
        {
            if (completing)
            {
                UpdateCompletion();
                return;
            }

            if (playerCamera == null ||
                !playerCamera.gameObject.activeInHierarchy)
            {
                playerCamera = NPLevel45Camera.Find();
            }

            if (exposurePoints == null ||
                exposurePoints.Length == 0 ||
                currentPoint >= exposurePoints.Length)
            {
                BeginCompletion();
                return;
            }

            Transform point = exposurePoints[currentPoint];

            if (point == null || playerCamera == null)
            {
                RefreshUI();
                return;
            }

            float distance = Vector3.Distance(
                playerCamera.transform.position,
                point.position);

            bool insidePoint = distance <= activationRadius;
            bool lookingAtSpider = insidePoint &&
                NPLevel45Camera.IsLookingAt(
                    playerCamera,
                    spiderTarget,
                    viewportLookRadius);

            if (lookingAtSpider)
            {
                currentLookTime += Time.deltaTime;
                lostLookTime = 0f;
                bestAttempt = Mathf.Max(bestAttempt, currentLookTime);

                if (currentLookTime >= secondsPerPoint)
                {
                    CompleteCurrentPoint();
                }
            }
            else
            {
                lostLookTime += Time.deltaTime;

                if (lostLookTime > lostLookTolerance)
                {
                    currentLookTime = 0f;
                }
            }

            RefreshUI(distance, insidePoint, lookingAtSpider);
        }

        private void CompleteCurrentPoint()
        {
            if (exposurePoints != null &&
                currentPoint < exposurePoints.Length &&
                exposurePoints[currentPoint] != null)
            {
                exposurePoints[currentPoint].gameObject.SetActive(false);
            }

            PlayClip(checkpointClip);

            currentPoint++;
            currentLookTime = 0f;
            lostLookTime = 0f;
            bestAttempt = 0f;

            if (currentPoint >= exposurePoints.Length)
            {
                BeginCompletion();
                return;
            }

            ActivateOnlyCurrentPoint();
            RefreshUI();
        }

        private void ActivateOnlyCurrentPoint()
        {
            if (exposurePoints == null)
            {
                return;
            }

            for (int index = 0; index < exposurePoints.Length; index++)
            {
                if (exposurePoints[index] != null)
                {
                    exposurePoints[index].gameObject.SetActive(
                        index == currentPoint);
                }
            }
        }

        private void RefreshUI(
            float distance = float.PositiveInfinity,
            bool insidePoint = false,
            bool lookingAtSpider = false)
        {
            int total = exposurePoints != null
                ? exposurePoints.Length
                : 0;

            int displayedPoint = Mathf.Clamp(
                currentPoint + 1,
                1,
                Mathf.Max(total, 1));

            if (objectiveText != null)
            {
                objectiveText.text =
                    "Punto de exposición: " +
                    displayedPoint + " / " +
                    Mathf.Max(total, 1);
            }

            if (timerText != null)
            {
                timerText.text =
                    "Mirada continua: " +
                    currentLookTime.ToString("0.0") +
                    " / " +
                    secondsPerPoint.ToString("0") +
                    " s";
            }

            if (bestText != null)
            {
                bestText.text =
                    "Mejor intento: " +
                    bestAttempt.ToString("0.0") +
                    " s";
            }

            if (progressFill != null)
            {
                progressFill.fillAmount =
                    secondsPerPoint > 0f
                        ? Mathf.Clamp01(
                            currentLookTime / secondsPerPoint)
                        : 0f;
            }

            if (promptText != null)
            {
                if (!insidePoint)
                {
                    promptText.text =
                        "Acércate al orbe luminoso\n" +
                        (float.IsInfinity(distance)
                            ? string.Empty
                            : "Distancia: " +
                              distance.ToString("0.0") +
                              " m");
                }
                else if (!lookingAtSpider)
                {
                    promptText.text =
                        "Mira directamente a la araña";
                }
                else
                {
                    promptText.text =
                        "Mantén la mirada";
                }
            }
        }

        private void BeginCompletion()
        {
            if (completing)
            {
                return;
            }

            completing = true;
            completionRemaining = transitionDelay;

            if (spiderPatrol != null)
            {
                spiderPatrol.PausePatrol();
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }

            if (completionTitle != null)
            {
                completionTitle.text = "¡Felicitaciones!";
            }

            if (completionSubtitle != null)
            {
                completionSubtitle.text =
                    "Completaste el Nivel 4";
            }

            PlayClip(completionClip);
            UpdateCompletionText();
        }

        private void UpdateCompletion()
        {
            completionRemaining -= Time.unscaledDeltaTime;
            UpdateCompletionText();

            if (completionRemaining <= 0f)
            {
                completing = false;

                if (!string.IsNullOrWhiteSpace(nextSceneName))
                {
                    SceneManager.LoadSceneAsync(nextSceneName);
                }
            }
        }

        private void UpdateCompletionText()
        {
            if (countdownText != null)
            {
                countdownText.text =
                    "El Nivel 5 comenzará en " +
                    Mathf.CeilToInt(
                        Mathf.Max(0f, completionRemaining)) +
                    " segundos";
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (uiAudio != null && clip != null)
            {
                uiAudio.PlayOneShot(clip);
            }
        }
    }
}
