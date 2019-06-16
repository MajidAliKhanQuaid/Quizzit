using Quizzit.Models;
using Quizzit.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

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
            ViewBag.Question = question;
            return View();
        }

        [ActionName("Index")]
        [HttpPost]
        public ActionResult SaveAnswer(QuestionFormVM qvm)
        {
            if (qvm.Answer == null)
            {
                //var question = db.Questions.Find(qvm.QuestionID);
                var question = (Session["Questions"] as List<Question>).Find(x => x.ID == qvm.QuestionID);
                if(question != null)
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
                    ViewBag.Question = question;
                    return View();
                }
                return Content("Question could be recognized by the system");
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
                    return RedirectToAction("Summary");
                }
                return Content("Question could be recognized by the system");
            }
            //
            Question nextQues = SearchQuestionById(qvm.NextQuestion);
            ViewBag.Question = nextQues;
            //
            return View();
        }
        
        [HttpPost]
        public ActionResult AjaxSave(QuestionFormVM qvm)
        {
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
                    return PartialView("_QuestionControls", question);
                    //ViewBag.Question = question;
                    //return View();
                }
                return HttpNotFound();
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
                    return PartialView("_ShowSummaryLink");
                }
                return HttpNotFound();
            }
            //
            //
            Question nextQues = SearchQuestionById(qvm.NextQuestion);
            if(nextQues.NextQuestionID == null)
            {
                nextQues.NextQuestionID = int.MinValue;
                return PartialView("_QuestionControls", nextQues);
            }
            return PartialView("_QuestionControls", nextQues);
            //ViewBag.Question = nextQues;
            ////
            //return View();
        }


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