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
        public float orbCollectionRadius = 1.35f;
        public float finalZoneRadius = 1.55f;
        public float viewportLookRadius = 0.21f;
        public float finalLookSeconds = 15f;
        public float lostLookTolerance = 0.65f;

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

        private Camera playerCamera;
        private int collectedOrbs;
        private float finalLookTime;
        private float lostLookTime;
        private bool completed;

        private void Start()
        {
            playerCamera = NPLevel45Camera.Find();

            if (titleText != null)
            {
                titleText.text = "NIVEL 5 · DESAFÍO FINAL";
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }

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

            if (finalZone != null)
            {
                finalZone.gameObject.SetActive(false);
            }

            RefreshUI();
        }

        private void Update()
        {
            if (completed)
            {
                return;
            }

            if (playerCamera == null ||
                !playerCamera.gameObject.activeInHierarchy)
            {
                playerCamera = NPLevel45Camera.Find();
            }

            if (playerCamera == null)
            {
                return;
            }

            if (orbs != null && collectedOrbs < orbs.Length)
            {
                UpdateOrbCollection();
            }
            else
            {
                UpdateFinalExposure();
            }
        }

        private void UpdateOrbCollection()
        {
            Transform currentOrb = orbs[collectedOrbs];

            if (currentOrb == null)
            {
                CollectOrb();
                return;
            }

            float distance = Vector3.Distance(
                playerCamera.transform.position,
                currentOrb.position);

            if (distance <= orbCollectionRadius)
            {
                CollectOrb();
                return;
            }

            if (objectiveText != null)
            {
                objectiveText.text =
                    "Orbes recogidos: " +
                    collectedOrbs + " / " +
                    orbs.Length;
            }

            if (timerText != null)
            {
                timerText.text =
                    "Siguiente orbe: " +
                    distance.ToString("0.0") +
                    " m";
            }

            if (promptText != null)
            {
                promptText.text =
                    "Recorre la fábrica y acércate al orbe luminoso";
            }

            if (progressFill != null)
            {
                progressFill.fillAmount =
                    orbs.Length > 0
                        ? (float)collectedOrbs / orbs.Length
                        : 0f;
            }
        }

        private void CollectOrb()
        {
            if (orbs != null &&
                collectedOrbs < orbs.Length &&
                orbs[collectedOrbs] != null)
            {
                orbs[collectedOrbs].gameObject.SetActive(false);
            }

            collectedOrbs++;
            PlayClip(orbClip);

            if (orbs != null && collectedOrbs >= orbs.Length)
            {
                if (finalZone != null)
                {
                    finalZone.gameObject.SetActive(true);
                }
            }
            else
            {
                ActivateOnlyCurrentOrb();
            }

            RefreshUI();
        }

        private void ActivateOnlyCurrentOrb()
        {
            if (orbs == null)
            {
                return;
            }

            for (int index = 0; index < orbs.Length; index++)
            {
                if (orbs[index] != null)
                {
                    orbs[index].gameObject.SetActive(
                        index == collectedOrbs);
                }
            }
        }

        private void UpdateFinalExposure()
        {
            if (finalZone == null ||
                centralSpider == null)
            {
                CompleteLevel();
                return;
            }

            float distance = Vector3.Distance(
                playerCamera.transform.position,
                finalZone.position);

            bool insideFinalZone =
                distance <= finalZoneRadius;

            bool lookingAtSpider =
                insideFinalZone &&
                NPLevel45Camera.IsLookingAt(
                    playerCamera,
                    centralSpider,
                    viewportLookRadius);

            if (lookingAtSpider)
            {
                finalLookTime += Time.deltaTime;
                lostLookTime = 0f;

                if (finalLookTime >= finalLookSeconds)
                {
                    CompleteLevel();
                    return;
                }
            }
            else
            {
                lostLookTime += Time.deltaTime;

                if (lostLookTime > lostLookTolerance)
                {
                    finalLookTime = 0f;
                }
            }

            if (objectiveText != null)
            {
                objectiveText.text =
                    "Orbes recogidos: " +
                    collectedOrbs + " / " +
                    (orbs != null ? orbs.Length : collectedOrbs);
            }

            if (timerText != null)
            {
                timerText.text =
                    "Mirada final: " +
                    finalLookTime.ToString("0.0") +
                    " / " +
                    finalLookSeconds.ToString("0") +
                    " s";
            }

            if (progressFill != null)
            {
                progressFill.fillAmount =
                    finalLookSeconds > 0f
                        ? Mathf.Clamp01(
                            finalLookTime / finalLookSeconds)
                        : 0f;
            }

            if (promptText != null)
            {
                if (!insideFinalZone)
                {
                    promptText.text =
                        "Entra en el círculo final\n" +
                        "Distancia: " +
                        distance.ToString("0.0") +
                        " m";
                }
                else if (!lookingAtSpider)
                {
                    promptText.text =
                        "Mira a la araña central";
                }
                else
                {
                    promptText.text =
                        "Mantén la mirada final";
                }
            }
        }

        private void RefreshUI()
        {
            if (objectiveText != null)
            {
                int total = orbs != null ? orbs.Length : 0;
                objectiveText.text =
                    "Orbes recogidos: " +
                    collectedOrbs + " / " + total;
            }

            if (timerText != null)
            {
                timerText.text =
                    collectedOrbs < (orbs != null ? orbs.Length : 0)
                        ? "Busca el siguiente orbe"
                        : "Prepárate para la mirada final";
            }

            if (progressFill != null)
            {
                int total = orbs != null ? orbs.Length : 0;
                progressFill.fillAmount =
                    total > 0
                        ? (float)collectedOrbs / total
                        : 0f;
            }

            if (promptText != null)
            {
                promptText.text =
                    collectedOrbs < (orbs != null ? orbs.Length : 0)
                        ? "Sigue el orbe luminoso"
                        : "Dirígete al círculo final";
            }
        }

        private void CompleteLevel()
        {
            if (completed)
            {
                return;
            }

            completed = true;

            if (spiderPatrols != null)
            {
                foreach (NPNaturalSpiderPatrol patrol in spiderPatrols)
                {
                    if (patrol != null)
                    {
                        patrol.PausePatrol();
                    }
                }
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
                    "Completaste NeuroPhobia\n" +
                    "Terminaste los cinco niveles de exposición.";
            }

            PlayClip(completionClip);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LoadMenu()
        {
            if (!string.IsNullOrWhiteSpace(menuSceneName))
            {
                SceneManager.LoadSceneAsync(menuSceneName);
            }
        }

        public void RestartLevel()
        {
            SceneManager.LoadSceneAsync(
                SceneManager.GetActiveScene().name);
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
