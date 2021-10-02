function voteClick(sender) {

    var btn = $(sender);
    btn.prop('disabled', true);
    var id = btn.data('post-id');
    var isLike = btn.data('is-like');
    var url = btn.data('url');
    var container = $('#post_' + id);
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id,
            isLike: isLike
        }
    }).done(function (data, statusText, xhdr) {
        container.before(data);
        container.remove();
    }).fail(function (xhdr, statusText, errorText) {
        alert(xhdr.responseText);
        btn.prop("disabled", false);
    });
}

// Called from DisqusController.SubmitPost after a post is edited/created
function updatePost(response) {

    if (!response.success) {
        alert(response.message);
    }
    else {
        if (response.action === 'create') {

            $.ajax({
                method: 'POST',
                url: response.url,
                data: {
                    id: response.id
                }
            }).done(function (data, statusText, xhdr) {

                var container;
                if (response.parent === '') {
                    // Not a reply. The first submit form has no id so add after that
                    container = $('#disqus_thread #post_reply_container_reply_');
                    // Clear the main post input
                    container.find('.ql-editor').html('');
                }
                else {
                    // Reply, add after the reply form for parent
                    container = $('#disqus_thread #post_reply_container_reply_' + response.parent);
                    // Collapse the reply container on parent
                    container.toggleClass('show');
                    // Clear the input on the parent's reply form
                    container.find('.ql-editor').html('');
                }
                container.after(data);
            }).fail(function (xhdr, statusText, errorText) {
                alert(xhdr.responseText);
            });
        }
        else if (response.action === 'update') {

            $.ajax({
                method: 'POST',
                url: response.url,
                data: {
                    id: response.id
                }
            }).done(function (data, statusText, xhdr) {

                var container = $('#disqus_thread #post_' + response.id);
                container.before(data);
                container.remove();
            }).fail(function (xhdr, statusText, errorText) {
                alert(xhdr.responseText);
            });
        }
    }
}

// Called from the report reason modal
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

// Called from a post's edit button
function editPost(sender) {

    var btn = $(sender);
    btn.prop('disabled', true);
    var id = btn.data('post-id');
    var url = btn.data('url');
    var container = $('#post_' + id + '> .post-main');
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id
        }
    }).done(function (data, statusText, xhdr) {
        container.html(data);
    }).fail(function (xhdr, statusText, errorText) {
        alert(xhdr.responseText);
        btn.prop("disabled", false);
    });
}

function cancelEdit(sender) {

    var btn = $(sender);
    btn.prop('disabled', true);
    var id = btn.data('post-id');
    var url = btn.data('url');
    $.ajax({
        method: 'POST',
        url: url,
        data: {
            id: id
        }
    }).done(function (data, statusText, xhdr) {

        var container = $('#disqus_thread #post_' + id);
        container.before(data);
        container.remove();
    }).fail(function (xhdr, statusText, errorText) {
        alert(xhdr.responseText);
    });
}

// Called from a post's delete button
function deletePost(sender) {

    if (confirm('Are you sure you want to delete this?')) {

        var btn = $(sender);
        btn.prop('disabled', true);
        var id = btn.data('post-id');
        var url = btn.data('url');
        $.ajax({
            method: 'POST',
            url: url,
            data: {
                id: id
            }
        }).done(function (data, statusText, xhdr) {
            var container = $('#disqus_thread #post_' + id);
            container.remove();
        }).fail(function (xhdr, statusText, errorText) {
            alert(xhdr.responseText);
            btn.prop("disabled", false);
        });
    }
}

function ajaxBegin() {
    $('#disqus_thread input[type=submit]').attr('disabled', true);
}

function ajaxComplete() {
    $('#disqus_thread input[type=submit]').attr('disabled', false);
}