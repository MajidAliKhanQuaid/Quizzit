using Quizzit.Models;
using Quizzit.ViewModel;
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
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
            PartialViewResult pvr = PartialView(viewName, model);
            ViewDataDictionary dDict = new ViewDataDictionary(model);
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                using(HtmlTextWriter markupWriter = new HtmlTextWriter(writer))
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
            LoadQuestionsIfNotExist();
        }

        public ActionResult Index()
        {
            //Question question = db.Questions.FirstOrDefault();
            Question question = (Session["Questions"] as List<Question>).FirstOrDefault();
            if (question == null)
            {
                return Content("There're no questions in the database");
            }
            Question prevQues = db.Questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
            if(prevQues == null)
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

        //[ActionName("Index")]
        //[HttpPost]
        //public ActionResult SaveAnswer(QuestionFormVM qvm)
        //{
        //    if (qvm.Answer == null)
        //    {
        //        //var question = db.Questions.Find(qvm.QuestionID);
        //        var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
        //        if (question != null)
        //        {
        //            if (question.QuestionType == (int)QuestionType.Checkbox ||
        //            question.QuestionType == (int)QuestionType.Radio ||
        //            question.QuestionType == (int)QuestionType.Dropdown)
        //            {
        //                ViewBag.ErrorMessage = "You must have atleast one option selected";
        //            }
        //            else
        //            {
        //                ViewBag.ErrorMessage = "Answer field can not be blank";
        //            }
        //            //
        //            ViewBag.Question = question;
        //            return View();
        //        }
        //        return Content("Question could be recognized by the system");
        //    }
        //    //
        //    if (qvm.NextQuestion == int.MinValue)
        //    {
        //        //var question = db.Questions.Find(qvm.QuestionID);
        //        var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
        //        if (question != null)
        //        {
        //            // ************************************
        //            // *********   Save Here    ***********
        //            // ************************************
        //            return RedirectToAction("Summary");
        //        }
        //        return Content("Question could be recognized by the system");
        //    }
        //    //
        //    Question nextQues = SearchQuestionById(qvm.NextQuestion);
        //    ViewBag.Question = nextQues;
        //    //
        //    return View();
        //}

        [HttpPost]
        public JsonResult AjaxSave2(QuestionFormVM qvm)
        {
            string viewAsString = "";
            // Get Previous
            // Load This Question
            // Search for its previous and return

            var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.PrevQuestion);
            if (question != null)
            {
                Question prevQues = db.Questions.Where(x => x.NextQuestionID == question.ID).FirstOrDefault();
                if(prevQues == null){
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
        public JsonResult AjaxSave(QuestionFormVM qvm)
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
                    //return RedirectToAction("Summary");
                    //
                    return Json(new { status = true, url = Url.Action("Summary", "Home") });
                    //return PartialView("_ShowSummaryLink");
                }
                //return Json(new { status = false });
                //return HttpNotFound();
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

        #region Original

        //[HttpPost]
        //public ActionResult AjaxSave(QuestionFormVM qvm)
        //{
        //    string viewAsString = "";
        //    //
        //    if (qvm.Answer == null)
        //    {
        //        //var question = db.Questions.Find(qvm.QuestionID);
        //        var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
        //        if (question != null)
        //        {
        //            if (question.QuestionType == (int)QuestionType.Checkbox ||
        //            question.QuestionType == (int)QuestionType.Radio ||
        //            question.QuestionType == (int)QuestionType.Dropdown)
        //            {
        //                ViewBag.ErrorMessage = "You must have atleast one option selected";
        //            }
        //            else
        //            {
        //                ViewBag.ErrorMessage = "Answer field can not be blank";
        //            }
        //            //
        //            //viewAsString = RenderViewAsString("_QuestionControls", question);
        //            //return Json(new { status = true, view = viewAsString });
        //            return PartialView("_QuestionControls", question);
        //            //ViewBag.Question = question;
        //            //return View();
        //        }
        //        return HttpNotFound();
        //        //return Json(new { status = false });
        //    }
        //    //
        //    if (qvm.NextQuestion == int.MinValue)
        //    {
        //        //var question = db.Questions.Find(qvm.QuestionID);
        //        var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
        //        if (question != null)
        //        {
        //            // ************************************
        //            // *********   Save Here    ***********
        //            // ************************************
        //            //return RedirectToAction("Summary");
        //            //
        //            //return Json(new { status = true, url = Url.Action("Summary", "Home") });
        //            return PartialView("_ShowSummaryLink");
        //        }
        //        return HttpNotFound();
        //        //return Json(new { status = false });
        //    }
        //    //
        //    Question objQuestion = SearchQuestionById(qvm.NextQuestion);
        //    Question prevQues = db.Questions.Where(x => x.NextQuestionID == objQuestion.ID).FirstOrDefault();
        //    if (prevQues == null)
        //    {
        //        objQuestion.PrevQuestionID = int.MinValue;
        //    }
        //    if (objQuestion.NextQuestionID == null)
        //    {
        //        objQuestion.NextQuestionID = int.MinValue;
        //        //viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
        //        //return Json(new { status = true, view = viewAsString });
        //        return PartialView("_QuestionControls", objQuestion);
        //    }
        //    //viewAsString = RenderViewAsString("_QuestionControls", objQuestion);
        //    //return Json(new { status = true, view = viewAsString });
        //    return PartialView("_QuestionControls", objQuestion);
        //    //ViewBag.Question = nextQues;
        //    ////
        //    //return View();
        //}

        #endregion

        public ActionResult Summary()
        {
            return View();
            //return Content("<h1>Summary Page Goes Here</h1>");
        }

        //public JsonResult NextQuestion(int questionId)
        //{
        //    var question = db.Questions.Find(questionId);
        //    if (question != null)
        //    {
        //        var qOptions = db.QuestionAnswers.
        //            Where(x => x.QuestionID == question.ID)
        //            .Select(x => new QuestionOptionVM()
        //            {
        //                Option = x.AnswerText,
        //                IsAnswer = false
        //            }).ToList();
        //        //
        //        QuestionVM qVm = new QuestionVM();
        //        qVm.QuestionText = question.QuestionText;
        //        qVm.QuestionType = question.QuestionType;
        //        qVm.Options = qOptions;
        //        qVm.NextQuestion = question.NextQuestionID;
        //        //
        //        return Json(new { status = true, question = qVm });
        //    }
        //    return Json(new { status = false });
        //}

        //protected override void Dispose(bool disposing)
        //{
        //db.Dispose();
        //}

    }
}