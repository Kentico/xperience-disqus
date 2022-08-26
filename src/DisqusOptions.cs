namespace Kentico.Xperience.Disqus
{
    /// <summary>
    /// Disqus integration options.
    /// </summary>
    public sealed class DisqusOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SECTION_NAME = "xperience.disqus";


        /// <summary>
        /// Disqus site short name.
        /// </summary>
        public string SiteShortName {
            get;
            set;
        }
    }
}
