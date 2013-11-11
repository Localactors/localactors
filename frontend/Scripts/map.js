var addresses = new Array();
var geocoder;
var map;
var infowindow = null;
var geocoder = new google.maps.Geocoder();
var center = new google.maps.LatLng(49, 10);
var mapOptions = {
    zoom: 1,
    center: center,
    mapTypeId: google.maps.MapTypeId.ROADMAP
}


function latlong(address,title,url){

    geocoder.geocode( { 'address': address}, function(results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            latlong = results[0].geometry.location;
            var random = 1 + Math.floor(Math.random() * 3);
            var marker = new google.maps.Marker({
                map: map,
                position: latlong,
                icon: "https://s3-eu-west-1.amazonaws.com/localactors-webapp/content/images/googlemap/"+random+".png",
            });

            // Set up markers with info windows
            google.maps.event.addListener(marker, 'click', function() {
                // Close all open infowindows
                if (infowindow) {
                    infowindow.close();
                }

                infowindow = new google.maps.InfoWindow({
                    content: '<a href="'+url+'"><strong>'+title+'</strong></a>'
                });

                infowindow.open(map,marker);
            });
        } else {
            return "Geocode was not successful for the following reason: " + status;
        }
    });
}

$( document ).ready(function() {


    map = new google.maps.Map(document.getElementById("mapgoogle"), mapOptions);
    $("#mapgoogle").css({"width":"100%", "height":"300", "margin-bottom":"8px"});

    $(".cover-projects").each(function( index ) {
        address = $(this).find(".subtitle").text();
        title = $(this).find(".title").text();
        url = $(this).find(".title a").attr('href');
        latlong(address,title,url);

    });





        //set up map options


});