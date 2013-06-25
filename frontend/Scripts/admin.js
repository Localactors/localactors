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

