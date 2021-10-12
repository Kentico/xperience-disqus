using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace Disqus.Components.DisqusComponent
{
    public class DisqusComponentProperties : IWidgetProperties
    {
        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "Header", ExplanationText = "Text to display above comments. Add '{num}' to insert the comment count into the header.", DefaultValue = "Comments")]
        public string Header { get; set; }

        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "Thread identifier", ExplanationText = "An arbitrary string identifying the thread to load. If empty, it will be generated based on the current page's DocumentGUID.")]
        public string ThreadIdentifier { get; set; }
    }
}
