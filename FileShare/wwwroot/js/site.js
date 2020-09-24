"use strict";

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
}
function setCookie(cname, cvalue) {
    var d = new Date();
    d.setTime(d.getTime() + (1 * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}


function inputFileTemplate() {
    $(".custom-file-input").on("change", function () {
        var fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    });
}

function googleAdsense() {
    $(".adsbygoogle").each(function () { (adsbygoogle = window.adsbygoogle || []).push({}); });
}

function showAlertDanger(message) {
    $("#alertText").html(message);
    $("#alertShow").show();
}

$(document).ready(function () {
    inputFileTemplate();
    googleAdsense();
});

function UploadSubimit() {
    var formData = new FormData(document.getElementById('uploadForm'));
    $("#progress").show();
    $.ajax({
        url: 'Streaming/UploadFileStream',
        headers: {
            'RequestVerificationToken': getCookie('RequestVerificationToken'),
            'X-CSRF-TOKEN-REQUEST': document.getElementById('RequestVerificationToken')
        },
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
                $("#progressbar").css("width", + percent + "%");
                $("#progressbar").html(percent + "%");
            }
            return xhr;
        },
        success: function (data) {      
            $("#progress").hide();
            window.location.href = '/download/' + data;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            $("#progress").hide();
            console.log(xhr.status + ' - ' + xhr.statusText + ' - ' + xhr.responseText);
            var output = '';
            for (var entry in xhr.responseJSON) {
                output += entry + ' - ' + xhr.responseJSON[entry] + '<br/>';
            }
            console.log(output);
            showAlertDanger("Error in file upload")
            bootbox.alert(xhr.status + ' - ' + xhr.statusText + '<br/>' + output);
        }
    });
    return false;
};
