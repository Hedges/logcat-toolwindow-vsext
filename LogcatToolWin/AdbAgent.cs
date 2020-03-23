using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;
using Microsoft.Win32.SafeHandles;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Threading;
  
namespace LogcatToolWin
{
    /*[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES
    {
        public DWORD nLength;
        public IntPtr lpSecurityDescriptor;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
    }*/

    public class AdbAgent
    {
        public static Action<LogcatOutputToolWindowControl.LogcatItem.Level, string, int, string, string, string> OnOutputLogcat;
        public bool IsDeviceReady = false;
        public string DeviceName;
        public static Action<string, bool> OnDeviceChecked;
        public string AdbExePath;
        public static Action ToOpenSettingDlg;
        public static Action<int> OnPidGreped;

        Process outputProcess = null;
        public AdbAgent()
        {
        }
        ~AdbAgent()
        {
            StopAdbLogcat();
        }
        /*[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreatePipe(ref IntPtr hReadPipe, ref IntPtr hWritePipe, IntPtr lpPipeAttributes, DWORD nSize);
        [DllImport("Kernel32.dll")]
        public static extern bool CloseHandle(System.IntPtr hObject);

        public string ProceedAdbCommand(string cmd)
        {
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = (DWORD)Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true;

            IntPtr attr = Marshal.AllocHGlobal(Marshal.SizeOf(sa));
            Marshal.StructureToPtr(sa, attr, true);

            IntPtr hChildStd_OUT_Rd = new IntPtr();
            IntPtr hChildStd_OUT_Wr = new IntPtr();
            //HANDLE hChildStd_IN_Rd = NULL;
            //HANDLE hChildStd_IN_Wr = NULL;


            if (!CreatePipe(ref hChildStd_OUT_Rd, ref hChildStd_OUT_Wr, attr, 0))
            {
                MessageBox.Show("StdoutRd CreatePipe Fail");
                return "";
            }

            MessageBox.Show("StdoutRd CreatePipe OK");

            /*PROCESS_INFORMATION piProcInfo;
            STARTUPINFO siStartInfo;
            BOOL bSuccess = FALSE;

            ZeroMemory(&piProcInfo, sizeof(PROCESS_INFORMATION));
            ZeroMemory(&siStartInfo, sizeof(STARTUPINFO));
            siStartInfo.cb = sizeof(STARTUPINFO);
            siStartInfo.hStdError = hChildStd_OUT_Wr;
            siStartInfo.hStdOutput = hChildStd_OUT_Wr;
            //siStartInfo.hStdInput = hChildStd_IN_Rd;
            siStartInfo.dwFlags |= (STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW);
            siStartInfo.wShowWindow = SW_HIDE;

            // Create the child process. 

            std::string full_cmd = "c:\\Develop\\Android\\SDK\\platform-tools\\adb.exe " + cmd;
            bSuccess = CreateProcess(NULL,
                (LPSTR)full_cmd.c_str(),     // command line 
                NULL,          // process security attributes 
                NULL,          // primary thread security attributes 
                TRUE,          // handles are inherited 
                0,             // creation flags 
                NULL,          // use parent's environment 
                NULL,          // use parent's current directory 
                &siStartInfo,  // STARTUPINFO pointer 
                &piProcInfo);  // receives PROCESS_INFORMATION 

            if (!bSuccess)
            {
                OutputDebugString(TEXT("CreateProcess"));
                return "";
            }

            WaitForSingleObject(piProcInfo.hProcess, INFINITE);
            DWORD dwRead;
            CHAR chBuf[2048];
            ZeroMemory(chBuf, 2048);
            bSuccess = FALSE;

            bSuccess = ReadFile(hChildStd_OUT_Rd, chBuf, 2048, &dwRead, NULL);
            std::string ret = "";
            if (bSuccess) ret = std::string(chBuf);
            OutputDebugString(chBuf);*/
        /*  CloseHandle(hChildStd_OUT_Rd);
          CloseHandle(hChildStd_OUT_Wr);
          return "";
      }*/
        public void ProceedAdbCommandToExit(string cmd, EventHandler handler, bool waitForExit)
        {
            Process process = new Process();
            string full_cmd = AdbExePath + " "; // "c:/Develop/Android/SDK/platform-tools/adb.exe "; // + cmd;
            process.StartInfo.FileName = full_cmd;
            process.StartInfo.Arguments = cmd; // "/c DIR"; // Note the /c command (*)
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Exited += handler;
            process.EnableRaisingEvents = true;
            //process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            //process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't Start adb.exe, please setup AdbExePath");
                if (ToOpenSettingDlg != null)
                {
                    ToOpenSettingDlg();
                }
            }
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
            }
            //* Read the output (or the error)
            //string output = process.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            //Console.WriteLine(output);
            //string err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);
        }
        public void ProceedAdbCommandToOutput(string cmd, DataReceivedEventHandler handler)
        {
            if (outputProcess != null) return;
            outputProcess = new Process();
            string full_cmd = AdbExePath + " "; // "c:/Develop/Android/SDK/platform-tools/adb.exe "; // + cmd;
            outputProcess.StartInfo.FileName = full_cmd;
            //outputProcess.StartInfo.Arguments = cmd; // "/c DIR"; // Note the /c command (*)
            outputProcess.StartInfo.UseShellExecute = false;
            outputProcess.StartInfo.RedirectStandardOutput = true;
            outputProcess.StartInfo.RedirectStandardError = true;
            outputProcess.StartInfo.CreateNoWindow = true;
            outputProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            outputProcess.OutputDataReceived += handler;
            outputProcess.ErrorDataReceived += handler;
            //* Start process and handlers
            try
            {
                outputProcess.Start();
                outputProcess.PriorityClass = ProcessPriorityClass.Idle;
                outputProcess.BeginOutputReadLine();
                outputProcess.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't Start adb.exe, please setup AdbExePath");
                if (ToOpenSettingDlg != null)
                {
                    ToOpenSettingDlg();
                }
                outputProcess = null;
            }
            //process.WaitForExit();
            //* Read the output (or the error)
            //string output = process.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            //Console.WriteLine(output);
            //string err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);
        }
        public void CheckAdbDevice()
        {
            ProceedAdbCommandToExit("devices", new EventHandler(AdbCheckDeviceHandler), false);
        }
        public void StartAdbLogcat()
        {
            // 開始前先清除前面的, 免得因為前面超多, 會 Hang住
            ProceedAdbCommandToExit("logcat -b all -c", null, false);
            ProceedAdbCommandToOutput("logcat -v time", new DataReceivedEventHandler(OutputHandler));
        }
        public void StopAdbLogcat()
        {
            if (outputProcess != null)
            {
                outputProcess.CancelErrorRead();
                outputProcess.CancelOutputRead();
                //outputProcess.CloseMainWindow();
                //outputProcess.Close();
                try
                {
                    outputProcess.Kill();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("process kill exception " + e.Message);
                }
                outputProcess = null;
            }
        }
        void AdbCheckDeviceHandler(object sender, System.EventArgs ev)
        {
            Process process = sender as Process;
            string output = process.StandardOutput.ReadToEnd();
            string[] msg_line = output.Split(new char[] { '\n', '\r'});
            foreach (string msg in msg_line)
            {
                string[] tokens = msg.Split('\t');
                if (tokens.Length != 2) continue;
                DeviceName = tokens[0];
                if (tokens[1] == "device") IsDeviceReady = true; else IsDeviceReady = false;
            }
            IsDeviceReady = true;
            if (OnDeviceChecked != null)
            {
                OnDeviceChecked(DeviceName, IsDeviceReady);
            }
            //MessageBox.Show(output);
        }
        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (OnOutputLogcat == null) return;
            if (outLine.Data == null) return;

            string msg = outLine.Data;

            char[] separators = {' ', '[', ']'};
            string[] tokens = msg.Split(separators, 8, System.StringSplitOptions.RemoveEmptyEntries);
            if(tokens.Length < 7)
            {
                return;
            }

            try
            {
                string time_token = tokens[0] + " " + tokens[1] + " " + tokens[2];

                string tag_token = tokens[4];

                string pid_token = tokens[5];
                int pid = System.Convert.ToInt32(pid_token);

                string level_token = tokens[6];
                LogcatOutputToolWindowControl.LogcatItem.Level log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Verbose;
                switch (level_token)
                {
                    case "<Emergency>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Fatal; break;
                    case "<Alert>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Fatal; break;
                    case "<Critical>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Fatal; break;
                    case "<Error>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Error; break;
                    case "<Warning>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Warning; break;
                    case "<Notice>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Verbose; break;
                    case "<Info>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Info; break;
                    case "<Debug>": log_level = LogcatOutputToolWindowControl.LogcatItem.Level.Debug; break;
                }

                string msg_token = tokens[7];

                OnOutputLogcat(log_level, time_token, pid, tag_token, msg_token, outLine.Data);
            }
            catch (Exception e) { }
        }

        public void AdbGrepPid(string package_name)
        {
            ProceedAdbCommandToExit($"shell ps | grep {package_name}", new EventHandler(AdbGrepPidHandler), true);
        }
        void AdbGrepPidHandler(object sender, System.EventArgs ev)
        {
            Process process = sender as Process;
            string output = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(output)) return;
            string[] msg_line = output.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string msg in msg_line)
            {
                string[] tokens = msg.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) continue;
                string pid_string = tokens[1];
                int pid = Convert.ToInt32(pid_string);
                if (pid > 0)
                {
                    Debug.WriteLine($"Grep PID = {pid}");
                    if (OnPidGreped != null)
                    {
                        OnPidGreped(pid);
                    }
                    return;
                }
            }
        }
    }
}
