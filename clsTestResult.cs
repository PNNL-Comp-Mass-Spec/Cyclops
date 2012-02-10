using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyclops
{
    /// <summary>
    /// Provides an externally accessible return type for testing and 
    /// evaluating modules of Cyclops
    /// </summary>
    public class clsTestResult
    {
        private bool b_Successful = false;
        private string s_Module = "";
        private string s_Message = "";
        private string s_RStatement = "";

        #region Properties
        public bool IsSuccessful
        {
            get { return b_Successful; }
            set { b_Successful = value; }
        }

        public string Module
        {
            get { return s_Module; }
            set { s_Module = value; }
        }

        public string Message
        {
            get { return s_Message; }
            set { s_Message = value; }
        }

        public string R_Statement
        {
            get { return s_RStatement; }
            set { s_RStatement = value; }
        }
        #endregion

        #region Contructors
        public clsTestResult()
        {
        }

        public clsTestResult(bool IsSuccessful, string Message)
        {
            this.IsSuccessful = IsSuccessful;
            this.Message = Message;
        }

        public clsTestResult(bool IsSuccessful, string Message, string R_Statement)
        {
            this.IsSuccessful = IsSuccessful;
            this.Message = Message;
            this.R_Statement = R_Statement;
        }
        #endregion
    }
}
