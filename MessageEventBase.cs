/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 * 
 * Notice: This computer software was prepared by Battelle Memorial Institute,
 * hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
 * Department of Energy (DOE).  All rights in the computer software are reserved
 * by DOE on behalf of the United States Government and the Contractor as
 * provided in the Contract.
 * 
 * NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
 * IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
 * 
 * This notice including this sentence must appear on any copies of this computer
 * software.
 * -----------------------------------------------------*/

namespace Cyclops
{
    public class MessageEventBase
    {
        #region "Event Delegates and Classes"

        public event MessageEventHandler ErrorEvent;
        public event MessageEventHandler WarningEvent;
        public event MessageEventHandler MessageEvent;

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        #endregion

        protected void ReportError(string message)
        {
            OnErrorMessage(new MessageEventArgs(message));
        }

        protected void ReportError(string message, string module)
        {
            OnErrorMessage(new MessageEventArgs(message, module));
        }

        protected void ReportError(string message, string module, int step)
        {
            OnErrorMessage(new MessageEventArgs(message, module, step));
        }

        protected void ReportMessage(string message)
        {
            OnMessage(new MessageEventArgs(message));
        }

        protected void ReportMessage(string message, string module)
        {
            OnMessage(new MessageEventArgs(message, module));
        }

        protected void ReportMessage(string message, string module, int step)
        {
            OnMessage(new MessageEventArgs(message, module, step));
        }

        protected void ReportWarning(string message)
        {
            OnWarningMessage(new MessageEventArgs(message));
        }

        protected void ReportWarning(string message, string module)
        {
            OnWarningMessage(new MessageEventArgs(message, module));
        }

        protected void ReportWarning(string message, string module, int step)
        {
            OnWarningMessage(new MessageEventArgs(message, module, step));
        }

        #region "Event Functions"

        protected void OnErrorMessage(MessageEventArgs e)
        {
            if (ErrorEvent != null)
                ErrorEvent(this, e);
        }

        protected void OnMessage(MessageEventArgs e)
        {
            if (MessageEvent != null)
                MessageEvent(this, e);
        }

        protected void OnWarningMessage(MessageEventArgs e)
        {
            if (WarningEvent != null)
                WarningEvent(this, e);
        }
        #endregion
    }

    public class MessageEventArgs : System.EventArgs
    {
        public readonly string Message;
        public readonly string Module;
        public readonly int Step;

        public MessageEventArgs(string strMessage)
        {
            Message = strMessage;
        }

        public MessageEventArgs(string strMessage,
            string strModule)
        {
            Message = strMessage;
            Module = strModule;
        }

        public MessageEventArgs(string strMessage,
            string strModule, int iStep)
        {
            Message = strMessage;
            Module = strModule;
            Step = iStep;
        }
    }
}
