using Quizzit.Models;
using Quizzit.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                var questions = db.Questions.ToList();
                Session["Questions"] = questions;
            }
        }

        [NonAction]
        private Question SearchQuestionById(int qid)
        {
            //return db.Questions.Find(qid);

            var question = db.Questions.Find(qid);

            // New Clients Modification
            // On Checkbox and Radio load next from QuestionAnswers table

            //if (question.QuestionType == (int)QuestionType.Checkbox || question.QuestionType == (int)QuestionType.Radio || question.QuestionType == (int)QuestionType.Dropdown)
            if (question.QuestionType == (int)QuestionType.Radio || question.QuestionType == (int)QuestionType.Dropdown)
            {
                var lastQuest = db.QuestionAnswers.Where(x => x.QuestionID == question.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                if(lastQuest != null)
                {
                    question.NextQuestionID = lastQuest.NextQuestionID;
                }
            }
            else
            {
                if(question.NextQuestionID == null)
                {
                    var lastQuest = db.QuestionAnswers.Where(x => x.QuestionID == question.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                    if (lastQuest != null)
                    {
                        question.NextQuestionID = lastQuest.NextQuestionID;
                    }
                }
            }
            //
            return question;
        }

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
        private SummaryVM GetSummaryObj(Question question, List<QuestionAndAnswer> qas)
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
        private SummaryVM GetSummaryObjFromDict(Question question, Dictionary<int, string> qas)
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

    public partial class HomeController : Controller
    {
        QuizzitEntities db = new QuizzitEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LoadQuestionsIfNotExist();
        }

        public ActionResult Startup()
        {
            var questions = db.Questions.ToList();
            Session["Questions"] = questions;
            // Here Integer Holds Value for Question ID | String accounts for the Answer
            Session["AnsweredQuestions"] = new Dictionary<int, string>();

            /*
             USERID = GetUserID(); 
             */

            //Question question = db.Questions.FirstOrDefault();
            Question question = (Session["Questions"] as List<Question>).FirstOrDefault();
            if (question == null)
            {
                return Content("There're no questions in the database");
            }
            Question prevQues = db.Questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
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
            var qas = db.QuestionAndAnswers.Where(x => x.UserID == USERID).ToList();
            foreach (var qa in qas)
            {
                string strQuestId = qa.QuestionsAndAnswers.Substring(0, 5);
                var question = db.Questions.Find(int.Parse(strQuestId));
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

            var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.PrevQuestion);
            if (question != null)
            {
                Question prevQues = db.Questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
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
                //return PartialView("_QuestionControls", question);
            }
            return Json(new { status = false });
            //
            //viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
            //return Json(new { status = true, view = viewAsString });
            //return PartialView("_QuestionControls", objQuestion);
        }

        [HttpPost]
        public JsonResult SaveLoadNext(QuestionFormVM qvm)
        {
            string viewAsString = "";
            //
            if (qvm.Answer == null)
            {
                //var question = db.Questions.Find(qvm.QuestionID);
                var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
                if (question != null)
                {
                    var prevQue = db.Questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
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
                    //return PartialView("_QuestionControls", question);
                }
                return Json(new { status = false });
                //return HttpNotFound();
            }
            //
            Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            //        
            if (qvm.NextQuestion == int.MinValue)
            {
                //var question = db.Questions.Find(qvm.QuestionID);
                var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
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
                    //int UserID = USERID; // Convert.ToInt32();
                    //bool isDirty = false;
                    ////
                    //db.QuestionAndAnswers.RemoveRange(db.QuestionAndAnswers.Where(x => x.UserID == UserID));
                    ////
                    //foreach (var item in dictQA)
                    //{
                    //    QuestionAndAnswer qs = new QuestionAndAnswer();
                    //    //
                    //    qs.UserID = UserID;
                    //    //
                    //    qs.QuestionsAndAnswers = $"{item.Key.ToString().PadRight(5, ' ')}{item.Value}";
                    //    db.QuestionAndAnswers.Add(qs);
                    //    isDirty = true;
                    //}
                    ////
                    //if (isDirty)
                    //{
                    //    db.SaveChanges();
                    //}
                    //
                    return Json(new { status = true, url = Url.Action("Summary", "Home") });
                    //return PartialView("_ShowSummaryLink");
                }
                //return Json(new { status = false });
                //return HttpNotFound();
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
            Question objQuestion = SearchQuestionById(qvm.NextQuestion);
            Question prevQues = db.Questions.Where(x => x.NextQuestionID == objQuestion.ID).FirstOrDefault();
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
                //return PartialView("_QuestionControls", objQuestion);
            }
            //
            //Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            if (dictQA != null)
            {
                if (dictQA.ContainsKey(objQuestion.ID))
                {
                    objQuestion.Answered = dictQA[objQuestion.ID];
                }
            }
            viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
            return Json(new { status = true, view = viewAsString });
            //return PartialView("_QuestionControls", objQuestion);
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
            var qu = db.Questions.ToList();
            Dictionary<int, string> qas = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            //var qas = db.QuestionAndAnswers.Where(x => x.UserID == USERID).ToList();
            foreach (var q in qu)
            {
                SummaryVM summary = GetSummaryObjFromDict(q, qas);
                summaries.Add(summary);
            }

            return View(summaries);
            // MAJID ADD CODE HERE

            //foreach (var qa in qas)
            //{
            //    //
            //    var qu = db.Questions.ToList();

            //    //
            //    string strQuestId = qa.QuestionsAndAnswers.Substring(0, 5);
            //    var question = db.Questions.Find(int.Parse(strQuestId));
            //    //
            //    SummaryVM summary = new SummaryVM();
            //    summary.Question = question.QuestionText;
            //    summary.Answer = qa.QuestionsAndAnswers.Substring(5);
            //    summaries.Add(summary);
            //}
            //return View(summaries);
            //return Content("<h1>Summary Page Goes Here</h1>");
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

            bool isDirty = false;
            //
            db.QuestionAndAnswers.RemoveRange(db.QuestionAndAnswers.Where(x => x.UserID == USERID));
            //
            foreach (var item in dictQA)
            {
                QuestionAndAnswer qs = new QuestionAndAnswer();
                //
                qs.UserID = USERID;
                //
                qs.QuestionsAndAnswers = $"{item.Key.ToString().PadRight(5, ' ')}{item.Value}";
                db.QuestionAndAnswers.Add(qs);
                isDirty = true;
            }
            //
            if (isDirty)
            {
                db.SaveChanges();
                return Json(new { result = true });
            }
            return Json(new { result = false, errorType = "NO_DATA", error = "No data was found to be saved" });
        }

    }
}