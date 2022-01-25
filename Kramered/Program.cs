using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kramered
{
    internal class Program
    {
        private static string filePath = string.Empty, fileName = string.Empty, tempFilePath = string.Empty, PIP = string.Empty;
        private static string getFileName(string x)
        {
            string[] fileSplit = x.Split('\\');
            return fileSplit[fileSplit.Length - 1].ToString();
        }
        static void Main(string[] args)
        {
            while (!File.Exists(filePath))
            {
                Console.Write("File Name: ");
                filePath = Console.ReadLine().Replace("\"", string.Empty);
                Console.Clear();
            }

            fileName = getFileName(filePath);

            Process.Start("cmd.exe", string.Format("/c pycdas.exe {0} > disassembled", fileName));

            Thread.Sleep(1000);
            
            string fileContent = File.ReadAllText("disassembled");
            string key = Regex.Match(fileContent, "22      LOAD_CONST              1: .*").ToString().Replace("22      LOAD_CONST              1: ", string.Empty);
            MatchCollection codeMatch = Regex.Matches(fileContent, "('.*')");
            string code = codeMatch[codeMatch.Count - 2].Value.Replace("'", string.Empty);

            string codeToExecute = Properties.Resources.script.Replace("%key%", key).Replace("%code%", code);

            tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            File.WriteAllText(tempFilePath, codeToExecute);

            Thread.Sleep(200);

            File.Delete("disassembled");

            while (!File.Exists(PIP))
            {
                Console.WriteLine("Python.exe File Path: ");
                PIP = Console.ReadLine().Replace("\"", string.Empty);
                Console.Clear();
            }

            
            ProcessStartInfo proc = new ProcessStartInfo()
            {
                FileName = PIP,
                Arguments = tempFilePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = true
            };
            Process process = Process.Start(proc);
            process.Start();
            process.WaitForExit();
            File.Delete(tempFilePath);
            using (StreamReader reader = process.StandardOutput)
            {
                string err = process.StandardError.ReadToEnd();
                if (err.Length > 0)
                {
                    Console.WriteLine(String.Format("Error:\n{0}", err));
                    Console.ReadLine();
                    Environment.Exit(1337);
                }
                File.WriteAllText("deobfuscated.py", String.Format("# => https://github.com/HideakiAtsuyo/Kramered\n{0}", process.StandardOutput.ReadToEnd()));
                Console.WriteLine("Kramered !");
            }
            Console.ReadLine();
        }
    }
}
