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
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int FreeConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        public static int StartInkingMsg = RegisterWindowMessage("START_INKING");
        public static string ProgramFolder = "";
        public static string RunningFolder = "";

        public static CallForm frm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static void Main(string [] args)
		{
            if (!EnsureSingleInstance()) return;

            ProgramFolder = AppDomain.CurrentDomain.BaseDirectory;
            ProgramFolder = ProgramFolder.Replace("/", "\\");
            if (!ProgramFolder.EndsWith("\\"))
                ProgramFolder += "\\";

            RunningFolder = ProgramFolder;
            for(int i=0;i<args.Length;i++)
            {
                if (args[i] == "-c")
                    RunningFolder = args[i + 1];
            }
            RunningFolder = PrepareConfigFolder(ProgramFolder, RunningFolder);
            RunningFolder = RunningFolder.Replace("\\", "/");
            ProgramFolder = ProgramFolder.Replace("\\", "/");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AllocConsole();
            ShowWindow(GetConsoleWindow(), 0);
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            // add some debug the help analysis for some testcase reports
            Console.WriteLine();
            foreach (Screen sc in Screen.AllScreens)
            {
                Console.WriteLine("Screen #" + sc.DeviceName);
                Console.WriteLine(sc.Bounds.ToString()+" == "+ sc.WorkingArea.ToString());
            }
            Console.WriteLine();

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
            if (Environment.CommandLine.IndexOf("--StartInking", StringComparison.OrdinalIgnoreCase) >= 0 )
                PostMessage((IntPtr)HWND_BROADCAST, StartInkingMsg, (IntPtr)null, (IntPtr)null); // to Myself
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                Console.WriteLine(string.Format("Loaded: {0}", module.FileName));                
            }
            Console.WriteLine("-----------");
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Console.WriteLine(string.Format("Found: {0} - {1}", a.FullName,a.Location));
            }

            Application.Run();
            FreeConsole();
		}

        private static string PrepareConfigFolder(string programFolder, string runningFolder)
        // returns CurrentFolder
        {
            try
            {
                if (runningFolder != programFolder)
                    throw new Exception("forced running folder");
                string tst_fn = runningFolder + "$test";
                FileStream tst = new FileStream(tst_fn,FileMode.CreateNew);
                tst.Close();
                File.Delete(tst_fn);
            }
            catch
            {
                if (runningFolder == programFolder || runningFolder == "$")
                    runningFolder = "%APPDATA%" + "\\ppInk\\";
                runningFolder = Environment.ExpandEnvironmentVariables(runningFolder);
                if (!runningFolder.EndsWith("\\"))
                    runningFolder += "\\";
                if (!File.Exists(runningFolder+"files_copied"))
                {
                    string[] NON_COPY_EXTS = { ".DLL", ".EXE", ".W32", ".X64", ".MD", ".ZIP", ".LNK", ".PDB", };
                    string[] INI_FILES = { "CONFIG.INI", "HOTKEYS.INI", "PENS.INI",};
                    foreach (string f in Directory.GetFiles(programFolder, "*.*", SearchOption.AllDirectories))
                    {
                        string f1 = f.Replace(programFolder, runningFolder);
                        if (INI_FILES.Contains(Path.GetFileName(f).ToUpper()) && File.Exists(f1))
                            continue;
                        if(!NON_COPY_EXTS.Contains(Path.GetExtension(f).ToUpper()))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(f1));
                            File.Copy(f, f1, true);
                        }
                    }
                }
            }
            Directory.SetCurrentDirectory(runningFolder);
            return runningFolder;
        }

        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
		{
			DialogResult result = DialogResult.Cancel;
			try
			{
				Exception ex = (Exception)t.Exception;
                DateTime lastModified = System.IO.File.GetLastWriteTime(Environment.GetCommandLineArgs()[0]);
                string errorMsg = "UIThreadException\r\n\r\n";
                errorMsg += "version "+Assembly.GetExecutingAssembly().GetName().Version.ToString() + " built on " + Build.Timestamp + "\r\n";
                errorMsg += "Oops, ppInk crashed! Please include the following information if you plan to contact the developers (a copy of the following information is stored in crash.txt in the application folder):\r\n\r\n";
				errorMsg += ex.Message + "\r\n\r\n";
				errorMsg += "Stack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
				WriteErrorLog(errorMsg);

				errorMsg += "\r\n!!! Do you want to Quit ?";
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
                errorMsg += "version "+Assembly.GetExecutingAssembly().GetName().Version.ToString() + " built on " + Build.Timestamp + "\r\n";
                errorMsg += "Oops, ppInk crashed! Please include the following information if you plan to contact the developers:\r\n\r\n";
				errorMsg += ex.Message + "\r\n\r\n";
				errorMsg += "Stack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
				WriteErrorLog(errorMsg);
                errorMsg += "\r\n!!! Do you want to Quit ?";

                ShowErrorDialog("UnhandledException", errorMsg);

                try
                {
                    if (!EventLog.SourceExists("UnhandledException"))
                    {
                        EventLog.CreateEventSource("UnhandledException", "Application");
                    }
                    EventLog myLog = new EventLog();
                    myLog.Source = "UnhandledException";
                    myLog.WriteEntry(errorMsg);
                }
                catch
                {
                    ;
                }

            }
			catch (Exception exc)
			{
				try
				{
					MessageBox.Show("Fatal Non-UI Error. Could not write the error to the event log. Reason: " + exc.Message, "Fatal Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
			DialogResult rst = MessageBox.Show(errormsg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if(rst==DialogResult.Yes)
            {
                try
                {
                    if (frm.Root.FormCollection != null && frm.Root.FormCollection.Visible)
                        frm.Root.FormCollection.SaveStrokes("AutoSave.strokes.txt");
                }
                catch { }
                finally
                {
                    Application.Exit();
                }
            }
            return rst;
		}

		public static void WriteErrorLog(string errormsg)
		{
			try
			{
				FileStream fs = new FileStream(Program.RunningFolder+"crash.txt", FileMode.Append);
				StreamWriter sw = new StreamWriter(fs);
                sw.Write(System.DateTime.Now.ToString("MM / dd / yyyy HH:mm"));
				sw.Write(errormsg);
				sw.Close();
				fs.Close();
			}
			catch
			{
				FileStream fs = new FileStream(Program.RunningFolder+ "crash.txt", FileMode.Append);
				StreamWriter sw = new StreamWriter(fs);
                sw.Write(System.DateTime.Now.ToString("MM / dd / yyyy HH:mm"));
                sw.Write(errormsg);
				sw.Close();
				fs.Close();
			}
		}
	}
}
