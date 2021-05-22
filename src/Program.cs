using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;

namespace gInk
{
    static class Program
    {

        #region Dll Imports
        private const int HWND_BROADCAST = 0xFFFF;

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        private static extern int RegisterWindowMessage(string message);
        #endregion Dll Imports
        public static int StartInkingMsg = RegisterWindowMessage("START_INKING");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CallForm frm;

            if (!EnsureSingleInstance()) return;

            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DateTime n = DateTime.Now;
            Environment.SetEnvironmentVariable("YYYY", n.Year.ToString("0000"));
            Environment.SetEnvironmentVariable("YY", (n.Year % 100).ToString("00"));
            Environment.SetEnvironmentVariable("MM", n.Month.ToString("00"));
            Environment.SetEnvironmentVariable("DD", n.Day.ToString("00"));
            Environment.SetEnvironmentVariable("H", n.Hour.ToString("00"));
            Environment.SetEnvironmentVariable("M", n.Month.ToString("00"));
            Environment.SetEnvironmentVariable("S", n.Second.ToString("00"));

            frm = new CallForm(new Root());
            //frm.Root = new Root();
            frm.Root.callForm = frm;
            if (frm.Root.FormOpacity > 0)
                frm.Show();
            // if not applied after shown there seems to be issues with the dimensions
            frm.Top = frm.Root.FormTop;
            frm.Left = frm.Root.FormLeft;
            frm.Width = frm.Root.FormWidth;
            frm.Height = frm.Root.FormWidth;
            frm.Opacity = frm.Root.FormOpacity / 100.0;
            if (Environment.CommandLine.IndexOf("--StartInking", StringComparison.OrdinalIgnoreCase) >= 0)
                PostMessage((IntPtr)HWND_BROADCAST, StartInkingMsg, (IntPtr)null, (IntPtr)null); // to Myself
            Application.Run();
        }

        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            DialogResult result = DialogResult.Cancel;
            try
            {
                Exception ex = (Exception)t.Exception;
                DateTime lastModified = System.IO.File.GetLastWriteTime(Environment.GetCommandLineArgs()[0]);
                string errorMsg = "UIThreadException\r\n\r\n";
                errorMsg += "version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " built on " + Build.Timestamp + "\r\n";
                errorMsg += "Oops, ppInk crashed! Please include the following information if you plan to contact the developers (a copy of the following information is stored in crash.txt in the application folder):\r\n\r\n";
                errorMsg += ex.Message + "\r\n\r\n";
                errorMsg += "Stack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                WriteErrorLog(errorMsg);

                errorMsg += "!!! PLEASE PRESS ESC KEY TO EXIT IF YOU FEEL YOUR MOUSE CLICK IS BLOCKED BY SOMETHING";
                ShowErrorDialog("UIThreadException", errorMsg);
            }
            catch
            {
                try
                {
                    MessageBox.Show("Fatal Windows Forms Error", "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            // Exits the program when the user clicks Abort.
            if (result == DialogResult.Abort)
                Application.Exit();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                DateTime lastModified = System.IO.File.GetLastWriteTime(Environment.GetCommandLineArgs()[0]);
                string errorMsg = "UnhandledException\r\n\r\n";
                errorMsg += "version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " built on " + Build.Timestamp + "\r\n";
                errorMsg += "Oops, ppInk crashed! Please include the following information if you plan to contact the developers:\r\n\r\n";
                errorMsg += ex.Message + "\r\n\r\n";
                errorMsg += "Stack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                WriteErrorLog(errorMsg);

                ShowErrorDialog("UnhandledException", errorMsg);

                if (!EventLog.SourceExists("UnhandledException"))
                {
                    EventLog.CreateEventSource("UnhandledException", "Application");
                }
                EventLog myLog = new EventLog();
                myLog.Source = "UnhandledException";
                myLog.WriteEntry(errorMsg);
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("Fatal Non-UI Error", "Fatal Non-UI Error. Could not write the error to the event log. Reason: " + exc.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }

        private static bool EnsureSingleInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();

            var runningProcess = (from process in Process.GetProcesses()
                                  where
                                    process.Id != currentProcess.Id &&
                                    process.ProcessName.Equals(
                                      currentProcess.ProcessName,
                                      StringComparison.Ordinal) &&
                                    process.SessionId == currentProcess.SessionId
                                  select process).FirstOrDefault();

            if (runningProcess != null)
            {
                PostMessage((IntPtr)HWND_BROADCAST, StartInkingMsg, (IntPtr)null, (IntPtr)null);
                return false;
            }

            return true;
        }


        private static DialogResult ShowErrorDialog(string title, string errormsg)
        {
            return MessageBox.Show(errormsg, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }

        public static void WriteErrorLog(string errormsg)
        {
            try
            {
                FileStream fs = new FileStream("crash.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(System.DateTime.Now.ToString("MM / dd / yyyy HH:mm"));
                sw.Write(errormsg);
                sw.Close();
                fs.Close();
            }
            catch
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "crash.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(System.DateTime.Now.ToString("MM / dd / yyyy HH:mm"));
                sw.Write(errormsg);
                sw.Close();
                fs.Close();
            }
        }
    }
}
