using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quizzit.Models
{
    public class QuestionAndAnswerA
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string QuestionsAndAnswers { get; set; }
        public QuestionA Question { get; set; }
    }
}