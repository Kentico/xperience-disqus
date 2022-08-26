using Azure.AI.TextAnalytics;

namespace Kentico.Xperience.Disqus.OnlineMarketing
{
    /// <summary>
    /// Maps the <see cref="TextSentiment"/> type to the <see cref="DisqusTextSentiment"/> type.
    /// </summary>
    internal static class TextSentimentMapper
    {
        /// <summary>
        /// Returns <see cref="DisqusTextSentiment"/> type mapped from <see cref="TextSentiment"/> type.
        /// </summary>
        /// <param name="textSentiment"><see cref="TextSentiment"/> type.</param>
        public static DisqusTextSentiment Map(TextSentiment textSentiment)
        {
            return textSentiment switch
            {
                TextSentiment.Positive => DisqusTextSentiment.Positive,
                TextSentiment.Negative => DisqusTextSentiment.Negative,
                TextSentiment.Neutral => DisqusTextSentiment.Neutral,
                TextSentiment.Mixed => DisqusTextSentiment.Mixed,
                _ => DisqusTextSentiment.Uknown,
            };
        }
    }
}
