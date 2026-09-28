public interface ITypeableWord
{
    string CurrentWord { get; }
    string GetOriginalWord();
    void AdvanceLetter();
    void MistypeWord(float duration);
}
