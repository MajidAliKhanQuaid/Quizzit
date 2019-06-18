using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


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