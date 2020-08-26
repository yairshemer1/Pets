using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Pets
{
    public class Global : System.Web.HttpApplication
    {
        public static Random random;
        public static Dictionary<string, DateTime> dicCreationTimeByToken;
        public static Dictionary<string, int> dicUserByToken;
        public static Dictionary<int, int> dicRoleByUser;
        public static string strLogFileName;
        public static string strConnectionString;

        protected void Application_Start(object sender, EventArgs e)
        {
            dicUserByToken = new Dictionary<string, int>();
            dicCreationTimeByToken = new Dictionary<string, DateTime>();
            dicRoleByUser = new Dictionary<int, int>();
            AppSettingsReader asr = new AppSettingsReader();
            try
            {
                random = new Random(31 * (885 + int.Parse((DateTime.Now.Ticks % 10000).ToString())));
           //     strLogFileName = asr.GetValue("LogFileName", typeof(string)).ToString();
                strConnectionString = asr.GetValue("cs", typeof(string)).ToString();
            }
            catch (Exception ex)
            {
                //
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            DeleteExpiredTokens();
        }

        public static void DeleteExpiredTokens()
        {
            foreach (string strToken in dicCreationTimeByToken.Keys)
            {
                if ((DateTime.Now - dicCreationTimeByToken[strToken]) > TimeSpan.FromHours(12))
                {
                    try
                    {
                        dicCreationTimeByToken.Remove(strToken);
                        dicUserByToken.Remove(strToken);
                    }
                    catch (Exception ex)
                    {
                        Pets.HandleException(ex);
                    }
                }
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex.Message.Contains("Session state has created a session id, but cannot save it because the response was already flushed by the application."))
            {
                //
            }
            else
            {
                ex = new Exception("an APPLICATION error occured!", ex);
                Pets.HandleException(ex);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}