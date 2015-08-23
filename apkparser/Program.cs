using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace apkparser
{
    class Program
    {

        static StreamWriter fileWriter;
        static ApkData apk;
        static Guid guid;

        static void Main(string[] args)
        {
            string apkPath = args[0];
            string aaptPath = @"C:\Users\Prashant\AppData\Local\Android\sdk\build-tools\22.0.1\aapt.exe";

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
            process.ErrorDataReceived += ProcessOnOutputDataReceived;

            guid = Guid.NewGuid();

            string filename = string.Format(@"c:\logs\{0}.txt", guid);
            fileWriter = new StreamWriter(filename, true);
            apk = new ApkData();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Debug.WriteLine(process.ExitCode);

            CleanProcess(process);
        }

        static void CleanProcess(Process process)
        {

            var jsonstring = JsonConvert.SerializeObject(apk);
            File.WriteAllText(@"c:\logs\" + guid + ".json", jsonstring);

            fileWriter.Close();
            fileWriter.Dispose();

            process.OutputDataReceived -= ProcessOnOutputDataReceived;
            process.ErrorDataReceived -= ProcessOnOutputDataReceived;
            process.Dispose();
        }


        static void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;

            if (line == null)
            {
                return;
            }

            fileWriter.WriteLine(line);

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
