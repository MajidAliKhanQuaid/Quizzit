using Quizzit.Models;
using Quizzit.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;

namespace Quizzit.Controllers
{
   
    public partial class HomeController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LoadQuestionsIfNotExist();
        }
        
        public ActionResult Startup()
        {
            //var questions = db.Questions.ToList();
            var questions = Session["Questions"] as List<QuestionA>;
            // Here Integer Holds Value for Question ID | String accounts for the Answer
            Session["AnsweredQuestions"] = new Dictionary<int, string>();
            
            //Question question = db.Questions.FirstOrDefault();
            QuestionA question = questions.FirstOrDefault();
            if (question == null)
            {
                return Content("There're no questions in the database");
            }
            QuestionA prevQues = questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
            if (prevQues == null)
            {
                question.PrevQuestionID = int.MinValue;
            }
            else
            {
                question.PrevQuestionID = prevQues.ID;
            }
            ViewBag.Question = question;
            return View();
        }

        public ActionResult Index()
        {
            List<SummaryVM> summaries = new List<SummaryVM>();
            var qas = GetAnsweredQuestionsByUserID(USERID);
            foreach (var qa in qas)
            {
                string strQuestId = qa.QuestionsAndAnswers.Substring(0, 5);
                var question = FindQuestion(int.Parse(strQuestId));
                //
                SummaryVM summary = new SummaryVM();
                summary.Question = question.QuestionText;
                summary.Answer = qa.QuestionsAndAnswers.Substring(5);
                summaries.Add(summary);
            }
            return View(summaries);
        }

        [HttpPost]
        public JsonResult LoadPrevious(QuestionFormVM qvm)
        {
            string viewAsString = "";
            // Get Previous
            // Load This Question
            // Search for its previous and return
            var questions = Session["Questions"] as List<QuestionA>;
            var question = questions.Where(x => x.ID == qvm.PrevQuestion).FirstOrDefault();
            if (question != null)
            {
                QuestionA prevQues = questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
                if (prevQues == null)
                {
                    question.PrevQuestionID = int.MinValue;
                }
                else
                {
                    question.PrevQuestionID = prevQues.ID;
                }
                //
                Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
                if (dictQA != null)
                {
                    if (dictQA.ContainsKey(question.ID))
                    {
                        question.Answered = dictQA[question.ID];
                    }
                }
                //
                viewAsString = RenderViewAsString("_QuestionControls", question);
                return Json(new { status = true, view = viewAsString });
            }
            return Json(new { status = false });
        }

        [HttpPost]
        public JsonResult SaveLoadNext(QuestionFormVM qvm)
        {
            string viewAsString = "";
            //
            var questions = Session["Questions"] as List<QuestionA>;
            if (qvm.Answer == null)
            {
                var question = questions.Where(x => x.ID == qvm.QuestionID).FirstOrDefault();
                if (question != null)
                {
                    // Optimize
                    var dbQuestions = GetAllQuestions();
                    var prevQue = dbQuestions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
                    question.PrevQuestionID = (prevQue == null ? int.MinValue : prevQue.ID);
                    question.NextQuestionID = (question.NextQuestionID == null ? int.MinValue : question.NextQuestionID);

                    if (question.QuestionType == (int)QuestionType.Checkbox ||
                    question.QuestionType == (int)QuestionType.Radio ||
                    question.QuestionType == (int)QuestionType.Dropdown)
                    {
                        question.ErrorMessage = "You must have atleast one option selected";
                    }
                    else
                    {
                        question.ErrorMessage = "Answer field can not be blank";
                    }
                    //
                    viewAsString = RenderViewAsString("_QuestionControls", question);
                    return Json(new { status = true, view = viewAsString });
                }
                return Json(new { status = false });
            }
            //
            Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            //        
            if (qvm.NextQuestion == int.MinValue)
            {
                var question = questions.Where(x => x.ID == qvm.QuestionID).FirstOrDefault();
                if (question != null)
                {
                    // ************************************
                    // *********   Save Here    ***********
                    // ************************************
                    if (qvm.Answer != null)
                    {
                        if (dictQA.Keys.Contains(qvm.QuestionID))
                        {
                            dictQA[qvm.QuestionID] = qvm.Answer;
                        }
                        else
                        {
                            dictQA.Add(qvm.QuestionID, qvm.Answer);
                        }
                    }
                    //
                    return Json(new { status = true, url = Url.Action("Summary", "Home") });
                }
            }
            // Saving Answer in the Session variable
            if (qvm.Answer != null)
            {
                Dictionary<int, string> dict = (Session["AnsweredQuestions"] as Dictionary<int, string>);
                if (dict.Keys.Contains(qvm.QuestionID))
                {
                    dict[qvm.QuestionID] = qvm.Answer;
                }
                else
                {
                    dict.Add(qvm.QuestionID, qvm.Answer);
                }
            }
            //
            QuestionA objQuestion = SearchQuestionById(qvm.NextQuestion);
            
            QuestionA prevQues = questions.Where(x => x.NextQuestionID == objQuestion.ID).FirstOrDefault();
            if (prevQues == null)
            {
                objQuestion.PrevQuestionID = int.MinValue;
            }
            else
            {
                objQuestion.PrevQuestionID = prevQues.ID;
            }
            //
            if (objQuestion.NextQuestionID == null)
            {
                objQuestion.NextQuestionID = int.MinValue;
                viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
                return Json(new { status = true, view = viewAsString });
            }
            //
            if (dictQA != null)
            {
                if (dictQA.ContainsKey(objQuestion.ID))
                {
                    objQuestion.Answered = dictQA[objQuestion.ID];
                }
            }
            viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
            return Json(new { status = true, view = viewAsString });
        }
        
        public ActionResult Summary()
        {
            if (Session["AnsweredQuestions"] == null)
            {
                return RedirectToAction("Index");
            }
            //
            List<SummaryVM> summaries = new List<SummaryVM>();
            //
            var qu = GetAllQuestions();
            Dictionary<int, string> qas = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            foreach (var q in qu)
            {
                SummaryVM summary = GetSummaryObjFromDict(q, qas);
                summaries.Add(summary);
            }
            return View(summaries);
        }
        
        public ActionResult Thanks()
        {
            if (Session["AnsweredQuestions"] == null)
            {
                return RedirectToAction("Index");
            }
            //
            return View();
        }

        [HttpPost]
        public JsonResult Save()
        {
            if (Session["AnsweredQuestions"] == null)
            {
                return Json(new { result = false, errorType = "SESSION_EXPIRY", error = "Your session has expired" });
            }

            Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            List<string> Queries = new List<string>();
            Queries.Add(string.Format("DELETE FROM [Quizzit].[dbo].[QuestionAndAnswer] WHERE [QuestionAndAnswer].[UserID] = {0};", 1));
            //
            bool first = true;
            foreach (var item in dictQA)
            {
                if(first == true)
                {
                    Queries.Add(string.Format("INSERT INTO [Quizzit].[dbo].[QuestionAndAnswer]([UserID],[QuestionsAndAnswers]) VALUES({0}, '{1}')", USERID, $"{item.Key.ToString().PadRight(5, ' ')}{item.Value}"));
                    first = false;
                }
                else
                {
                    Queries.Add(string.Format(",({0}, '{1}')", USERID, $"{item.Key.ToString().PadRight(5, ' ')}{item.Value}"));
                }
            }
            //
            if (dictQA.Count > 0)
            {
                ExecuteBulkQueries(Queries);
                return Json(new { result = true });
            }
            return Json(new { result = false, errorType = "NO_DATA", error = "No data was found to be saved" });
        }

    }
}