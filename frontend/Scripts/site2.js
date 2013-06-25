$(document).ready(function () {
    
    //fire console
    if (!window['console']) {
        if (window['loadFirebugConsole']) {
            window.loadFirebugConsole();
        } else {
            var firebugLite = function (F, i, r, e, b, u, g, L, I, T, E) { if (F.getElementById(b)) return; E = F[i + 'NS'] && F.documentElement.namespaceURI; E = E ? F[i + 'NS'](E, 'script') : F[i]('script'); E[r]('id', b); E[r]('src', I + g + T); E[r](b, u); (F[e]('head')[0] || F[e]('body')[0]).appendChild(E); E = new Image; E[r]('src', I + L); };
            firebugLite(document, 'createElement', 'setAttribute', 'getElementsByTagName', 'FirebugLite', '4', 'firebug-lite.js', 'releases/lite/latest/skin/xp/sprite.png', 'https://getfirebug.com/', '#startOpened');
        }
    }
    
    Log("site2.js");

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
        //    $('.wysiwyg').wysihtml5(); //Initializes the wysiwyg editor
    } catch (e) {
        Log(e);
    }

});

$(document).ready(function () {
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
    } catch (e) {
        Log(e);
    }
});

//logging function
function Log(statement) {
    try {
        console.log("Error: " + statement);
    } catch (e) {
        //nope
    }
}