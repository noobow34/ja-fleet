function linkJetphotos(reg) {
    $.blockUI({message:""});
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
            $.colorbox({ href: data, iframe: true, width: "80%", height: "90%" });
        }
    });
}

function datatablesLangInit() {
    $.extend($.fn.dataTable.defaults, {
        language: {
            url: "https://cdn.datatables.net/plug-ins/9dcbecd42ad/i18n/Japanese.json"
        }
    });
}