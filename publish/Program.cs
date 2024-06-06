using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net.Security;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace publish
{
    class Program
    {

        public static string token = "";
        public static string gitUsername = "";
        public static string email = "";

        static void Git(string args, Action<string> output)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C git "+args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if(e.Data != null && e.Data.Length > 0)
                Console.WriteLine(e.Data);
            });
            p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null && e.Data.Length > 0)
                    Console.WriteLine(e.Data);
            });
            

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
        }

        static void GitShell(string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C git " + args,
                    UseShellExecute = true,
                }
            };

            p.Start();
            p.WaitForExit();
        }




        static void Main(string[] args)
        {
            string repositoryName = Path.GetFileName(Environment.CurrentDirectory);
            bool exists = false;
            

            if (!File.Exists(".gitcreated"))
            {
                Console.WriteLine("Try to build repository...");
                try
                {
                    HttpWebRequest request = null;
                    request = (HttpWebRequest)WebRequest.Create("https://api.github.com/user/repos");

                    string Data = "{\"name\":\"" + repositoryName + "\"}";

                    byte[] body = Encoding.UTF8.GetBytes(Data);
                    //request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.Method = "POST";
                    request.Accept = "*/*";
                    request.Headers["Authorization"] = "token " + token;

                    //  request.Headers["Accept-Encoding"] = "gzip, deflate, br";
                    request.Headers["Accept-Language"] = "en-US,en;q=0.9";
                    request.ContentLength = body.Length;
                    request.ContentType = "application/json";

                    request.Headers["Sec-Fetch-Dest"] = "empty";
                    request.Headers["Sec-Fetch-Mode"] = "cors";
                    request.Headers["Sec-Fetch-Site"] = "same-site";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36";
                    //request.Headers["X-Requested-With"] = "XMLHttpRequest";



                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(body, 0, body.Length);
                        stream.Close();
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    if (responseString.Contains(repositoryName) || responseString.Contains("422"))
                    {
                        File.WriteAllText(".gitcreated", "");
                        exists = true;
                    }

                    Console.WriteLine("Repository successfully created!");
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("422"))
                    {
                        File.WriteAllText(".gitcreated", "");
                        exists = true;
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                        Console.ReadLine();
                    }

                }

                Git("init", (string result) => { Console.WriteLine(result); });
                Git($"config user.email ${email}", (string result) => { Console.WriteLine(result); });
                Git($"config user.name {gitUsername}", (string result) => { Console.WriteLine(result); });

                Git($"remote set-url origin git@github.com:{gitUsername}/" + repositoryName + ".git", (string result) => { Console.WriteLine(result); });
                Git($"remote add origin git@github.com:{gitUsername}/" + repositoryName + ".git", (string result) => { Console.WriteLine(result); });
                Git($"remote set-url origin git@github.com:{gitUsername}/" + repositoryName + ".git", (string result) => { Console.WriteLine(result); });
                Console.WriteLine("3");

            }

            if(!File.Exists(".gitignore"))
            {
                File.WriteAllText(".gitignore", "/push.exe\n/node_modules/*\n/packages/*");
            }
            string ignores = File.ReadAllText(".gitignore");

            if(!ignores.Contains("/push.exe"))
            {
                ignores += "\n/push.exe";
                File.WriteAllText(".gitignore", ignores);
            }

            ignores = File.ReadAllText(".gitignore");

            var vsname = Directory.GetFiles("./").ToList().Where(f => f.EndsWith(".sln")).Select(f=> f.Replace(".sln","").Replace("./","/")).ToList();
            if(vsname.Count > 0)
            {
                if (!ignores.Contains("/node_modules/*"))
                {
                    ignores += "\n/node_modules/*";
                }
                if (!ignores.Contains("/packages/*"))
                {
                    ignores += "\n/packages/*";
                }
                if (!ignores.Contains("chrome"))
                {
                    ignores += "\nchrome";
                }
                if (!ignores.Contains("/node_modules/*"))
                {
                    ignores += "\n/node_modules/*";
                }
                if (!ignores.Contains(vsname[0] + "/bin/*"))
                {
                    ignores += "\n" + vsname[0] + "/bin/*";
                }
                if (!ignores.Contains(vsname[0]+ "/bin/*"))
                {
                    ignores += "\n" + vsname[0] + "/bin/*";
                }
                if (!ignores.Contains(vsname[0] + "/obj/*"))
                {
                    ignores += "\n" + vsname[0] + "/obj/*";
                }
            }

            File.WriteAllText(".gitignore", ignores);

            if (File.Exists(".gitcreated") || exists)
            {
                
                Git("add .", (string result) => { Console.WriteLine(result); });
                Console.WriteLine("1");
                Git("commit -m Update", (string result) => { Console.WriteLine(result); });
                Console.WriteLine("2");
                //if (!File.Exists(".gitcreated"))
                //{
                   
                //}
                Git("commit -m \"Update of "+DateTime.Now.ToShortDateString()+"\"", (string result) => { Console.WriteLine(result); });
                Console.WriteLine("4: Now i want to upload changes:");
                GitShell("push --set-upstream -u origin master --force");
                //Git("push -u origin master", (string result) => { Console.WriteLine(result); });
            }
        }
    }
}
