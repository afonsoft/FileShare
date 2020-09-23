"use strict";

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
}

function UploadSubimit(oFormElement, action) {
    var formData = new FormData(oFormElement);
    $.ajax({
        url: action,
        headers: { 'RequestVerificationToken': getCookie('RequestVerificationToken') },
        type: 'POST',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        enctype: 'multipart/form-data',

        xhr: function () {
            //upload Progress
            var xhr = $.ajaxSettings.xhr();

            xhr.upload.onprogress = function (event) {
                var percent = 0;
                var position = event.loaded || event.position;
                var total = event.total;
                if (event.lengthComputable) {
                    percent = Math.ceil(position / total * 100);
                }
                //update progressbar
                $("#progress-bar").css("width", + percent + "%");
                $("#status").text(percent + "%");
            }

            return xhr;
        },
        success: function (data) {       
            alert(data);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.status + '-' + xhr.statusText);
            alert(thrownError);
        }
    });
    return false;
};


function inputFileTemplate() {
    $(".input-file").before(
        function () {
            if (!$(this).prev().hasClass('input-ghost')) {
                var element = $("<input type='file' class='input-ghost' style='visibility:hidden; height:0'>");
                element.attr("name", $(this).attr("name"));
                element.change(function () {
                    element.next(element).find('input').val((element.val()).split('\\').pop());
                });
                $(this).find("button.btn-choose").click(function () {
                    element.click();
                });
                $(this).find("button.btn-reset").click(function () {
                    element.val(null);
                    $(this).parents(".input-file").find('input').val('');
                });
                $(this).find('input').css("cursor", "pointer");
                $(this).find('input').mousedown(function () {
                    $(this).parents('.input-file').prev().click();
                    return false;
                });
                return element;
            }
        }
    );
}

$(document).ready(function () {
    inputFileTemplate();
    $(".adsbygoogle").each(function () { (adsbygoogle = window.adsbygoogle || []).push({}); });
});