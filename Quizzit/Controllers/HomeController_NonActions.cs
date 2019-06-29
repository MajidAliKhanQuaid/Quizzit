using Quizzit.Models;
using Quizzit.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Quizzit.Controllers
{

    public partial class HomeController : Controller
    {
        static int USERID = 1;

        [NonAction]
        private void LoadQuestionsIfNotExist()
        {
            if (Session["Questions"] == null)
            {
                var questions = GetAllQuestions();
                Session["Questions"] = questions;
            }
        }

        [NonAction]
        private QuestionAnswerA SearchInQuestionAnswer(int questionId)
        {
            var dbQuestions = GetAllQuestions();
            var nonExceptionQues = dbQuestions.Where(x => x.QuestionType < 4).ToList();
            foreach (var question in dbQuestions)
            {
                question.QAs.Where(x => x.QuestionID == questionId).FirstOrDefault();
            }
            return null;
        }

        [NonAction]
        private QuestionA SearchQuestionById(int qid)
        {
            //return db.Questions.Find(qid);
            var dbQuestions = Session["Questions"] as List<QuestionA>;
            var question = dbQuestions.Where(x => x.ID == qid).First();

            // New Clients Modification
            // On Checkbox and Radio load next from QuestionAnswers table

            if (question.QuestionType == (int)QuestionType.Radio || question.QuestionType == (int)QuestionType.Dropdown)
            {
                //*****************************
                // This Logic Seems Meaning Less
                //*****************************

                var options = Session["Options"] as List<QuestionAnswerA>;
                var lastQuest = options.Where(x => x.QuestionID == question.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                if (lastQuest != null)
                {
                    question.NextQuestionID = lastQuest.NextQuestionID;
                }
            }
            else
            {
                if (question.NextQuestionID == null)
                {
                    //*****************************
                    // This Logic Seems Meaning Less
                    //*****************************
                    //var lastQuest = db.QuestionAnswers.Where(x => x.QuestionID == question.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                    //if (lastQuest != null)
                    //{
                    //    question.NextQuestionID = lastQuest.NextQuestionID;
                    //}
                    var options = Session["Options"] as List<QuestionAnswerA>;
                    var lastQuest = options.Where(x => x.QuestionID == question.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                    if (lastQuest != null)
                    {
                        question.NextQuestionID = lastQuest.NextQuestionID;
                    }
                }
            }
            //
            return question;
        }

        [NonAction]
        private string RenderViewAsString(string viewName, object model)
        {
            string viewAsString = "";

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
            PartialViewResult pvr = PartialView(viewName, model);
            ViewDataDictionary dDict = new ViewDataDictionary(model);
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                using (HtmlTextWriter markupWriter = new HtmlTextWriter(writer))
                {
                    var viewContext = new ViewContext(ControllerContext, viewResult.View, dDict, new TempDataDictionary(), writer);
                    viewResult.View.Render(viewContext, writer);
                    viewAsString = sb.ToString();
                }
            }
            return viewAsString;
        }

        [NonAction]
        private int GetUserID()
        {
            char[] numbers = Guid.NewGuid().ToString().Where(x => Char.IsDigit(x)).ToArray();
            string UserID = string.Join("", numbers);
            if (UserID.Length > 9)
            {
                UserID = UserID.Substring(0, 9);
            }
            return Convert.ToInt32(UserID);
        }

        [NonAction]
        private SummaryVM GetSummaryObj(QuestionA question, List<QuestionAndAnswerA> qas)
        {
            SummaryVM summary = new SummaryVM();
            summary.Question = question.QuestionText;
            foreach (var item in qas)
            {
                if (string.IsNullOrWhiteSpace(item.QuestionsAndAnswers) == false)
                {
                    string srQuesID = item.QuestionsAndAnswers.Substring(0, 4);
                    int QuesID = int.Parse(srQuesID);
                    if (QuesID == question.ID)
                    {
                        summary.Answer = item.QuestionsAndAnswers.Substring(5);
                        break;
                    }
                }
            }
            if (summary.Answer == null)
            {
                summary.Answer = "N/A";
            }
            return summary;
        }

        [NonAction]
        private SummaryVM GetSummaryObjFromDict(QuestionA question, Dictionary<int, string> qas)
        {
            SummaryVM summary = new SummaryVM();
            summary.Question = question.QuestionText;
            if (qas.ContainsKey(question.ID))
            {
                summary.Answer = qas[question.ID];
            }
            return summary;
        }

        //[NonAction]
        //private Tuple<bool, List<ValidationResult>> Validate<T>(T obj)
        //{
        //    var context = new ValidationContext(obj, serviceProvider: null, items: null);
        //    var validationResults = new List<ValidationResult>();
        //    bool isValid = Validator.TryValidateObject(obj, context, validationResults, true);
        //    return new Tuple<bool, List<ValidationResult>>(isValid, validationResults);
        //}
    }

}