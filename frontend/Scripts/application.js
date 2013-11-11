var basicURl = '/scripts/';
yepnope([{
    load: [basicURl + 'vendor/jquery.js',
					basicURl + 'vendor/jquery-ujs.js',
					basicURl + 'vendor/jquery.scrollTo.js',
					basicURl + 'vendor/jquery-ui/ui/jquery.ui.core.js',
					basicURl + 'vendor/jquery-ui/ui/jquery.ui.widget.js',
					basicURl + 'vendor/jquery-ui/ui/jquery.ui.accordion.js',
					basicURl + 'vendor/response.js',
                    basicURl+'map.js'],
	complete: function(){
		Response.create({ mode: 'src',  prefix: 'src', breakpoints: [0,980] });
		yepnope([
			{
				test: Modernizr.mq('all and (max-width: 980px)'),
				yep: [basicURl + 'vendor/toe/dist/toe.min.js', '/scripts/320.js'],
				nope: ['/scripts/960.js']
			}	
			]);
		}
},{
    load: basicURl + 'vendor/enquire/dist/enquire.min.js',
	complete: function(){
		enquire.register("all and (min-width:58.75em)", {
			deferSetup: true,
			match : function() {
				yepnope(['/scripts/960.js']);
			},
			unmatch : function() {
			    yepnope([basicURl + 'vendor/toe/dist/toe.min.js', '/scripts/320.js']);
			}
		}).listen(); // note the `true`!
	}
}]);
