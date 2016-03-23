/*global $ */
$(document).ready(function () {

    

    "use strict";

    $('.menu > ul > li:has( > ul)').addClass('menu-dropdown-icon');
    //Checks if li has sub (ul) and adds class for toggle icon - just an UI

    $('.menu > ul > li > ul > li:has( > ul)').addClass('menu-dropdown-icon');



    //$('.menu > ul > li > ul:not(:has(ul))').addClass('normal-sub');
    //Checks if drodown menu's li elements have anothere level (ul), if not the dropdown is shown as regular dropdown, not a mega menu (thanks Luka Kladaric)

    $(".menu > ul").before("<a href=\"#\" class=\"menu-mobile\"><i class=\"fa fa-book fa-lg fa-2x\"></i> Каталог</a>");

    //Adds menu-mobile class (for mobile toggle menu) before the normal menu
    //Mobile menu is hidden if width is more then 959px, but normal menu is displayed
    //Normal menu is hidden if width is below 959px, and jquery adds mobile menu
    //Done this way so it can be used with wordpress without any trouble

    $(".menu > ul > li").hover(function (e) {
        if ($(window).width() > 943) {
            $(this).children("ul").stop().fadeIn(150);
        }
    },
    function (e) {
        if ($(window).width() > 943) {
            $(this).children("ul").stop().fadeOut(150);
        }
    });
    //If width is more than 943px dropdowns are displayed on hover

    $(".menu > ul > li").click(function (e) {
        if ($(window).width() > 943) {
        }
    });

    $(".menu > ul > li").click(function (e) {
        if ($(window).width() <= 943) {
            $(this).children("ul").stop().fadeToggle(150);
            if ($(this).children().length>1) {
                e.preventDefault();
            }
            e.stopPropagation();
        }
    });

    $(".menu > ul > li > ul > li").click(function (e) {
        if ($(window).width() <= 943) {
            $(this).children("ul").stop().fadeToggle(150);
            e.stopPropagation();
        }
    });


    //If width is less or equal to 943px dropdowns are displayed on click (thanks Aman Jain from stackoverflow)

    $(".menu-mobile").click(function (e) {
        $(".menu > ul").toggleClass('show-on-mobile');
        e.preventDefault();
    });
    //when clicked on mobile-menu, normal menu is shown as a list, classic rwd menu story (thanks mwl from stackoverflow)

});
