//logging function
window.Log = function () {
    if (this.console) {
        console.log(arguments);
        //console.log(Array.prototype.slice.call(arguments));
    }
};

$(document).ready(function () {

    $(".close").click(function () {
        $(this).parent().fadeOut();
        return false;
    });
    
    try {
        //donations
        $("#supported_list a.close")[0].click(function () {
            $("#supported_list").slideToggle();
            return false;
        });
        $("#profile-bar #total-donated a").click(function () {
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
    } catch (e) {
        Log(e);
    }

    try {
        $('.datepicker').datepicker(); //Initialise any date pickers
    } catch (e) {
        Log(e);
    }

    //uploader
    try {
        $(".imageupload").disableValidation = true;
        $(".imageupload").change(function () {
            $("input").addClass("ignore").disableValidation = true;
            $("form").disableValidation = true;
            $("#mainform").submit();
        });

        $("input[type=file]").each(function () {
            var input = $(this);
            var name = input.attr("id");
            var btn = $("<input type='button' value='upload' class='uploadbutton btn'></input>");
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
    } catch (e) {
        Log(e);
    }
});

