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