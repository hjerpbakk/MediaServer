// Write your JavaScript code.
// TODO: Dette kan gjøres serverside for å slippe ventingen på rendring
$(document).ready(function() {
  $('li.active').removeClass('active');
  $('a[href*="' + location.pathname.split('/')[2] + '"]').closest('li').addClass('active'); 
});