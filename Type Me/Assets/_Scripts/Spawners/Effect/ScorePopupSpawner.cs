using System.Collections;
using UnityEngine;

public class ScorePopupSpawner : MonoBehaviour
{
    [SerializeField] private ScorePopup popupPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (ScoreManager.Instance == null)
            yield return null;

        ScoreManager.Instance.OnScoreAdded += SpawnPopup;
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreAdded -= SpawnPopup;
    }


    private void SpawnPopup(Vector3 worldPos, int points)
    {
        Vector3 spawnPos = worldPos + spawnOffset;
        ScorePopup popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        popup.Show(points);
    }
}
