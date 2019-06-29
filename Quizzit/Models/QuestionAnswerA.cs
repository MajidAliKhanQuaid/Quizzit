using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quizzit.Models
{
    public class QuestionAnswerA
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public string AnswerText { get; set; }
        public Nullable<int> NextQuestionID { get; set; }
    }
}