const openDetail = function (reg) {
    if (window.innerWidth < 700) {
        window.open('/ADNB/' + reg);
    } else {
        openPopUp('/ADN/' + reg)
    }
}

const openPopUp = function(url){
    $.magnificPopup.open({
        items: { src: url },
        type: 'iframe'
    });
    let setwidth = window.innerWidth * 0.95;
    if (setwidth > 1200) {
        setwidth = 1200;
    }
    $('.mfp-iframe-holder .mfp-content').css({ 'height': '95vh' });
    $('.mfp-iframe-holder .mfp-content').css({ 'max-width': setwidth + 'px' });
    $('.mfp-iframe-holder .mfp-content').css({ 'width': setwidth + 'px' });
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
