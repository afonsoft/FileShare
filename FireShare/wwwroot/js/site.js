"use strict";
async function AJAXSubmit(oFormElement) {
    const formData = new FormData(oFormElement);
    try {
        const response = await fetch(oFormElement.action, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getCookie('RequestVerificationToken')
            },
            body: formData
        });
        console.error('Result:', response.status + ' ' + response.statusText);
    } catch (error) {
        console.error('Error:', error);
    }
}
function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
}

 function UploadSubimit(e) {
    e.preventDefault();
     var formData = new FormData(oFormElement);
    $.ajax({
        url: oFormElement.action,
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
            if (xhr.upload) {
                xhr.upload.addEventListener('progress', function (event) {
                    var percent = 0;
                    var position = event.loaded || event.position;
                    var total = event.total;
                    if (event.lengthComputable) {
                        percent = Math.ceil(position / total * 100);
                    }
                    //update progressbar
                    $(".progress-bar").css("width", + percent + "%");
                    $(".status").text(percent + "%");
                }, true);
            }
            return xhr;
        },
        success: function (mdata) {
            if (mdata == 0) {
                alert("Done");
            }

        },
    });
};