using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Enemy_WordHandler : MonoBehaviour, ITypeableWord
{
    public event Action<Enemy> OnWordCompletedEvent;

    [Header("Text Components")]
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private Canvas worldCanvas;

    [Header("Visual Settings")]
    [SerializeField] private float successFlashDuration = 0.3f;
    [SerializeField] private float mistypeFlashDuration = 0.45f;

    [Header("Sorting Settings")]
    [SerializeField] private int baseSortingOrder = 1;
    [SerializeField] private int focusedSortingOrder = 100;

    private string _originalWord = "";
    private int _typedCount = 0;
    private bool _hasStartedTyping = false;
    private bool _isFocused = false;
    private bool _lockSortingDuringMistype = false;


    private Coroutine _mistypeCoroutine;
    private Coroutine _successCoroutine;
    private Camera _mainCamera;

    public string CurrentWord
    {
        get
        {
            if (string.IsNullOrEmpty(_originalWord)) return "";
            if (_typedCount >= _originalWord.Length) return "";
            return _originalWord.Substring(_typedCount);
        }
    }

    public bool IsFocused => _isFocused;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
            _mainCamera = FindObjectOfType<Camera>();

        if (worldCanvas == null)
            worldCanvas = GetComponentInChildren<Canvas>();
    }

    private void OnEnable()
    {
        FixCanvasCamera();

        if (WordManager.Instance != null)
            WordManager.Instance.RegisterWord(this);

        ResetWordState();
        SetFocused(false);
    }

    private void OnDisable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.UnregisterWord(this);
    }

    public void SetFocused(bool focused)
    {
        if (_isFocused == focused) return;

        _isFocused = focused;

        // Only update sorting order if not locked by a mistype
        if (!_lockSortingDuringMistype)
        {
            UpdateSortingOrder();
        }

        UpdateVisual();
    }


    private void UpdateSortingOrder()
    {
        if (worldCanvas == null) return;

        int newSortingOrder = _isFocused ? focusedSortingOrder : baseSortingOrder;

        if (worldCanvas.sortingOrder != newSortingOrder)
        {
            worldCanvas.sortingOrder = newSortingOrder;
        }
    }

    private void FixCanvasCamera()
    {
        if (worldCanvas == null) return;

        if (worldCanvas.worldCamera != _mainCamera)
        {
            worldCanvas.worldCamera = _mainCamera;
        }

        if (worldCanvas.renderMode != RenderMode.WorldSpace)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
        }

        worldCanvas.sortingLayerName = "UI Text";
        UpdateSortingOrder();
    }

    private void ResetWordState()
    {
        _originalWord = "";
        _typedCount = 0;
        _hasStartedTyping = false;

        if (_mistypeCoroutine != null)
        {
            StopCoroutine(_mistypeCoroutine);
            _mistypeCoroutine = null;
        }

        if (_successCoroutine != null)
        {
            StopCoroutine(_successCoroutine);
            _successCoroutine = null;
        }

        if (wordText != null)
        {
            wordText.text = "";
        }
    }

    public void SetWord(string word)
    {
        _originalWord = word ?? "";
        _typedCount = 0;
        _hasStartedTyping = false;
        UpdateVisual();
    }

    public void AdvanceLetter()
    {
        if (!_hasStartedTyping)
            _hasStartedTyping = true;

        _typedCount++;
        if (_typedCount >= _originalWord.Length)
        {
            if (_successCoroutine != null) StopCoroutine(_successCoroutine);
            _successCoroutine = StartCoroutine(SuccessRoutine());
        }
        else
        {
            UpdateVisual();
        }
    }

    public void MistypeWord(float duration)
    {
        if (string.IsNullOrEmpty(_originalWord)) return;
        if (!_hasStartedTyping) return;

        if (_mistypeCoroutine != null)
            StopCoroutine(_mistypeCoroutine);

        _typedCount = 0;
        _mistypeCoroutine = StartCoroutine(MistypeRoutine(duration));
    }

    public void ResetWord()
    {
        _typedCount = 0;
        UpdateVisual();
    }

    public string GetOriginalWord() => _originalWord;

    private void UpdateVisual()
    {
        if (wordText == null) return;

        if (string.IsNullOrEmpty(_originalWord))
        {
            wordText.text = "";
            return;
        }

        if (_typedCount <= 0)
        {
            // Show first letter underlined if focused and no letters typed yet
            if (_isFocused && _originalWord.Length > 0)
            {
                string firstLetter = _originalWord[0].ToString();
                string restOfWord = _originalWord.Length > 1 ? _originalWord.Substring(1) : "";
                wordText.text = $"<u>{firstLetter}</u>{restOfWord}";
            }
            else
            {
                wordText.text = _originalWord;
            }
            return;
        }

        var sb = new System.Text.StringBuilder(_originalWord.Length * 16);

        for (int i = 0; i < _originalWord.Length; i++)
        {
            char currentChar = _originalWord[i];

            if (i < _typedCount)
            {
                // Already typed letters = green
                sb.Append($"<color=green>{currentChar}</color>");
            }
            else if (i == _typedCount && _isFocused)
            {
                // Next letter to type = underlined (only if focused)
                sb.Append($"<u>{currentChar}</u>");
            }
            else
            {
                // Future letters = normal
                sb.Append(currentChar);
            }
        }

        wordText.text = sb.ToString();
    }

    private IEnumerator MistypeRoutine(float duration)
    {
        _lockSortingDuringMistype = true;

        wordText.text = $"<color=red>{_originalWord}</color>";
        yield return new WaitForSeconds(duration > 0 ? duration : mistypeFlashDuration);

        _lockSortingDuringMistype = false;

        UpdateVisual();
        UpdateSortingOrder();

        _mistypeCoroutine = null;
    }


    private IEnumerator SuccessRoutine()
    {
        wordText.text = $"<color=blue>{_originalWord}</color>";
        yield return new WaitForSeconds(successFlashDuration);

        OnWordCompletedEvent?.Invoke(GetComponent<Enemy>());
        GetComponent<Enemy>().OnWordCompleted();

        wordText.text = "";
        _successCoroutine = null;
    }

    public void InitializeForSpawn()
    {
        FixCanvasCamera();
        ResetWordState();
        SetFocused(false);
    }

    private void OnMouseDown()
    {
        if (WordManager.Instance != null && !string.IsNullOrEmpty(CurrentWord))
        {
            WordManager.Instance.SelectEnemy(this);
        }
    }
}
