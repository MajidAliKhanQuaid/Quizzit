using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quizzit.Models
{
    public class QuestionA
    {
        public int ID { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType { get; set; }
        public Nullable<int> NextQuestionID { get; set; }
        public string Answered { get; set; }
        public int PrevQuestionID { get; set; }
        public string ErrorMessage { get; set; }
        public List<QuestionAnswerA> QAs { get; set; }
        public QuestionA()
        {
            QAs = new List<QuestionAnswerA>();
        }
    }
}