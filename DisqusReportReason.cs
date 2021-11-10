namespace Kentico.Xperience.Disqus
{
    //https://disqus.com/api/docs/posts/report/
    public enum ReportReason
    {
        HARASSMENT,
        SPAM,
        INAPPROPRIATE_CONTENT,
        THREAT,
        IMPERSONATION,
        PRIVATE_INFORMATION,
        DISAGREE,
    }

    public static class ReportReasonMethods
    {
        public static string GetStringRepresentation(this ReportReason reason)
        {
            var reasonName = reason.ToString().ToLower().Replace('_', ' ');
            var description = "";
            switch (reason)
            {
                case ReportReason.HARASSMENT:
                    description = "posted or encouraged others to post harassing comments or hate speech targeting me, other individuals, or groups";
                    break;
                case ReportReason.SPAM:
                    description = "posted spam comments or discussions";
                    break;
                case ReportReason.INAPPROPRIATE_CONTENT:
                    description = "profile or comment contains inappropriate images or text";
                    break;
                case ReportReason.THREAT:
                    description = "posted directly threatening content";
                    break;
                case ReportReason.IMPERSONATION:
                    description = "misrepresents themselves as someone else";
                    break;
                case ReportReason.PRIVATE_INFORMATION:
                    description = "posted someone else's personally identifiable information";
                    break;
                case ReportReason.DISAGREE:
                    description = "I disagree with this user";
                    break;
            }
            return $"<b>{reasonName}</b> - {description}";
        }
    }
}
