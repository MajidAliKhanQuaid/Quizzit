function GetRow() {
    var div = document.createElement("div");
    div.setAttribute("class", "col-md-12");
    return div;
}

function GetNextQuestion() {
    var nextQuestId = document.getElementById("").getAttribute("next-question-id");
    $.ajax({
        
        url: "script.php",
        method: "POST",
        data: { questionId: nextQuestId },
        dataType: "html",
        success: function (response) {
            if (response.status === true) {
                return;
            }
        },
        error: function (error) {

        }
    })
}