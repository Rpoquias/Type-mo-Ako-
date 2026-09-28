using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "WordBank", menuName = "Scriptable Objects/WordBank")]
public class WordBank : ScriptableObject
{
    [Header("Word Files")]
    public TextAsset normalWordsFile;
    public TextAsset smallWordsFile;
    public TextAsset twoWordsFile;
    public TextAsset sentenceFile;

    private string[] normalWords;
    private string[] smallWords;
    private string[] twoWords;
    private string[] sentence;

    private void OnEnable()
    {
        if (normalWordsFile != null)
            normalWords = LoadWords(normalWordsFile);

        if (smallWordsFile != null)
            smallWords = LoadWords(smallWordsFile);

        if (twoWordsFile != null)
            twoWords = LoadWords(twoWordsFile);

        if (sentenceFile != null)
            sentence = LoadWords(sentenceFile);
    }

    private string[] LoadWords(TextAsset file)
    {
        return file.text
            .Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => !string.IsNullOrEmpty(w))
            .ToArray();
    }


    public string GetRandomWord(WordDifficulty difficulty)
    {

        string[] pool = null;

        switch (difficulty)
        {
            case WordDifficulty.Normal: pool = normalWords; break;
            case WordDifficulty.Small: pool = smallWords; break;
            case WordDifficulty.TwoWords: pool = twoWords; break;
            case WordDifficulty.Sentence: pool = sentence; break;
        }

        if (pool == null || pool.Length == 0)
            return string.Empty;

        return pool[Random.Range(0, pool.Length)];
       
    }
}
