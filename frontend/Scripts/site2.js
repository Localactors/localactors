$(document).ready(function () {
    $(".close").click(function () {
        $(this).parent().fadeOut();
        return false;
    });

    //donations
    $("#supported_list a.close")[0].click(function () {
        $("#supported_list").slideToggle();
        return false;
    });
    $("#profile-bar #donation a").click(function () {
        $("#supported_list").slideToggle();
        $("#stream_update").slideUp();
        return false;
    });

    //updates
    $("#stream_update a.close")[0].click(function () {
        $("#stream_update").slideToggle();
        return false;
    });
    $("#profile-bar #updates a").click(function () {
        $("#stream_update").slideToggle();
        $("#supported_list").slideUp();
        return false;
    });

    //    $('.datepicker').datepicker(); //Initialise any date pickers
    //    $('.wysiwyg').wysihtml5(); //Initializes the wysiwyg editor

    //datepicker on modal
    $('.modal').on('shown', function () {
    });
    
});

$(document).ready(function () {
    $(".imageupload").disableValidation = true;
    $(".imageupload").change(function () {
        $("input").addClass("ignore").disableValidation = true;
        $("form").disableValidation = true;
        $("#mainform").submit();
    });

    $("input[type=file]").each(function () {
        var input = $(this);
        var name = input.attr("id");
        var btn = $("<input type='button' value='upload'></input>");
        var img = $(".imagepreview");

        input.after(btn);
        input.css("display", "none");

        btn.click(function () {
            input.click();
        });

        img.click(function () {
            input.click();
        });
    });
});