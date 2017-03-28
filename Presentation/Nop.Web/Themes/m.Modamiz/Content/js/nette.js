$(function(){
	
	var $window = $(window);
	
	$(".sidebar-icon").click(function () {
		if ($(this).hasClass("sidebar-icon-closed")) {
		$(this).removeClass('sidebar-icon-closed');
		} else { 
		$(this).addClass('sidebar-icon-closed');
		}
		$(".sidebar").stop(true , true).slideToggle();
		return false;
	});
	
	$(".tab-icon").click(function () {
		if ($(this).hasClass("sidebar-icon-closed")) {
		$(this).removeClass('sidebar-icon-closed');
		} else { 
		$(this).addClass('sidebar-icon-closed');
		}
		$(".tab").stop(true , true).slideToggle();
		return false;
	});
	
	$('.sidebar-scroll').slimScroll({
		color: '#999',
		size: '6px',
		height: '175px',
		railColor: '#000',
		railOpacity: 0.07,
		railVisible: true,
		alwaysVisible: false,
		touchScrollStep: 40
	});
	
	$('.product-text-scroll').slimScroll({
		color: '#999',
		size: '6px',
		height: '86px',
		railColor: '#000',
		railOpacity: 0.07,
		railVisible: true,
		alwaysVisible: false,
		touchScrollStep: 40
	});
	
	$(".top-search-icon").click(function () {
		
		
		var div = $(".top-search");		
		$(".top-opened").not(div).stop(true , true).slideUp().removeClass("top-opened");		
		if(div.hasClass("top-opened")){div.removeClass("top-opened"); }else {div.addClass("top-opened")};
		
		if ($(this).hasClass("top-search-close")) {
			$(this).removeClass('top-search-close');
		} else { 
			$(this).addClass('top-search-close');
		}
		
		$(".top-search").stop(true , true).slideToggle();
		return false;
	});
	
	$(".top-avatar").click(function () {
		
		var div = $(".top-menu");		
		$(".top-opened").not(div).stop(true , true).slideUp().removeClass("top-opened");
		$(".top-search-close").removeClass("top-search-close");
		if(div.hasClass("top-opened")){div.removeClass("top-opened"); }else {div.addClass("top-opened")};
		
		$(".top-menu").stop(true , true).slideToggle();
		return false;
	});
	
	$(".nav-icon").click(function () {
		var div = $("#nav");		
		$(".top-opened").not(div).stop(true , true).slideUp().removeClass("top-opened");
		$(".top-search-close").removeClass("top-search-close");
		if(div.hasClass("top-opened")){div.removeClass("top-opened"); }else {div.addClass("top-opened")};
		
		$("#nav").stop(true , true).slideToggle();
		return false;
	});
	
	$(".icon-filter").click(function () {
		$(".sidebar-wrap").stop(true , true).slideToggle();
		return false;
	});
	
	$(".cat-sort-icon").click(function () {
		$(".cat-sort").stop(true , true).slideToggle();
		return false;
	});

});

$(document).ready(function() {
	$(".sidebar-toggle").click(function(){
		
		$(this).parent().children('.sidebar-content').slideToggle();
		$(this).toggleClass('sidebar-closed')
	});
});

//$(document).ready(function() {
	
//	$('.fancybox').fancybox();
	
//	$("input[type=checkbox],input[type=radio]").iCheck({
//		checkboxClass: 'icheckbox_minimal-pink',
//		radioClass: 'iradio_minimal-pink',
//	});
	
//});

$(document).ready(function(){
		
	$('.slider-product').slick({
		arrows:false,
		dots:true
	});
	
	$('.main-slider').slick({
		arrows:false,
		dots:true,
		pauseOnHover:true,
		autoplaySpeed:3000,
		autoplay:true
	});
	
	$('.slider-selected').slick({
		dots:false
	});
	
});

$(document).ready(function(){
	$('div#nav ul li.nav-multi > a').click(function(event){
		
		event.preventDefault();
		$(this).parent().parent().children('li').children('span').not($(this).parent().children("span")).slideUp();
		$(this).parent().children('span').stop(true , true).slideToggle();
		return false;
		
	})
});

$(document).ready(function() {
    function close_accordion_section() {
        $('.accordion .accordion-section-title').removeClass('active');
        $('.accordion .accordion-section-content').slideUp(300).removeClass('open');
    }
 
    $('.accordion-section-title').click(function(e) {
        // Grab current anchor value
        var currentAttrValue = $(this).attr('href');
 
        if($(e.target).is('.active')) {
            close_accordion_section();
        }else {
            close_accordion_section();
 
            // Add active class to section title
            $(this).addClass('active');
            // Open up the hidden content panel
            $('.accordion ' + currentAttrValue).slideDown(300).addClass('open'); 
        }
 
        e.preventDefault();
    });
});