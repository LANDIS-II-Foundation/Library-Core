using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;

namespace Landis.Core
{
    public class Pause
    {
        public string ExternalScript { get; private set; }
        public string ExternalExecutable { get; private set; }
        public string ExternalCommand { get; private set; }

        public bool UsePause { get; private set; }
        private bool useScript = false;
        private bool useShell = false;
        private static ICore Model { get; set; }

        public Pause(string script, string engine, string command, ICore model)
        {
            Model = model;
            ExternalScript = script;
            ExternalExecutable = engine;
            ExternalCommand = command;

            if (!string.IsNullOrEmpty(ExternalCommand))
            {
                useShell = true;
                useScript = false;
            }
            else if (!string.IsNullOrEmpty(ExternalScript) && !string.IsNullOrEmpty(ExternalExecutable))
            {
                useScript = true;
                useShell = false;
            }

            if (!useShell && !useScript)
                UsePause = false;
            else
                UsePause = true;
        }

        public void PauseTimestep()
        {
            Model.UI.WriteLine("Current time: ", Model.CurrentTime);

            //Create an empty lockfile at the appropriate path - write model timestep to the contents
            StreamWriter lock_file = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "/lockfile");
            lock_file.WriteLine(Model.CurrentTime.ToString());
            lock_file.Close();

            Process pause_process = null;
            if (useShell) //Exhibits preference for custom commands
            {
                pause_process = CallShellScript();
                pause_process.WaitForExit();
                pause_process.Close();
            }
            else
            {
                pause_process = CallExternalExecutable();
                pause_process.WaitForExit();
                pause_process.Close();
            }
        }

        //Using a command shell to evoke arbitrary processes specified by the user
        public Process CallShellScript()
        {
            Model.UI.WriteLine("Starting external shell...");
            Process shell_process = new Process();

            shell_process.StartInfo.UseShellExecute = false;
            shell_process.StartInfo.CreateNoWindow = true;
            shell_process.StartInfo.FileName = "CMD.exe";
            shell_process.StartInfo.Arguments = "/C " + ExternalCommand;
            shell_process.StartInfo.RedirectStandardOutput = true;
            shell_process.StartInfo.RedirectStandardError = true;
            shell_process.StartInfo.CreateNoWindow = true;
            shell_process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            shell_process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            try
            {
                shell_process.Start(); // start the process
                shell_process.BeginOutputReadLine();
                shell_process.BeginErrorReadLine();
            }
            catch (Win32Exception w)
            {
                Model.UI.WriteLine(w.Message);
                Model.UI.WriteLine(w.ErrorCode.ToString());
                Model.UI.WriteLine(w.NativeErrorCode.ToString());
                Model.UI.WriteLine(w.StackTrace);
                Model.UI.WriteLine(w.Source);
                Exception e = w.GetBaseException();
                Model.UI.WriteLine(e.Message);
            }

            return shell_process;
        }

        //Directly running a script using a scripting engine executable
        public Process CallExternalExecutable()
        {
            Model.UI.WriteLine("Starting external process...");
            Process external_process = new Process();

            external_process.StartInfo.FileName = ExternalExecutable;
            external_process.StartInfo.UseShellExecute = false;
            external_process.StartInfo.CreateNoWindow = true;
            external_process.StartInfo.Arguments = ExternalScript;
            external_process.StartInfo.RedirectStandardOutput = true;
            external_process.StartInfo.RedirectStandardError = true;
            external_process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            external_process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            try
            {
                external_process.Start(); // start the process (the python program)
                external_process.BeginOutputReadLine();
                external_process.BeginErrorReadLine();
            }
            catch (Win32Exception w)
            {
                Model.UI.WriteLine(w.Message);
                Model.UI.WriteLine(w.ErrorCode.ToString());
                Model.UI.WriteLine(w.NativeErrorCode.ToString());
                Model.UI.WriteLine(w.StackTrace);
                Model.UI.WriteLine(w.Source);
                Exception e = w.GetBaseException();
                Model.UI.WriteLine(e.Message);
            }

            return external_process;
        }

        public void PrintPause()
        {
            Model.UI.WriteLine("Pause routines: ");
            Model.UI.WriteLine("External script path: " + ExternalScript);
            Model.UI.WriteLine("External script executable: " + ExternalExecutable);
            Model.UI.WriteLine("External command to execute: " + ExternalCommand);
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Model.UI.WriteLine(outLine.Data);
        }
    }
}
