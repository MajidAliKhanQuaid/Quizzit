using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Quizzit.ViewModel
{
    public class QuestionFormVM
    {
        public int PrevQuestion { get; set; }
        public int QuestionID { get; set; }
        public int NextQuestion { get; set; }
        [Required]
        public string Answer { get; set; }
    }
}