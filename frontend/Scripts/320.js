$(window).bind('resize', function(e)
	{
	    window.resizeEvt;
	    $(window).resize(function()
	    {
	        clearTimeout(window.resizeEvt);
	        window.resizeEvt = setTimeout(function()
	        {
	        	fitMap();
						$( "#faq_accordion" ).accordion( "refresh" );
	        }, 250);
	    });
	});
	
	
	$( "input#mail_submit" ).click(function(event){
		event.preventDefault();
		email = $("input#email").val();
		list = $("input#mail_list").val();
		$.ajax({
			data: {email: email, list: list, boolean: true},
			dataType: "html",
			url: "http://thedoersproject.com/mailinglist/subscribe",
			type: "post",
			success: function(response){
					switch (response){
						case "1":
							showFlashMessage("success","success");
						break;
						case "Some fields are missing.":
							showFlashMessage(response,"error");
						break;
						case "Invalid email address.":
							showFlashMessage(response,"error");
						break;
						case "Already subscribed.":
							showFlashMessage(response,"notice");
						break;
						
					}
				},
			error: function(jqXHR, textStatus, errorThrown){
				
					console.log(textStatus)
					
				}
					
			
		});
	});
	
	
	function showFlashMessage(msg, type){
		flushFlashMessages();
		var form = $('form#mail_form')
		if("notice"==type){
		 $('<div class="'+type+'"><div class="box"><span class="symbol"><i>V</i></span><span class="msg">'+msg+'</span></div></div>').hide().prependTo(form).fadeIn();
	 }else if("success"==type){
		 $('<div class="'+type+'"><div class="box"><span class="symbol"><i>V</i></span><span class="msg">'+msg+'</span></div></div>').hide().prependTo(form).fadeIn();
	 }else if("error"==type){
		 $('<div class="'+type+'"><div class="box"><span class="symbol"><i>V</i></span><span class="msg">'+msg+'</span></div></div>').hide().prependTo(form).fadeIn();
	 }	
	}
	
	function flushFlashMessages(){
		$('form#mail_form .notice').remove();
		$('form#mail_form .error').remove();
		$('form#mail_form .success').remove();
	}	
	

var subscribe = $("a.subscribe");
var ask = $("a.ask");
var submit = $("a.submit");

subscribe.click(function(event) {
	event.preventDefault();
	$(window).scrollTo( $("#mailing-list"),600);
	$( "#sections_accordion" ).accordion('activate' , 0);
});

ask.click(function(event) {
	event.preventDefault();
	$(window).scrollTo( $("#faq"),600);
	$( "#sections_accordion" ).accordion('activate' , 1);
});

submit.click(function(event) {
	event.preventDefault();
	$(window).scrollTo( $("#submit-project"),600);
	$( "#sections_accordion" ).accordion('activate' , 2);
});

var i=0;
function changeLanguage (arLanguages)
{
	$(arLanguages).hide();
	
	if(arLanguages.length*.5 == i){
		i=0;
	};
	var item = arLanguages[i];
	$(item).fadeIn();
	i++;
}

function fitMap() {
	var slides_width =  $('#slides').css("width");
	$('#slides ul li').css("width",slides_width);
	var slides_height =  $('#slides ul li:first-child').css("height");
	$('#slides').css('height',slides_height);
}

$(document).ready(function() {
	$( "#sections_accordion" ).accordion({ header: "header", collapsible: true, heightStyle: "content", active: false  });
	$( "#faq_accordion" ).accordion({ header: "h3", collapsible: true, heightStyle: "content", active: false  });
	
	var step1 = $( "li.step1" );
	var step2 = $( "li.step2" );
	var step3 = $( "li.step3" );
	
	arSteps = [step1,step2,step3];
	
	var slide1 = $( "li.slide1" );
	var slide2 = $( "li.slide2" );
	var slide3 = $( "li.slide3" );
	
	arSlides = [slide1,slide2,slide3];
	
	var right_arrow = $( "#arrow_right" );
	var left_arrow = $( "#arrow_left" );
	
	var current = 0
	
	var objLanguages =  $('#map .banner-call ul li')
	var arLanguages = $.makeArray(objLanguages);
	changeLanguage (arLanguages);
	setInterval (function() { changeLanguage(arLanguages)}, 3000 );
	
	
	
	
	fitMap();
	showCurrentSlide(current);
	showCurrentStep(current);
	hideLeft();
	
	
	right_arrow.click(function(){
		current++;
		checkLimit(current);
		showCurrentSlide(current);
		hideAllSteps();
		showCurrentStep(current);
		left_arrow.css('visibility', 'visible');
});

$('#slides').on('swipe', function (event) {
		if(event.direction === 'right') { // or right, down, left
			current--;
			checkLimit(current);
			showCurrentSlide(current);
			hideAllSteps();
			showCurrentStep(current);
			right_arrow.css('visibility', 'visible');
		}
		if(event.direction === 'left') { // or right, down, left
			current++;
			checkLimit(current);
			showCurrentSlide(current);
			hideAllSteps();
			showCurrentStep(current);
			left_arrow.css('visibility', 'visible');
		}
});

left_arrow.click(function(){
	current--;
	checkLimit(current);
	showCurrentSlide(current);
	hideAllSteps();
	showCurrentStep(current);
	right_arrow.css('visibility', 'visible');
});

function checkLimit(reached){
	if (reached <= 0){
		current = 0
		hideLeft();
		hideRest(current,1);
	}

	if (reached == 1){
		hideRest(current,2);
	}

	if (reached == arSlides.length-1){
		current = arSlides.length-1;
		hideRight();		
	}
}


function hideRest(current,offset){
	for (var i=offset;i<arSlides.length;i++){
		(function(slide) {
			if (current != slide){
				slide.fadeOut("slow");
			}
		})(arSlides[i]);
	}
}

function hideLeft(){
	left_arrow.css('visibility', 'hidden');
	left_arrow.click(false);
}

function hideRight(){
	right_arrow.css('visibility', 'hidden');
	right_arrow.click(false);
}


function showCurrentSlide(current){
	arSlides[current].delay(500).fadeIn("slow");
}

function hideAllSteps(){
	for (var i=0;i<arSlides.length;i++){
		(function(step) {
			step.fadeOut("fast");
		})(arSteps[i]);
	}
}

function showCurrentStep(current){
	arSteps[current].delay(500).fadeIn("slow");
}

});
