[![Nuget](https://img.shields.io/nuget/v/Kentico.Xperience.Disqus)](https://www.nuget.org/packages/Kentico.Xperience.Disqus) [![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico)

# Xperience Disqus Widget

## Installation

1. Install [Kentico.Xperience.Disqus](https://www.nuget.org/packages/Kentico.Xperience.Disqus) in your .NET Core project
1. Open the [Disqus Admin](https://disqus.com/admin/)
1. If you see "What would you like to do with Disqus?" select "I want to install Disqus on my site"
1. Create a new site (or select an existing one) and note the __Shortname__ on the __Settings > General tab__
1. In your MVC project's `appsettings.json`, add the following setting:

```json
"xperience.disqus":  {
    "siteShortname": "my-awesome-site"
}
```
## Requirements

The integration is currently built on __Kentico.Xperience.AspNetCore.WebApp__ 13.0.32

## Adding Disqus to your pages

The Disqus widget can be added as a standard pagebuilder widget, or directly to your views as a [standalone widget](https://docs.xperience.io/developing-websites/page-builder-development/rendering-widgets-in-code):

```cs
@using Kentico.Xperience.Disqus

<standalone-widget widget-type-identifier="@DisqusComponent.IDENTIFIER" widget-properties="new DisqusComponentProperties()" />
```

There are 3 optional properties that you can configure:

- __Title__ - A custom title for the created Disqus thread. If not set, the `DocumentName` or HTML `title` will be used
- __PageIdentifier__ - The [Disqus identifier](https://help.disqus.com/en/articles/1717082-what-is-a-disqus-identifier) of the created thread. If not set, the `DocumentGUID` is used
- __CssClass__ - One or more classes to add to the DIV that surrounds the Disqus comments

The widget can be placed on _any_ view in your .NET Core project. However, if it is placed on a view without representation in the Xperience content tree, you _must_ set the __PageIdentifier__ property.

## Displaying links with comment counts

You can place a link to the comments section of any page by appending `#disqus_thread` to the URL. The link can also contain the number of comments on that page:

![Comment link](img/comment-link.png)

If you'd like to also display the number of comments, you can use the [default Disqus functionality](https://help.disqus.com/en/articles/1717274-adding-comment-count-links-to-your-home-page), which requires adding the `count.js` script along with the absolute URL. To get the URL of an Xperience page, you can use `IPageUrlRetriever`:

```cs
@inject IPageUrlRetriever urlRetriever

<a href="@(urlRetriever.Retrieve(node).AbsoluteUrl)#disqus_thread"></a>
```

# On-line marketing

## Logging On-line marketing activities

With this integration, you can log activities whenever a new comment is posted. If you have enabled [text analysis](https://docs.xperience.io/configuring-xperience/managing-sites/configuring-settings-for-sites/settings-content/settings-text-analytics), the sentiment of the comment will be logged with the activity, otherwise all comments will have the "neutral" sentiment. This could be helpful in the creation of [marketing automation](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/marketing-automation) processes or [contact groups](https://docs.xperience.io/on-line-marketing-features/managing-your-on-line-marketing-features/contact-management/segmenting-contacts-into-contact-groups). For example, if a contact leaves a positive comment on an article, you may want to send them an email offering a discount on the article's advertised products.

To begin logging activities, configure the following custom [activity type](https://docs.xperience.io/on-line-marketing-features/configuring-and-customizing-your-on-line-marketing-features/configuring-activities/adding-custom-activity-types) in __Contact management > Configuration > Activity types__. Only the code name of the activity type needs to match exactly- the rest can be altered to meet your needs.

![Comment activity](img/activity-comment.png)

Your MVC project's routing must add comment activity route:

```cs
app.UseEndpoints(endpoints =>
{
    endpoints.Kentico().MapRoutes();

    ...

    endpoints.MapDisqusActivityTracking();

...
```
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
