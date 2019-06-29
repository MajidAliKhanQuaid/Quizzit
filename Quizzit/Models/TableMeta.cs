using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quizzit.Models
{
    public class QuestionAndAnswerFields
    {
        public static string ID { get; set; } = "ID";
        public static string UserID { get; set; } = "UserID";
        public static string QuestionsAndAnswers { get; set; } = "QuestionsAndAnswers";
        // Fields from [Question] FOREIGN KEY
        public static string _ID { get; set; } = "_ID";
        public static string _QuestionText { get; set; } = "_QuestionText";
        public static string _QuestionType { get; set; } = "_QuestionType";
        public static string _NextQuestionID { get; set; } = "_NextQuestionID";
    }

    public class QuestionAnswerFields
    {
        public static string ID { get; set; } = "ID";
        public static string QuestionID { get; set; } = "QuestionText";
        public static string AnswerText { get; set; } = "QuestionType";
        public static string NextQuestionID { get; set; } = "NextQuestionID";
    }

    public class QuestionFields
    {
        public static string ID { get; set; } = "ID";
        public static string QuestionText { get; set; } = "QuestionText";
        public static string QuestionType { get; set; } = "QuestionType";
        public static string NextQuestionID { get; set; } = "NextQuestionID";
        // Fields from [QuestionAnswer] FOREIGN KEY
        public static string _ID { get; set; } = "_ID";
        public static string _QuestionID { get; set; } = "_QuestionID";
        public static string _AnswerText { get; set; } = "_AnswerText";
        public static string _NextQuestionID { get; set; } = "_NextQuestionID";
        // Not Mapped Field
        public static string Answered { get; set; } = "Answered";
        public static string PrevQuestionID { get; set; } = "PrevQuestionID";
        public static string ErrorMessage { get; set; } = "ErrorMessage";
    }

}