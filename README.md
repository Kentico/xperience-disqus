[![Nuget](https://img.shields.io/nuget/v/Xperience.Core.Disqus)](https://www.nuget.org/packages/Xperience.Core.Disqus) [![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico)

# Xperience Disqus Widget

## Installation

1. Install this package in your .NET Core project
1. Create a [new application](https://disqus.com/api/applications/register/)
1. On the __Settings__ tab, set the __Callback URL__ to `https://<your MVC site>/disqus/auth`
1. Switch to the __Details__ tab and note the keys under the __OAuth Settings__ section
1. Open the [Disqus Admin](https://disqus.com/admin/)
1. If you see "What would you like to do with Disqus?" select "I want to install Disqus on my site"
1. Create a new site (or select an existing one) and note the __Shortname__ on the __Settings > General tab__
1. In your MVC project's startup code, initialize the Disqus integration: `services.AddDisqus();`
1. In your MVC project's `appsettings.json`, add the following section:

```json
"Disqus": {
    "Site": "<Shortname from step 5>",
    "ApiKey": "<API key from step 4>",
    "ApiSecret": "<API secret from step 4>",
    "AuthenticationRedirect": "https://<your MVC site>/disqus/auth"
}
```
## Requirements

The integration is currently built on __Kentico.Xperience.AspNetCore.WebApp__ 13.0.32

---

The default layout requires Bootstrap and jQuery. The following resources should be added to your main `_Layout.cshtml`:

```html
   <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-F3w7mX95PdgyTmZZMECAngseQB83DfGTowi0iMjiWaeVhAn4FJkqJByhZMI3AhiU" crossorigin="anonymous">
   <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
   <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>
```

---

Your MVC project's routing must contain a "catch-all" route:

```cs
endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}",
    defaults: new {
        controller = "Home",
        action = "Index"
    }
);
```

Or, you can register a custom route for Disqus:

```cs
endpoints.MapControllerRoute(
    name: "Disqus",
    pattern: "disqus/{action}",
    defaults: new {
        controller = "Disqus"
    }
);
```

## Adding Disqus to your pages

The Disqus widget can be added as a standard pagebuilder widget, or directly to your views as a [standalone widget](https://docs.xperience.io/developing-websites/page-builder-development/rendering-widgets-in-code):

```cs
@using Disqus.Components.DisqusComponent

@{
    var widgetProperties = new DisqusComponentProperties()
    {
        Header = "Reviews"
    };
}
<standalone-widget widget-type-identifier="@DisqusComponent.IDENTIFIER" widget-properties="widgetProperties" />
```

There are 2 optional properties that you can configure:

- __Header__ (default: "Comments") - The text that appears above the comments. You can use the placeholder `{num}` to display the comment count in the header, e.g. _"{num} comments"_
- __ThreadIdentifier__ (default: current DocumentGUID) - The [Disqus identifier](https://help.disqus.com/en/articles/1717082-what-is-a-disqus-identifier) of the thread to load comments for. This is an arbitrary string which can be used to load comments from any Disqus thread, even if it is not related to the current page

The widget can be placed on _any_ view in your .NET Core project. However, if it is placed on a page without representation in the Xperience content tree, you _must_ set the __ThreadIdentifier__ property.

## Displaying links with comment counts

You can place a link to the comments section of any page by appending `#disqus_thread` to the URL. The link can also contain the number of comments on that page:

![Comment link](img/comment-link.png)

If you'd like to also display the number of comments, you can use the [default Disqus functionality](https://help.disqus.com/en/articles/1717274-adding-comment-count-links-to-your-home-page), which requires adding the `count.js` script along with the absolute URL. To get the URL of an Xperience page, you can use `IPageUrlRetriever`:

```cs
@inject IPageUrlRetriever urlRetriever

<a href="@(urlRetriever.Retrieve(node).AbsoluteUrl)#disqus_thread"></a>
```

# Disqus administration

The following is a summary of the features implemented in this widget which are configurable from the [Disqus administration](https://disqus.com/admin/):

- __General tab__
  - Comment Policy URL
  - Comment Policy Summary
  - Default Commenter Avatar
  - Disqus Branding
- __Community tab__
  - Comment Count Link (default Disqus functionality)
  - Default Sort
  - Moderator Badge Text
  - Star Ratings
  - Hide Social Share
  - Voting
  - Comment Prompt
- __Moderation tab__
  - Images and Videos

# On-line marketing

## Logging On-line marketing activities

With this integration, you can log activities whenever a new comment is posted or a comment is reported by another user. If you have enabled [text analysis](https://docs.xperience.io/configuring-xperience/managing-sites/configuring-settings-for-sites/settings-content/settings-text-analytics), the sentiment of the comment will be logged with the activity, otherwise all comments will have the "neutral" sentiment.

This could be helpful in the creation of [marketing automation](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/marketing-automation) processes or [contact groups](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/contact-management/segmenting-contacts-into-contact-groups). For example, if a contact leaves a positive comment on an article, you may want to send them an email offering a discount on the article's advertised products. Or, if a comment is reported by other users, a site administrator can reach out to the commenter to try and defuse the situation.

To begin logging activities, configure either (or both) of the following [activity types](https://docs.xperience.io/on-line-marketing-features/configuring-and-customizing-your-on-line-marketing-features/configuring-activities/adding-custom-activity-types), depending on which you'd like to log. Only the code names of the activity types need to match exactly- the rest can be altered to meet your needs.

![Comment activity](img/activity-comment.png)
![Report activity](img/activity-report.png)

## Triggering marketing automation processes

You can reference these activies to trigger [marketing automation](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/marketing-automation) processes. For example, you may want to trigger a process when a positive comment is left on a specific page:

- Trigger:
    - __Display name__: Positive comment on article
    - __Type__: Contact performed an activity
    - __Activity type__: Disqus comment
    - __Additional condition__: `Activity.ActivityValue == "positive" && Activity.Node.NodeAliasPath == "/Articles/Coffee-Beverages-Explained"`

You could also check the comment's sentiment in an __If/Else__ step to create a process with multiple branches. Remove the __Additional condition__ from the above trigger so the process runs for all comments. Then, set up your process something like this:

![Automation](/img/automation.png)

In the __If/Else__ condition, you can check the sentiment of the triggering activity in the `AutomationState` object:

`AutomationState.StateCustomData["TriggerDataActivityValue"] == "negative"`

In this example, if the comment is negative the contact will be sent an email after 1 day. If it is positive, the contact is added to a contact group.

## Creating macro rules

These activities can be used in conditional [contact groups](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/contact-management/segmenting-contacts-into-contact-groups) and other On-line marketing functionalities by creating your own [macro rules](https://docs.xperience.io/macro-expressions/writing-macro-conditions/creating-macro-rules). The following steps create a macro rule that can be used in a contact group which contains contacts who left a comment with the chosen sentiment in the last X days:

1. In __Contact management > Configuration tab > Macro rules__, click "Create new macro rule"
- __Display name__: "Contact commented on Disqus"
- __User text__: "Contact left a {sentiment} comment in the last {days} days"
- __Condition__: `Contact.DidActivity("disquscomment", "", {days}, "ActivityValue='{sentiment}'")`
2. Save and switch to the __Parameters tab__
3. On the __sentiment__ property, change the control to "Drop-down list" and under __List of options__, add:
- positive
- negative
- neutral
- mixed
4. On the __days__ property, change the __Data type__ to "Integer number"

Your users can now reference this rule when creating contact groups:

![Rule designer](/img/rule-designer.png)

# Customizing your comments

Most customization of the Disqus widget can be accomplished using standard CSS practices. Refer to [disqus.css](/wwwroot/disqus.css) to see the default styles and classes you can override. For example, you change the background color and border of a comment and it's replies by adding this to your site's stylesheet:

```css
#disqus_thread .disqus-post {
    background-color: #ddd !important;
}

#disqus_thread .child-post {
    background-color: #fff !important;
    border-left: 2px dotted #888 !important;
}
```

For more complex alterations to the layout, you can use default MVC functionality to modify the generated HTML output of the widget. If you place a view in your .NET Core project with the same name and location as this repository, that view will be used instead. This means you can refer to [our default views](/Views/Shared/Components/DisqusComponent), copy the code there, add it to your project, and remove elements, add elements, and move them around.

> :warning: Much of the functionality, including asynchronous updates to the DOM, rely on certain IDs, classes, and attributes on elements. In general, moving elements and adding new ones should not interfere with this functionality. However, we recommend refraining from removing classes from elements or making heavy modifications to the layouts. Always test your modifications thoroughly before deploying the changes!

For example, I'm working in the Dancing Goat sample project and I'd like each comment to have a more "compact" layout. In Visual Studio I create the following folder(s):

- /Views/Shared/Components/DisqusComponent

I can then create a new view called `_DisqusPost.cshtml` in that folder and copy the code from [the default view](/Views/Shared/Components/DisqusComponent/_DisqusPost.cshtml). If I alter the `div` which contains the post to the following:

```cs
...

<div id="post_@Model.Id" class="row row-cols-1 @cssClass" style="padding-left:@margin">
    <div class="post-sidebar col-auto">
        <a id="comment-@Model.Id"></a>
        <a class="cursor-pointer" onclick="showUser(this)" data-user-id="@Model.Author.Id" data-url="@Url.Action("GetUserDetailBody", "Disqus")">
            <img class="disqus-user-avatar-xs" src="@Model.Author.AvatarUrl" />
        </a>
    </div>
    <div class="post-main col-auto">
        <div class="row row-cols-1">
            <div class="col px-0">
                @if (Model.Author.ThreadRating > 0 && disqusService.CurrentForum.Settings.ThreadRatingsEnabled && Model.ThreadObject.RatingsEnabled)
                {
                    await Html.RenderPartialAsync("_DisqusStarRatings.cshtml", new DisqusRatingViewModel()
                    {
                        Disabled = true,
                        Classes = "px-0 pt-1",
                        StarId = $"post-{Model.Id}",
                        Rating = Model.Author.ThreadRating
                    });
                }
                @if (await disqusRepository.IsModerator(Model.Author.Id))
                {
                    <div class="user-badge badge-mod">@disqusService.CurrentForum.ModeratorBadgeText</div>
                }
                <span class="post-author">
                    <a onclick="showUser(this)" class="user-link cursor-pointer" data-user-id="@Model.Author.Id" data-url="@Url.Action("GetUserDetailBody", "Disqus")">@Model.Author.Name</a>
                </span>
                <span class="post-date">
                    &commat; @Model.CreatedAt.ToString()
                    @if (Model.IsEdited)
                    {
                        <i>(Edited)</i>
                    }
                </span>
            </div>
            <div class="post-content col px-0">
                @Html.Raw(Model.Raw_Message)
            </div>
            @{
                await Html.RenderPartialAsync("_DisqusPostFooter.cshtml", Model);
            }
        </div>
    </div>

    <div class="post-flag-container d-none">
        <button class="report-button" title="Report post" onclick="$('#reportReasonModal #report_button').data('post-id', '@Model.Id');" data-toggle="modal" data-target="#reportReasonModal">
            &#127988
        </button>
    </div>
</div>
```

When I run the Dancing Goat project, I can see that the posts have changed slightly:

![Layout comapre](/img/layout-compare.png)