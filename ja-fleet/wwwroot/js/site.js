const linkJetphotos = function(reg) {
    $.blockUI({ message: "" });
    $.ajax({
        type: 'GET',
        url: '/Aircraft/Photo/' + reg,
        cache: false
    }).fail(function (data) {
        $.unblockUI();
        $("#photsearcherror").modal();
    }).done(function (data) {
        $.unblockUI();
        if (data == null || data == "") {
            $("#notexistphoto").modal();
        } else {
            $.colorbox({ href: data, iframe: true, width: "95%", height: "90%" });
        }
    });
}

const openColorbox = function (link) {
    $.colorbox({ href: link, iframe: true, width: "95%", height: "90%" });
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
