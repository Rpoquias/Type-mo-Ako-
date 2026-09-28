using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float moveUpSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private float _timer;

    public void Show(int score)
    {
        text.text = $"+{score}";
        _timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
            Destroy(gameObject); 
    }
}
