using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;


namespace apkparser
{
    class Program
    {
        static ApkData apk;
        static readonly StringBuilder LogBuilder = new StringBuilder();
        static string _path;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No APK file passed!");
                return;
            }

            string apkPath = args[0];
            _path = args[1];

            InitiateBadgingProcess(apkPath);
            InitiateApktoolProcess(apkPath);
        }

        static void InitiateApktoolProcess(string apkPath)
        {
            const string apktool = @"C:\Users\Prashant\AppData\Local\Android\apktool\apk.bat";

          
            string processArgs = string.Format(" d -b -s -f {0} -o {1}", apkPath, Path.Combine(_path, "dump"));
            var process = new Process
            {
                StartInfo =
                {
                    FileName = apktool,
                    Arguments = processArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();

            Console.WriteLine(process.ExitCode);

            LogBuilder.Clear();
            LogBuilder.AppendLine("## Apktool running!");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            Console.WriteLine(error);
            LogBuilder.Append(output);
            LogBuilder.Append(error);

            CleanApktoolProcess(_path);
        }

        static void CleanApktoolProcess(string path)
        {
            string logPath = Path.Combine(path, "process.log");
            File.AppendAllText(logPath, LogBuilder.ToString());
        }

        static void InitiateBadgingProcess(string apkPath)
        {
            const string aaptPath = @"C:\Users\Prashant\AppData\Local\Android\sdk\build-tools\22.0.1\aapt.exe";
            var process = new Process
            {
                StartInfo =
                {
                    FileName = aaptPath,
                    Arguments = "dump badging " + apkPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += ProcessOnOutputDataReceived;

            apk = new ApkData();

            Console.WriteLine("APK badging started!");
            StartProcess(process);


            process.Close();
            Console.WriteLine("Process completed");
            CleanBadgingProcess(process);
        }

        static void StartProcess(Process process)
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        static void CleanBadgingProcess(Process process)
        {
            Console.WriteLine("Finished badging!");
            var jsonstring = apk.ToString();

            var directory = new DirectoryInfo(_path);
            if (!directory.Exists)
            {
                directory.Create();
            }

            string tokenPath = Path.Combine(directory.FullName, "token.json");
            File.WriteAllText(tokenPath, jsonstring);

            string logsPath = Path.Combine(directory.FullName, "process.log");
            File.WriteAllText(logsPath, LogBuilder.ToString());


            process.OutputDataReceived -= ProcessOnOutputDataReceived;
            process.ErrorDataReceived -= ProcessOnOutputDataReceived;
            process.Close();
        }


        static void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;

            if (line == null)
            {
                return;
            }

            LogBuilder.AppendLine(line);
            GetPackageData(line, apk);
            GetApplicationData(line);

        }

        static void GetApplicationData(string line)
        {
            if (line.Contains("application:"))
            {
                var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var items = value.Split(new[] { '\'', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length <= 1)
                    {
                        continue;
                    }

                    if (items[0] == "label")
                    {
                        apk.Label = items[1];
                    }
                    else if (items[0] == "icon")

                    {
                        apk.Icon = items[1];
                    }
                }
            }
        }

        static void GetPackageData(string line, ApkData apk)
        {
            if (line.Contains("package:"))
            {
                var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Debug.WriteLine(values);
                foreach (
                    var items in values.Select(value => value.Split(new[] { '\'', '=' }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(items => items.Length > 1))
                {
                    if (items[0] == "name")
                    {
                        apk.Identifier = items[1];
                    }
                    else if (items[0] == "versionName")

                    {
                        apk.VersionName = items[1];
                    }
                }
            }
        }
    }
}
