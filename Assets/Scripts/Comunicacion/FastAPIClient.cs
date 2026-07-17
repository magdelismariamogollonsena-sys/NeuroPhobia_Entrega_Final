using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FastAPIClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:8000";
    [SerializeField] private int timeoutSeconds = 5;

    public IEnumerator PostSessionStart(string sessionId, int phobiaGrade)
    {
        SessionStartPayload payload = new SessionStartPayload
        {
            sessionId = sessionId,
            phobiaGrade = phobiaGrade,
            timestamp = System.DateTime.UtcNow.ToString("o")
        };
        yield return PostJson("/api/session/start", JsonUtility.ToJson(payload));
    }

    public IEnumerator PostLevelMetrics(string sessionId, int level, LevelMetrics metrics)
    {
        LevelMetricsPayload payload = new LevelMetricsPayload { sessionId = sessionId, level = level, metrics = metrics };
        yield return PostJson("/api/session/metrics", JsonUtility.ToJson(payload));
    }

    public IEnumerator PostSessionEnd(string sessionId, SessionReport fullReport)
    {
        SessionEndPayload payload = new SessionEndPayload { sessionId = sessionId, fullReport = fullReport };
        yield return PostJson("/api/session/end", JsonUtility.ToJson(payload));
    }

    public IEnumerator GetAdaptiveCommand(string sessionId, System.Action<string> onCommand)
    {
        string url = $"{baseUrl}/api/session/command/{sessionId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = timeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onCommand?.Invoke("none");
                yield break;
            }

            AdaptiveCommandResponse response = JsonUtility.FromJson<AdaptiveCommandResponse>(request.downloadHandler.text);
            onCommand?.Invoke(string.IsNullOrEmpty(response.action) ? "none" : response.action);
        }
    }

    private IEnumerator PostJson(string endpoint, string json)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"NeuroPhobia: backend no disponible para {endpoint}. Se continua localmente.");
            }
        }
    }

    [System.Serializable] private class SessionStartPayload { public string sessionId; public int phobiaGrade; public string timestamp; }
    [System.Serializable] private class LevelMetricsPayload { public string sessionId; public int level; public LevelMetrics metrics; }
    [System.Serializable] private class SessionEndPayload { public string sessionId; public SessionReport fullReport; }
    [System.Serializable] private class AdaptiveCommandResponse { public string action; }
}
