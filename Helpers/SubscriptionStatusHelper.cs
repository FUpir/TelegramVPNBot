using TelegramVPNBot.Enums;

namespace TelegramVPNBot.Helpers
{
    public static class SubscriptionStatusHelper
    {
        public static string GetSubscriptionStatusMessage(DateTime? expiredDateUtc, Language language)
        {
            SubscriptionStatus status = expiredDateUtc switch
            {
                null => SubscriptionStatus.None,
                var endDate when endDate > DateTime.UtcNow => SubscriptionStatus.Active,
                _ => SubscriptionStatus.Expired
            };

            switch (status)
            {
                case SubscriptionStatus.None:
                    return LanguageHelper.GetLocalizedMessage(language, "SubscriptionStatusNone");
                case SubscriptionStatus.Active:
                    return LanguageHelper.GetLocalizedMessage(language, "SubscriptionStatusActive");
                case SubscriptionStatus.Expired:
                    return LanguageHelper.GetLocalizedMessage(language, "SubscriptionStatusExpired");
                default:
                    return "Subscription status not found.";
            }
        }
    }
}
