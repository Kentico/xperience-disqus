var quillEmbedEnabled = false;
var quillPlaceholderText = "";

function initQuillEditor(replyingTo, embedEnabled = null, placeholderText = null) {

    if (embedEnabled) {
        quillEmbedEnabled = embedEnabled;
    }

    if (placeholderText) {
        quillPlaceholderText = placeholderText;
    }

    var mediaOptions = ['link'];
    if (quillEmbedEnabled) {
        mediaOptions.push('image', 'video');
    }
    var quill = new window.Quill(`#form_replyto_${replyingTo} #editor`, {
        bounds: `#form_replyto_${replyingTo} #editor_container`,
        theme: 'snow',
        placeholder: quillPlaceholderText,
        modules: {
            toolbar: [
                ['bold', 'italic', 'underline', 'strike'],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'header': 1 }, { 'header': 2 }, 'blockquote'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
                mediaOptions
            ]
        },
    });

    var message = $(`#form_replyto_${replyingTo} #Message`).val();
    const delta = quill.clipboard.convert(message);
    quill.setContents(delta, 'silent');

    // Update Message in form when Quill changes
    quill.on('text-change', function (delta, oldDelta, source) {
        var richText = quill.root.innerHTML;
        $(`#form_replyto_${replyingTo} #Message`).val(richText);
    });
}

function copyToClipboard(url) {

    var temp = $('<input>');
    $('body').append(temp);
    temp.val(url).select();
    document.execCommand('copy');
    temp.remove();
    alert('Comment permalink copied!');
}

function subscribeThread(sender) {

    var btn = $(sender);
    var id = btn.data('thread-id');
    var url = btn.data('url');
    var isSubscribed = btn.data('is-subscribed');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            doSubscribe: !isSubscribed
        }
    }).done(function (data, statusText, xhdr) {

        setIsSubscribed(!isSubscribed);
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

// Called from _DisqusUserDetails.cshtml modal follow button
function followUser(sender) {

    var btn = $(sender);
    var id = btn.data('user-id');
    var url = btn.data('url');
    var isFollowing = btn.data('is-following');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            doFollow: !isFollowing
        }
    }).done(function (data, statusText, xhdr) {

        setIsFollowing(!isFollowing);
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

function recommendThread(sender) {

    var btn = $(sender);
    var id = btn.data('thread-id');
    var url = btn.data('url');
    var isRecommended = btn.data('is-recommended');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            doRecommend: !isRecommended
        }
    }).done(function (data, statusText, xhdr) {

        setIsRecommended(!isRecommended);
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

// Called from a post's upvote/downvote buttons
function voteClick(sender) {

    var btn = $(sender);
    var id = btn.data('post-id');
    var isLike = btn.data('is-like');
    var url = btn.data('url');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            isLike: isLike
        }
    }).done(function (data, statusText, xhdr) {

        $(`#post_${id}`).before(data)
            .remove();
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
        btn.prop("disabled", false);
    });
}

