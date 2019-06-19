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
            return db.Questions.Find(qid);
        }

        public string RenderViewAsString(string viewName, object model)
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
            //LoadQuestionsIfNotExist();
        }

        public ActionResult Index()
        {
            var questions = db.Questions.ToList();
            Session["Questions"] = questions;
            // Here Integer Holds Value for Question ID | String accounts for the Answer
            Session["AnsweredQuestions"] = new Dictionary<int, string>();
            //Session["UserID"] = DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year +
            //    DateTime.Now.Hour + DateTime.Now.Minute +  DateTime.Now.Second;
            char[] numbers = Guid.NewGuid().ToString().Where(x => Char.IsDigit(x)).ToArray();
            string UserID = string.Join("", numbers);
            if(UserID.Length > 9)
            {
                UserID = UserID.Substring(0, 9);
            }
            Session["UserID"] = UserID;
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
                    if (question.QuestionType == (int)QuestionType.Checkbox ||
                    question.QuestionType == (int)QuestionType.Radio ||
                    question.QuestionType == (int)QuestionType.Dropdown)
                    {
                        ViewBag.ErrorMessage = "You must have atleast one option selected";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Answer field can not be blank";
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
            if (qvm.NextQuestion == int.MinValue)
            {
                //var question = db.Questions.Find(qvm.QuestionID);
                var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
                if (question != null)
                {
                    // ************************************
                    // *********   Save Here    ***********
                    // ************************************
                    Dictionary<int, string> dictQA = (Session["AnsweredQuestions"] as Dictionary<int, string>);
                    if (dictQA.Keys.Contains(qvm.QuestionID))
                    {
                        dictQA[qvm.QuestionID] = qvm.Answer;
                    }
                    else
                    {
                        dictQA.Add(qvm.QuestionID, qvm.Answer);
                    }
                    int UserID = Convert.ToInt32(Session["UserID"]);
                    bool isDirty = false;
                    //
                    db.QuestionAndAnswers.RemoveRange(db.QuestionAndAnswers.Where(x => x.UserID == UserID));
                    //
                    foreach (var item in dictQA)
                    {
                        QuestionAndAnswer qs = new QuestionAndAnswer();
                        //
                        qs.UserID = UserID;
                        //
                        qs.QuestionsAndAnswers = $"{item.Key.ToString().PadRight(5, ' ')}{item.Value}";
                        db.QuestionAndAnswers.Add(qs);
                        isDirty = true;
                    }
                    //
                    if (isDirty)
                    {
                        db.SaveChanges();
                    }
                    //
                    return Json(new { status = true, url = Url.Action("Summary", "Home") });
                    //return PartialView("_ShowSummaryLink");
                }
                //return Json(new { status = false });
                //return HttpNotFound();
            }
            // Saving Answer in the Session variable
            Dictionary<int, string> dict = (Session["AnsweredQuestions"] as Dictionary<int, string>);
            if (dict.Keys.Contains(qvm.QuestionID))
            {
                dict[qvm.QuestionID] = qvm.Answer;
            }
            else
            {
                dict.Add(qvm.QuestionID, qvm.Answer);
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
            viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
            return Json(new { status = true, view = viewAsString });
            //return PartialView("_QuestionControls", objQuestion);
        }

        public ActionResult Summary()
        {
            if(Session["UserID"] == null)
            {
                return RedirectToAction("Index");
            }
            List<SummaryVM> summaries = new List<SummaryVM>();
            int UserID = Convert.ToInt32(Session["UserID"]);
            var qas = db.QuestionAndAnswers.Where(x => x.UserID == UserID).ToList();
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
            //return Content("<h1>Summary Page Goes Here</h1>");
        }

    }
}