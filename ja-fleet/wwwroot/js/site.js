const openColorbox = function (link) {
    $.magnificPopup.open({
        items: { src: link },
        type: 'iframe'
    });
    $('.mfp-iframe-holder .mfp-content').css({ 'height': '90vh' });
}

const sendMessageToMe = function() {
    $.ajax({
        type: 'POST',
        url: '/Message/Send',
        data: { name: $('#uname').val(), replay: $('#replayto').val(), message: $('#message').val() },
        cache: false
    }).fail(function (data) {
        $('#result').text("送信しました");
    }).done(function (data) {
        if (data == "OK") {
            $('#uname').val("");
            $('#replayto').val("");
            $('#message').val("");
            $('#result').text("送信しました");
        } else {
            $('#result').text("送信に失敗しました");
        }
    });
}

const datatablesLangInit = function() {
    $.extend($.fn.dataTable.defaults, {
        language: {
            url: "https://cdn.datatables.net/plug-ins/9dcbecd42ad/i18n/Japanese.json"
        }
    });
}
