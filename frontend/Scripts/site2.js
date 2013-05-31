$(document).ready(function () {
    //    $(".close").click(function () {
    //        $(this).parent().fadeOut();
    //        return false;
    //    });

    $('.datepicker').datepicker(); //Initialise any date pickers

    $('.wysiwyg').wysihtml5(); //Initializes the wysiwyg editor

    //datepicker on modal
    $('.modal').on('shown', function () {
        //$('.datepicker').datepicker();
        //$('.wysihtml5').wysihtml5();
    });

});

