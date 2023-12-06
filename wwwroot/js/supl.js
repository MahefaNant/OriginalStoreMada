$(document).ready(function() {
    $('.dropdown-hover').hover(function() {
        if (!$(this).hasClass('show')) {
            $(this).addClass('show');
            $(this).siblings('.dropdown-menu').addClass('show');
        }
    }, function() {
        if ($(this).hasClass('show')) {
            $(this).removeClass('show');
            $(this).siblings('.dropdown-menu').removeClass('show');
        }
    });
});