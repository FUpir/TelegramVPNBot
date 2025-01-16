using System.Globalization;
using System.Resources;
using TelegramVPNBot.Enums;

namespace TelegramVPNBot.Helpers
{
    public static class LanguageHelper
    {
        public static Language GetLanguage(string? languageCode)
        {
            return languageCode switch
            {
                "en" => Language.English,
                "ru" => Language.Russian,
                "he" => Language.Hebrew,
                _ => Language.Another
            };
        }

        public static string GetLocalizedMessage(Language userLanguage, string resourceKey)
        {
            ResourceManager resourceManager;

            switch (userLanguage)
            {
                case Language.English:
                    resourceManager = Properties.Resource_en.ResourceManager;
                    break;
                case Language.Russian:
                    resourceManager = Properties.Resource_ru.ResourceManager;
                    break;
                case Language.Hebrew:
                    resourceManager = Properties.Resource_he.ResourceManager;
                    break;
                default:
                    resourceManager = Properties.Resource_en.ResourceManager;
                    break;
            }

            var messageTemplate = resourceManager.GetString(resourceKey);

            if (messageTemplate == null)
                return "Message not found.";

            return messageTemplate;
        }
    }
}
