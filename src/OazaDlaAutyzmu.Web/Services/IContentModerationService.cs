namespace OazaDlaAutyzmu.Web.Services;

public interface IContentModerationService
{
    bool ContainsProfanity(string content);
    string GetProfanityReason(string content);
}
