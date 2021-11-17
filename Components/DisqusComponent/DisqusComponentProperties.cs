using CMS.DocumentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace Kentico.Xperience.Disqus.Components
{
    /// <summary>
    /// The configurable properties for the Disqus widget.
    /// </summary>
    public class DisqusComponentProperties : IWidgetProperties
    {
        /// <summary>
        /// The CSS class(es) added to the Disqus widget's containing DIV.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "CSS class", ExplanationText = "The CSS class(es) added to the Disqus widget's containing DIV.")]
        public string CssClass { get; set; } = "disqus-thread";

        /// <summary>
        /// An unique string identifying the current page. If empty, it will be generated based on the page's DocumentGUID.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Page identifier", ExplanationText = "An unique string identifying the current page. If empty, it will be generated based on the page's DocumentGUID.")]
        public string PageIdentifier { get; set; }

        /// <summary>
        /// A custom title for the created Disqus thread. If null, the <see cref="TreeNode.DocumentName"/> or page title will be used.
        /// </summary>
        public string Title { get; set; }
    }
}