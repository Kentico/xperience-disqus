using CMS.DocumentEngine;

using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.Disqus.Widget
{
    /// <summary>
    /// The configurable properties for the Disqus widget.
    /// </summary>
    public class DisqusComponentProperties : IWidgetProperties
    {
        /// <summary>
        /// The CSS class(es) added to the Disqus widget's containing DIV.
        /// </summary>
        [TextInputComponent(Label = "Css classes", ExplanationText = "Enter any number of CSS classes to apply to the Disqus thread, e.g. 'comments blue'")]
        public string CssClass { get; set; } = "disqus-thread";


        /// <summary>
        /// An unique string identifying the current page. If empty, it will be generated based on the page's DocumentGUID.
        /// </summary>
        public string PageIdentifier { get; set; }


        /// <summary>
        /// A custom title for the created Disqus thread. If null, the <see cref="TreeNode.DocumentName"/> or page title will be used.
        /// </summary>
        public string Title { get; set; }
    }
}