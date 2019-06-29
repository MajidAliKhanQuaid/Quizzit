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
    //    public class QuestionAndAnswerA
    //    {
    //        public int ID { get; set; }
    //        public int UserID { get; set; }
    //        public string QuestionsAndAnswers { get; set; }
    //        public QuestionA Question { get; set; }
    //    }

    public class QuestionAndAnswerFields
    {
        public static string ID { get; set; } = "ID";
        public static string UserID { get; set; } = "UserID";
        public static string QuestionsAndAnswers { get; set; } = "QuestionsAndAnswers";
        //
        public static string _ID { get; set; } = "_ID";
        public static string _QuestionText { get; set; } = "_QuestionText";
        public static string _QuestionType { get; set; } = "_QuestionType";
        public static string _NextQuestionID { get; set; } = "_NextQuestionID";
    }

    //public class QuestionAnswerA
    //{
    //    public int ID { get; set; }
    //    public int QuestionID { get; set; }
    //    public string AnswerText { get; set; }
    //    public Nullable<int> NextQuestionID { get; set; }
    //}

    public class QuestionAnswerFields
    {
        public static string ID { get; set; } = "ID";
        public static string QuestionID { get; set; } = "QuestionText";
        public static string AnswerText { get; set; } = "QuestionType";
        public static string NextQuestionID { get; set; } = "NextQuestionID";
    }

    //public class QuestionA
    //{
    //    public int ID { get; set; }
    //    public string QuestionText { get; set; }
    //    public int QuestionType { get; set; }
    //    public Nullable<int> NextQuestionID { get; set; }
    //    public string Answered { get; set; }
    //    public int PrevQuestionID { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public List<QuestionAnswerA> QAs { get; set; }
    //    public QuestionA()
    //    {
    //        QAs = new List<QuestionAnswerA>();
    //    }
    //}

    public class QuestionFields
    {
        public static string ID { get; set; } = "ID";
        public static string QuestionText { get; set; } = "QuestionText";
        public static string QuestionType { get; set; } = "QuestionType";
        public static string NextQuestionID { get; set; } = "NextQuestionID";
        // Fields from [QuestionAnswer] FOREIGN KEY
        public static string _ID { get; set; } = "_ID";
        public static string _QuestionID { get; set; } = "_QuestionID";
        public static string _AnswerText { get; set; } = "_AnswerText";
        public static string _NextQuestionID { get; set; } = "_NextQuestionID";
        // Not Mapped Field
        public static string Answered { get; set; } = "Answered";
        public static string PrevQuestionID { get; set; } = "PrevQuestionID";
        public static string ErrorMessage { get; set; } = "ErrorMessage";
    }

    public partial class HomeController : Controller
    {
        static string CONNECTION_STRING = @"data source=HP-MAJIDALI\SQLEXPRESS;initial catalog=Quizzit;integrated security=True;";

        int ExecuteBulkQueries(List<string> _lstQueries)
        {
            string query = string.Join("", _lstQueries);
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = query;
                    //command.CommandText = "SELECT * FROM [Question]";
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected;
                }
            }
            return -1;
        }

        #region Question Queries

        public QuestionA GetPreviousQuestion(QuestionA question)
        {
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = string.Format("SELECT [Question].[ID], [Question].[QuestionText], [Question].[QuestionType], [Question].[NextQuestionID], [QuestionAnswer].[ID] AS _ID, [QuestionAnswer].[QuestionID] AS _QuestionID, [QuestionAnswer].[AnswerText] AS _AnswerText, [QuestionAnswer].[NextQuestionID] AS _NextQuestionID FROM [Question] LEFT JOIN [QuestionAnswer] ON [Question].[ID] = [QuestionAnswer].[QuestionID] WHERE [Question].[NextQuestionID] = {0}", question.ID);
                    //command.CommandText = "SELECT * FROM [Question]";
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        List<QuestionA> qt = ReadQuestionsFromReader(reader);
                        if (qt.Count > 0)
                        {
                            return qt[0];
                        }
                        return null;
                    }
                }
            }
            return null;
        }

        public QuestionA FindQuestion(int Id)
        {
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = String.Format("SELECT [Question].[ID], [Question].[QuestionText], [Question].[QuestionType], [Question].[NextQuestionID], [QuestionAnswer].[ID] AS _ID, [QuestionAnswer].[QuestionID] AS _QuestionID, [QuestionAnswer].[AnswerText] AS _AnswerText, [QuestionAnswer].[NextQuestionID] AS _NextQuestionID FROM [Question] LEFT JOIN [QuestionAnswer] ON [Question].[ID] = [QuestionAnswer].[QuestionID] WHERE [Question].[ID] = {0}", Id);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        List<QuestionA> qt = ReadQuestionsFromReader(reader);
                        if (qt.Count > 0)
                        {
                            return qt[0];
                        }
                        return null;
                    }
                }
            }
            return null;
        }

        public List<QuestionA> GetAllQuestions()
        {
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = "SELECT [Question].[ID], [Question].[QuestionText], [Question].[QuestionType], [Question].[NextQuestionID], [QuestionAnswer].[ID] AS _ID, [QuestionAnswer].[QuestionID] AS _QuestionID, [QuestionAnswer].[AnswerText] AS _AnswerText, [QuestionAnswer].[NextQuestionID] AS _NextQuestionID FROM [Question] LEFT JOIN [QuestionAnswer] ON [Question].[ID] = [QuestionAnswer].[QuestionID]";
                    //command.CommandText = "SELECT * FROM [Question]";
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        List<QuestionA> qt = ReadQuestionsFromReader(reader);
                        return qt;
                    }
                }
            }
            return new List<QuestionA>();
        }

        private QuestionAnswerA ReadQuestionForeignKeyData(SqlDataReader _reader)
        {
            QuestionAnswerA qA = new QuestionAnswerA();
            //
            if (_reader[QuestionFields._ID].GetType() != typeof(DBNull))
            {
                qA.ID = Convert.ToInt32(_reader[QuestionFields._ID]);
                qA.QuestionID = Convert.ToInt32(_reader[QuestionFields._QuestionID]);
                qA.AnswerText = _reader[QuestionFields._AnswerText].ToString();
                if (_reader[QuestionFields._NextQuestionID].GetType() == typeof(DBNull))
                {
                    qA.NextQuestionID = int.MinValue;
                }
                else
                {
                    qA.NextQuestionID = Convert.ToInt32(_reader[QuestionFields._NextQuestionID]);
                }

                return qA;
            }
            return null;
        }

        private List<QuestionA> ReadQuestionsFromReader(SqlDataReader _reader)
        {
            List<QuestionA> questions = new List<QuestionA>();
            List<QuestionAnswerA> options = new List<QuestionAnswerA>();
            Dictionary<int, bool> dict = new Dictionary<int, bool>();
            while (_reader.Read())
            {
                QuestionA question = new QuestionA();
                question.ID = Convert.ToInt32(_reader[QuestionFields.ID]);
                question.QuestionText = _reader[QuestionFields.QuestionText].ToString();
                question.QuestionType = Convert.ToInt32(_reader[QuestionFields.QuestionType]);
                if (_reader[QuestionFields.NextQuestionID].GetType() == typeof(DBNull))
                {
                    question.NextQuestionID = int.MinValue;
                }
                else
                {
                    question.NextQuestionID = Convert.ToInt32(_reader[QuestionFields.NextQuestionID]);
                }
                // Role of Dictionary is to make sure .. No duplication occurs in List
                if (dict.ContainsKey(question.ID) == false)
                {
                    dict.Add(question.ID, true);
                    questions.Add(question);
                }
                //
                QuestionAnswerA qA = ReadQuestionForeignKeyData(_reader);
                if (qA != null)
                {
                    options.Add(qA);
                    questions[questions.Count - 1].QAs.Add(qA);
                }
            }
            Session["Options"] = options;
            return questions;
        }

        #endregion

        #region QuestionAnswer

        #endregion

        #region QuestionAndAnswer
        //db.QuestionAndAnswers.Where(x => x.UserID == USERID).ToList();

        private List<QuestionAndAnswerA> ReadAnsweredQuestionsFromReader(SqlDataReader _reader)
        {
            List<QuestionAndAnswerA> answeredQs = new List<QuestionAndAnswerA>();
            Dictionary<int, bool> dict = new Dictionary<int, bool>();
            while (_reader.Read())
            {
                QuestionAndAnswerA answered = new QuestionAndAnswerA();
                answered.ID = Convert.ToInt32(_reader[QuestionAndAnswerFields.ID]);
                answered.UserID = Convert.ToInt32(_reader[QuestionAndAnswerFields.UserID]);
                answered.QuestionsAndAnswers = _reader[QuestionAndAnswerFields.QuestionsAndAnswers].ToString();
                // Foreign Key [QuestionID]
                int qId = Convert.ToInt32(answered.QuestionsAndAnswers.Substring(0, 5));
                QuestionA question = new QuestionA();
                question.ID = Convert.ToInt32(_reader[QuestionAndAnswerFields._ID]);
                question.QuestionText = _reader[QuestionAndAnswerFields._QuestionText].ToString();
                question.QuestionType = Convert.ToInt32(_reader[QuestionAndAnswerFields._QuestionType]);
                if (_reader[QuestionAndAnswerFields._NextQuestionID].GetType() == typeof(DBNull))
                {
                    question.NextQuestionID = int.MinValue;
                }
                else
                {
                    question.NextQuestionID = Convert.ToInt32(_reader[QuestionAndAnswerFields._NextQuestionID]);
                }
                answered.Question = question;
                answeredQs.Add(answered);
            }
            return answeredQs;
        }

        public List<QuestionAndAnswerA> GetAnsweredQuestionsByUserID(int _userId)
        {
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = string.Format("SELECT [QuestionAndAnswer].[ID],[QuestionAndAnswer].[UserID]     ,[QuestionAndAnswer].[QuestionsAndAnswers], [Question].[ID] AS _ID, [Question].[QuestionType] AS _QuestionType, [Question].[QuestionText] AS _QuestionText, [Question].[NextQuestionID] AS _NextQuestionID FROM [Quizzit].[dbo].[QuestionAndAnswer], [Quizzit].[dbo].[Question] WHERE SUBSTRING([Quizzit].[dbo].[QuestionAndAnswer].[QuestionsAndAnswers], 1, 5) = [Quizzit].[dbo].[Question].[ID] AND [Quizzit].[dbo].[QuestionAndAnswer].[UserID] = {0}", _userId);
                    //command.CommandText = "SELECT * FROM [Question]";
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        List<QuestionAndAnswerA> qt = ReadAnsweredQuestionsFromReader(reader);
                        return qt;
                    }
                }
            }
            return new List<QuestionAndAnswerA>();
        }


        #endregion

    }

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