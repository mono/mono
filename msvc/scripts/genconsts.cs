using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public static class Program {
    public static int Main (string[] args) {
        var myAssembly = Assembly.GetExecutingAssembly ();
        var codeBase = new Uri (myAssembly.CodeBase);
        var executablePath = Path.GetFullPath (codeBase.LocalPath);
        var executableDirectory = Path.GetDirectoryName (executablePath);

        var winsetupDirectory = Path.Combine (executableDirectory, "..");
        var winsetupPath = Path.Combine (winsetupDirectory, "winsetup.bat");

        var psi = new ProcessStartInfo (winsetupPath) {
            WorkingDirectory = winsetupDirectory,
            UseShellExecute = false,
            ErrorDialog = false,
            // CreateNoWindow = true,
            RedirectStandardOutput = true
        };

        string monoVersion, monoCorlibVersion;

        Process winsetupProcess;

        try {
            winsetupProcess = Process.Start(psi);
        } catch (Exception exc) {
            Console.Error.WriteLine ("Failed starting winsetup.bat");
            Console.Error.WriteLine (exc);
            return 1;
        }

        using (winsetupProcess) {
            var outputBuffer = new StringBuilder ();

            winsetupProcess.OutputDataReceived += (s, e) => {
                outputBuffer.AppendLine (e.Data);
            };
            winsetupProcess.BeginOutputReadLine();
            winsetupProcess.WaitForExit ();

            var output = outputBuffer.ToString ().Trim ();

            if (winsetupProcess.ExitCode != 0) {
                Console.Error.WriteLine ("Failed running winsetup.bat");
                Console.Write (output);
                return winsetupProcess.ExitCode;
            } else {
                var m = Regex.Match (output, "MONO_VERSION=([0-9.]+)");
                if (!m.Success)
                    return 1;
                monoVersion = m.Groups[1].Value;

                // HACK: winsetup.bat produces N.N.N instead of N.N.N.N like configure.ac,
                //  so we add .0's to match the Consts.cs generated by make
                while (monoVersion.Split ('.').Length < 4)
                    monoVersion += ".0";

                Console.WriteLine($"MONO_VERSION={monoVersion}");
                m = Regex.Match (output, "MONO_CORLIB_VERSION=([0-9]+)");
                if (!m.Success)
                    return 1;
                monoCorlibVersion = m.Groups[1].Value;
                Console.WriteLine($"MONO_CORLIB_VERSION={monoCorlibVersion}");
            }
        }

        var constsDirectory = Path.Combine (executableDirectory, "..", "..", "mcs", "build", "common");
        var constsTemplatePath = Path.Combine (constsDirectory, "Consts.cs.in");        
        if (!Directory.Exists (constsDirectory) || !File.Exists (constsTemplatePath)) {
            Console.Error.WriteLine ($"File not found: {constsTemplatePath}");
            return 1;
        }

        var resultPath = Path.GetFullPath (Path.Combine (constsDirectory, "Consts.cs"));
        var templateText = File.ReadAllText (constsTemplatePath);

        var resultText = templateText.Replace ("@MONO_VERSION@", monoVersion)
            .Replace ("@MONO_CORLIB_VERSION@", monoCorlibVersion);

        if (File.Exists (resultPath)) {
            var existingText = File.ReadAllText (resultPath);
            if (existingText.Trim () == resultText.Trim ()) {
                Console.WriteLine ($"{resultPath} not changed");
                return 0;
            }
        }

        File.WriteAllText (resultPath, resultText);
        Console.WriteLine ($"Generated {resultPath} successfully");

        return 0;
    }
}