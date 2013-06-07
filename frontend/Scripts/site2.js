$(document).ready(function () {
    $(".close").click(function () {
        $(this).parent().fadeOut();
        return false;
    });

    $("#supported_list a.close")[0].click(function () {
        $("#supported_list").slideUp();
        return false;
    });

    $("#profile-bar #donation a").click(function () {
        $("#supported_list").slideToggle();
        return false;
    });

    //    $('.datepicker').datepicker(); //Initialise any date pickers
    //    $('.wysiwyg').wysihtml5(); //Initializes the wysiwyg editor

    //datepicker on modal
    $('.modal').on('shown', function () {
    });

});

