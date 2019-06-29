using Quizzit.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Quizzit.Controllers
{
    
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

}