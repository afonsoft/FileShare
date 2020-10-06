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

function getFileSizeString(fileSizeInBytes) {
    var i = -1;
    var byteUnits = [' kB', ' MB', ' GB', ' TB', 'PB', 'EB', 'ZB', 'YB'];
    do {
        fileSizeInBytes = fileSizeInBytes / 1024;
        i++;
    } while (fileSizeInBytes > 1024);

    return Math.max(fileSizeInBytes, 0.1).toFixed(1) + byteUnits[i];
};

function FormatJsonDateToJavaScriptDate(value) {
    var dt = new Date(value);
    return (dt.getMonth() + 1) + "/" + dt.getDate() + "/" + dt.getFullYear();
}

function inputFileTemplate() {
    $(".custom-file-input").on("change", function () {
        var fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    });
}


function googleAdsense(count) {
    try {
        console.log("googleAdsense count " + count);
        $(".adsbygoogle").each(function () {
            (adsbygoogle = window.adsbygoogle || []).push({});
        });
    } catch{
        if (count == 4)
            return;
        count++;
        setTimeout(googleAdsense(count), 1000);
    }
}

function showAlertDanger(message) {
    $("#alertText").html(message);
    $("#alertShow").show();
}

$(document).ready(function () {
    inputFileTemplate();
    countDown();
    setTimeout(googleAdsense(1), 100);
});

function countDown() {
    $("#btnDownload").hide();
    $("#countdown").countdown360({
        radius: 60,
        seconds: 60,
        autostart: true,
        onComplete: function () {
            $("#countdown").hide();
            $("#btnDownload").removeAttr("disabled");
            $("#btnDownload").show();
        }
    });
}

function UploadSubimit() {
    var formData = new FormData(document.getElementById('uploadForm'));
    $("#progress").show();
    $("#alertShow").hide();
    $("#progressbar").css("width","0%");
    $("#progressbar").html("0%");
    $.ajax({
        url: 'Streaming/UploadFileStream',
        headers: {
            'RequestVerificationToken': document.getElementById('RequestVerificationToken'),
            'X-CSRF-TOKEN-REQUEST': document.getElementById('RequestVerificationToken')
        },
        type: 'POST',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        enctype: 'multipart/form-data',
        timeout: 5 * 60 * 1000,
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
            console.log(thrownError);
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
