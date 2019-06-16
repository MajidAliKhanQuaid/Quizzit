function GetRow() {
    var div = document.createElement("div");
    div.setAttribute("class", "col-md-12");
    return div;
}

function GetNextQuestion() {
    var questnId = document.getElementById("QuestionID").value;
    var nextQuestId = document.getElementById("NextQuestion").value;
    var answer = document.getElementById("Answer").value;
    $.ajax({
        url: "/Home/AjaxSave",
        method: "POST",
        data: { QuestionID: questnId, NextQuestion: nextQuestId, Answer: answer },
        dataType: "html",
        success: function (response) {
            $("._placeholder").html(response);
            $("#btnQuestion").click(function () {
                GetNextQuestion();
            });
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