// Called when a user's avatar is clicked
function showUser(sender) {

    var btn = $(sender);
    var id = btn.data('user-id');
    var url = btn.data('url');
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id
        }
    }).done(function (data, statusText, xhdr) {

        $('#user_modal').remove();
        $('body').append(data);
        var modal = new bootstrap.Modal(document.getElementById('user_modal'));
        modal.show();
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

function showOrHideReplyForm(sender) {

    var btn = $(sender);
    btn.text('Cancel');
    var postId = btn.data('post-id');
    var url = btn.data('url');
    var existingForm = $(`#form_replyto_${postId}`);
    if (existingForm.length > 0) {

        btn.text('Reply');
        existingForm.remove();
    }
    else {

        $.ajax({
            method: 'POST',
            url: url,
            data: {
                id: postId
            }
        }).done(function (data, statusText, xhdr) {

            $(`#post_${postId}`).after(data);
            initQuillEditor(postId);
        }).fail(function (xhdr, statusText, errorText) {

            alert(xhdr.responseText);
        });
    }
}

function addNewPost(getBodyUrl, newPostId, parent) {

    $.ajax({
        method: 'POST',
        url: getBodyUrl,
        data: {
            id: newPostId
        }
    }).done(function (data, statusText, xhdr) {

        if (parent === '') {

            // Not a reply, add after thread options
            $('#disqus_thread .thread-options').after(data);
            $('#disqus_thread #form_replyto_ .ql-editor').html('');
        }
        else {

            // Reply, add after the parent
            $(`#disqus_thread #post_${parent} .reply-link`).text('Reply');
            $(`#disqus_thread #post_${parent}`).after(data);
            $(`#disqus_thread #form_replyto_${parent}`).remove();
        }
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

function updateExistingPost(getBodyUrl, postId) {

    $.ajax({
        method: 'POST',
        url: getBodyUrl,
        data: {
            id: postId
        }
    }).done(function (data, statusText, xhdr) {

        $(`#disqus_thread #post_${postId}`)
            .before(data)
            .remove();
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

// Called from DisqusController.SubmitPost after a post is edited/created
function handleSubmit(response) {

    if (!response.success) {
        alert(response.message);
    }
    else {
        if (response.action === 'create') {

            addNewPost(response.url, response.id, response.parent);
        }
        else if (response.action === 'update') {

            updateExistingPost(response.url, response.id);
        }
    }
}

function reportPost(sender) {

    var btn = $(sender);
    var id = btn.data('post-id');
    var url = btn.data('url');
    var reason = $('#reportReasonModal input[name="report-reason"]:checked').val();
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            reason: reason
        }
    }).done(function (data, statusText, xhdr) {

        alert('Comment reported successfully.');
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

// Called from a post's edit button, gets editing form HTML
function editPost(sender) {

    var btn = $(sender);
    var id = btn.data('post-id');
    var url = btn.data('url');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id
        }
    }).done(function (data, statusText, xhdr) {

        $(`#disqus_thread #post_${id} .post-main`).html(data);
        initQuillEditor(id);
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
        btn.prop("disabled", false);
    });
}

function cancelEdit(sender) {

    var btn = $(sender);
    var id = btn.data('post-id');
    var url = btn.data('url');
    btn.prop('disabled', true);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id
        }
    }).done(function (data, statusText, xhdr) {

        $(`#disqus_thread #post_${id}`)
            .before(data)
            .remove();
    }).fail(function (xhdr, statusText, errorText) {

        alert(xhdr.responseText);
    });
}

function deletePost(sender) {

    if (confirm('Are you sure you want to delete this?')) {

        var btn = $(sender);
        var id = btn.data('post-id');
        var url = btn.data('url');
        btn.prop('disabled', true);
        $.ajax({
            method: 'POST',
            url: url,
            data: {
                id: id
            }
        }).done(function (data, statusText, xhdr) {

            var ids = data.split(',');
            for (var child of ids) {
                $(`#disqus_thread #post_${child}`).remove();
            }
            $(`#disqus_thread #post_${id}`).remove();
        }).fail(function (xhdr, statusText, errorText) {

            alert(xhdr.responseText);
            btn.prop("disabled", false);
        });
    }
}

// Changes the recommend heart color if the current user has voted positively on the thread
function setIsRecommended(isRecommended) {
    var icon = $('#disqus_thread .recommend-heart');
    var link = $('#disqus_thread .recommend-link');
    link.data('is-recommended', isRecommended);
    icon.toggleClass('recommended', isRecommended);

    if (isRecommended) {
        link.html('Recommended');
    }
    else {
        link.html('Recommend');
    }
}

// Alters the user modal follow button depending on whether the current user is following
function setIsFollowing(isFollowing) {
    var link = $('#user_modal .follow-button');
    link.prop('disabled', false)
        .data('is-following', isFollowing);

    if (isFollowing) {
        link.html('&check; Following')
            .css('background-color', '#43b311');

    }
    else {
        link.html('Follow')
            .css('background-color', '#2e9fff');
    }
}

// Changes the subscription text if the current user is subscribed to the thread
function setIsSubscribed(isSubscribed) {
    var link = $('#disqus_thread .subscribe-link');
    link.prop('disabled', false)
        .data('is-subscribed', isSubscribed);

    if (isSubscribed) {
        link.html('Unsubscribe');
    }
    else {
        link.html('Subscribe');
    }
}