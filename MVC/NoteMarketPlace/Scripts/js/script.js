function sticky_header() {
    var header_height = jQuery('.site-header').innerHeight() / 2;
    var scrollTop = jQuery(window).scrollTop();;
    if (scrollTop > header_height) {
        jQuery('body').addClass('sticky-header');
    } else {
        jQuery('body').removeClass('sticky-header');
    }
}

jQuery(document).ready(function () {
  sticky_header();
});

jQuery(window).scroll(function () {
  sticky_header();  
});
jQuery(window).resize(function () {
  sticky_header();
});



$(document).ready(function () {

    //$('#University').change(function () {
    //    $('#noteshowDD')[0].submit();
    //}

    $('#University').change(function () {
        // alert("test");
        //   alert($("#University").val());
        $('noteshowDD').attr('action', '/User/SearchNotes/' + $(this).val());
         this.form.submit();
      //  var University = $("#University").val();
      //  $('#noteshowDD').attr('action', $(this).attr('href')).submit();
       

       
     /*   var ajaxData =  $("#University").val();
        
         
        jQuery.ajax({
            type: "POST",
            URL: "@Url.Action("/User/SearchNotes/")",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: ajaxData ,
            success: function (data , text) {
                alert(data);
            },
            failure: function (errMsg) {
                alert(errMsg);
            }
        });*/
       
    });

});











