$(document).ready(function () {
    $(".close").click(function () {
        $(this).parent().fadeOut();
        return false;
    });

    $('.datepicker').datepicker(); //Initialise any date pickers
    //$('.wysiwyg').wysihtml5(); //Initializes the wysiwyg editor

    //datepicker on modal
    $('.modal').on('shown', function () {
    });

    // Javascript to enable link to tab
    var url = document.location.toString();
    if (url.match('#')) {
        $('.nav-tabs a[href=#' + url.split('#')[1] + ']').tab('show');
    }

    // Change hash for page-reload
    $('.nav-tabs a').on('shown', function(e) {
        window.location.hash = e.target.hash;
    });

});

$(document).ready(function () {

    $(".imageupload").disableValidation = true;
    $(".imageupload").change(function () {
        $("input").addClass("ignore").disableValidation = true;
        $("input").hide();
        $("form").disableValidation = true;
        $("#mainform").submit();
    });

    $(".imagepreview").load(function () {
        //console.log($(this));
    });
    $(".imagepreview").click(function () {
        PreviewImage($(this).attr("src"));
    });


    $("input[type=file]").each(function () {
        var input = $(this);
        var name = input.attr("id");
        var btn = $("<input type='button' value='upload' class='btn btn-info'></input>");

        input.after(btn);
        input.css("display", "none");

        btn.click(function () {
            //console.log(input.attr("name"));
            input.click();
        });

    });

});


PreviewImage = function (uri) {

    var imageDialog = $("#dialog");
    var imageTag = $('#image', imageDialog);
    var uriParts = uri.split("/");

    imageTag.attr('src', uri);

    imageTag.load(function () {

        $('#dialog').dialog({
            zIndex: 9999,
            modal: true,
            resizable: true,
            draggable: true,
            width: 'auto',
            title: uriParts[uriParts.length - 1]
        });
    });
};
