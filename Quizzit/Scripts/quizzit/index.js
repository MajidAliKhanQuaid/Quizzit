function GetRow() {
    var div = document.createElement("div");
    div.setAttribute("class", "col-md-12");
    return div;
}

function GetPrevQuestion() {
    var prevQuestId = document.getElementById("PrevQuestion").value;
    var questnId = document.getElementById("QuestionID").value;
    var nextQuestId = document.getElementById("NextQuestion").value;
    var answer = document.getElementById("Answer").value;
    $.ajax({
        url: "/Home/LoadPrevious",
        method: "POST",
        data: { PrevQuestion: prevQuestId, QuestionID: questnId, NextQuestion: nextQuestId, Answer: answer },
        dataType: "html",
        success: function (response) {
            response = JSON.parse(response);
            debugger;
            if (response.status === true) {
                if (response.view) {
                    $("._placeholder").html(response.view);
                    $("#btnBack").click(function () {
                        GetPrevQuestion();
                    });
                    $("#btnQuestion").click(function () {
                        GetNextQuestion();
                    });
                }
                return;
            }
            alert("Request could not be processed !");
        },
        error: function (error) {

        }
    })
}

function UploadAttachments() {
    var attach = document.getElementsByClassName("attachments")[0];
    var files = attach.files;
    if (files) {
        var answer = document.getElementById("Answer");
        answer.value = '';
        var ul = document.getElementById("fileNames");
        ul.innerHTML = "";
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            // check _upload.js
            var upload = new Upload(file, i);
            // execute upload
            upload.doUpload();
            //
            if (i > 0) {
                answer.value += ";";
            }
            answer.value += file.name;
        }
        console.log(`${files.length} files were uploaded`);
    }
    return true;
}

function GetNextQuestion() {
    // Get Files Attachements
    // Upload them one by one
    var x = document.getElementById("UploadingStatus");
    if (x) {
        if (x.value === "PENDING") {
            UploadAttachments();
        }
    }
    //
    var prevQuestId = document.getElementById("PrevQuestion").value;
    var questnId = document.getElementById("QuestionID").value;
    var nextQuestId = document.getElementById("NextQuestion").value;
    var answer = document.getElementById("Answer").value;
    //
    $.ajax({
        url: "/Home/SaveLoadNext",
        method: "POST",
        data: { PrevQuestion: prevQuestId, QuestionID: questnId, NextQuestion: nextQuestId, Answer: answer },
        dataType: "html",
        success: function (response) {
            response = JSON.parse(response);
            debugger;
            if (response.status === true) {
                if (response.view) {
                    $("._placeholder").html(response.view);
                    $("#btnBack").click(function () {
                        GetPrevQuestion();
                    });
                    $("#btnQuestion").click(function () {
                        GetNextQuestion();
                    });
                }
                else {
                    window.location.href = response.url;
                }
                return;
            }
            alert("Request could not be processed !");
        },
        error: function (error) {

        }
    })
}

$(document).ready(function () {
    $("#btnQuestion").click(function () {
        GetNextQuestion();
    });
});