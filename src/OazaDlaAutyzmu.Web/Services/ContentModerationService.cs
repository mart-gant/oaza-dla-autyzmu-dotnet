namespace OazaDlaAutyzmu.Web.Services;

public class ContentModerationService : IContentModerationService
{
    private static readonly HashSet<string> ProfanityWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Polish profanity list (commonly blocked words)
        "kurwa", "chuj", "pizda", "pierdol", "jebać", "skurwysyn", "zjeb", "dupek",
        "idiota", "debil", "kretyn", "głupek", "palant", "cham", "gnój", "świnia",
        "gówno", "srać", "szmata", "dziwka", "suka", "kuttas", "fiut", "cipka",
        // Add more as needed
    };

    public bool ContainsProfanity(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var words = content.Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var cleanWord = new string(word.Where(char.IsLetter).ToArray());
            if (ProfanityWords.Contains(cleanWord))
            {
                return true;
            }
        }

        return false;
    }

    public string GetProfanityReason(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        var words = content.Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var cleanWord = new string(word.Where(char.IsLetter).ToArray());
            if (ProfanityWords.Contains(cleanWord))
            {
                return $"Treść zawiera niedozwolone słowo: {cleanWord}";
            }
        }

        return string.Empty;
    }
}
