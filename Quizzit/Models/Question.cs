//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quizzit.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Question
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Question()
        {
            this.QuestionAnswers = new HashSet<QuestionAnswer>();
        }

        public int ID { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType { get; set; }
        public Nullable<int> NextQuestionID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; }
    }

    public partial class Question
    {
        [NotMapped]
        public string Answered { get; set; }

        [NotMapped]
        public int PrevQuestionID { get; set; }

        [NotMapped]
        public string ErrorMessage { get; set; }
    }
}
