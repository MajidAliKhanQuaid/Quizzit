var Upload = function (file) {
    this.file = file;
};

Upload.prototype.getType = function () {
    return this.file.type;
};
Upload.prototype.getSize = function () {
    return this.file.size;
};
Upload.prototype.getName = function () {
    return this.file.name;
};
Upload.prototype.doUpload = function () {
    var that = this;
    var formData = new FormData();
    //
    var qId = document.getElementById("QuestionID").value;
    //
    // add assoc key values, this will be posts values
    formData.append("file", this.file, this.getName());
    formData.append("question_id", qId);

    $.ajax({
        type: "POST",
        url: "script",
        xhr: function () {
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) {
                myXhr.upload.addEventListener('progress', that.progressHandling, false);
            }
            return myXhr;
        },
        success: function (data) {
            console.log("One file is uploaded");
            // your callback here
        },
        error: function (error) {
            // handle error
        },
        async: false,
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        timeout: 60000
    });
};

Upload.prototype.progressHandling = function (event) {
    var percent = 0;
    var position = event.loaded || event.position;
    var total = event.total;
    //var progress_bar_id = "#progress-wrp";
    if (event.lengthComputable) {
        percent = Math.ceil(position / total * 100);
    }
    console.log(`Uploading :  ${percent} +  %`);
    //// update progressbars classes so it fits your code
    //$(progress_bar_id + " .progress-bar").css("width", +percent + "%");
    //$(progress_bar_id + " .status").text(percent + "%");
};