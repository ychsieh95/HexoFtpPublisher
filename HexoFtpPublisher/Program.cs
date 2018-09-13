using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using static HexoFtpPublisher.LogExtensions;

namespace HexoFtpPublisher
{
    class Program
    {
        static string host = "127.0.0.1";
        static string port = "21";
        static string user = "";
        static string pass = "";
        static string sourcePath = $@"{ Environment.CurrentDirectory }\public";
        static string remotePath = "";
        static bool cleanRemotePath = true;
        static FtpExists ftpExists = FtpExists.Overwrite;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length <= 0)
                {
                    Console.Write("Host: "); host = Console.ReadLine();
                    Console.Write("Port: "); port = Console.ReadLine();
                    Console.Write("User: "); user = Console.ReadLine();
                    if (!string.IsNullOrEmpty(user)) { Console.Write("Pass: "); pass = GetHiddenConsoleInput(); Console.WriteLine(); }
                    Console.Write("Source Folder: "); sourcePath = Console.ReadLine();
                    Console.Write("Remote Folder: "); remotePath = Console.ReadLine();
                    Console.Write("Clean Remote Path (Y/n): "); cleanRemotePath = new List<string>() { "y", "yes" }.Any(key => Console.ReadLine().ToLower().Equals(key));
                    Console.Write("Exist File Option (append/overwirte/skip): "); ftpExists = (FtpExists)Enum.Parse(typeof(FtpExists), Console.ReadLine().FirstCharToUpper());

                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string param = args[i].Remove(args[i].IndexOf('=') + 1);
                        string value = args[i].Substring(args[i].IndexOf('=') + 1);
                        switch (param)
                        {
                            case "--host=": host = value; break;
                            case "--port=": port = value; break;
                            case "--user=": user = value; break;
                            case "--pass=": pass = value; break;
                            case "--source=": sourcePath = Console.ReadLine(); break;
                            case "--remote=": remotePath = Console.ReadLine(); break;
                            case "--clean_remote=": cleanRemotePath = new List<string>() { "y", "yes" }.Any(key => value.ToLower().Equals(key)); i++; break;
                            case "--exist_action=": ftpExists = (FtpExists)Enum.Parse(typeof(FtpExists), value.FirstCharToUpper()); i++; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(contents: "Error input.", messageType: MessageType.ERROR);
                Log(contents: ex.Message, messageType: MessageType.ERROR);
                Console.WriteLine();
                return;
            }

            Log(contents: "Start processing");
            long beginTick = DateTime.Now.Ticks;
            if (!System.IO.Directory.Exists(sourcePath))
                Log(contents: $@"Can NOT match the source folder ""{ sourcePath }""", messageType: MessageType.ERROR);
            else
            {
                sourcePath = System.IO.Path.GetFullPath(sourcePath).Replace('\\', '/');
                remotePath = $"/{ remotePath.Replace('\\', '/').TrimStart('/') }";
                try
                {
                    var ftpClient = new FtpClient()
                    {
                        Host = host,
                        Port = string.IsNullOrEmpty(port) ? 21 : int.Parse(port),
                        RetryAttempts = 3
                    };
                    if (!string.IsNullOrEmpty(user))
                        ftpClient.Credentials = new System.Net.NetworkCredential(user, pass);
                    Log(contents: $"Try to connect { host }:{ port }");
                    
                    ftpClient.Connect();
                    Log(contents: "Connected successfully");

                    // Clean
                    if (cleanRemotePath)
                        foreach (var item in ftpClient.GetListing(remotePath))
                            for (int i = 1; i <= 3; i++)
                            {
                                try
                                {
                                    if (item.Type == FtpFileSystemObjectType.Directory)
                                        ftpClient.DeleteDirectory(item.FullName, FtpListOption.AllFiles);
                                    if (item.Type == FtpFileSystemObjectType.File)
                                        ftpClient.DeleteFile(item.FullName);

                                    Log(contents: $"Deleted: ", writeMode: WriteMode.Append);
                                    Log(contents: $"{ item.Name }{ (item.Type == FtpFileSystemObjectType.Directory ? "/" : "") }",
                                        messageType: MessageType.PATH, withTitle: false, onlyTitleColor: false);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Log(contents: $"Deleted: ", messageType: MessageType.ERROR, writeMode: WriteMode.Append);
                                    Log(contents: $"{ item.Name }{ (item.Type == FtpFileSystemObjectType.Directory ? "/" : "") }",
                                        messageType: MessageType.PATH, withTitle: false, onlyTitleColor: false);
                                    Log(contents: ex.Message, messageType: MessageType.ERROR);
                                    System.Threading.Tasks.Task.Delay(1000).Wait();
                                }
                            }

                    // Upload
                    int successCount = 0, failureCount = 0;
                    foreach (var file in System.IO.Directory.GetFiles(sourcePath, "*", System.IO.SearchOption.AllDirectories))
                    {
                        string remoteFile = file
                            .Replace('\\', '/')
                            .Replace(sourcePath, remotePath);
                        try
                        {
                            if (ftpClient.UploadFile(localPath: file, remotePath: remoteFile, createRemoteDir: true, existsMode: ftpExists, verifyOptions: FtpVerify.Retry))
                            {
                                successCount++;
                                Log(contents: $"Uploaded: ", writeMode: WriteMode.Append);
                                Log(contents: $"{ remoteFile }", messageType: MessageType.PATH, withTitle: false, onlyTitleColor: false);
                            }
                            else
                            {
                                failureCount++;
                                Log(contents: $"Uploaded: ", messageType: MessageType.ERROR, writeMode: WriteMode.Append);
                                Log(contents: $"{ remoteFile }", messageType: MessageType.PATH, withTitle: false, onlyTitleColor: false);
                            }
                        }
                        catch (Exception ex)
                        {
                            failureCount++;
                            Log(contents: $"Uploaded: ", messageType: MessageType.ERROR, writeMode: WriteMode.Append);
                            Log(contents: $"{ remoteFile }", messageType: MessageType.PATH, withTitle: false, onlyTitleColor: false);
                            Log(contents: ex.Message, messageType: MessageType.ERROR);
                        }

                    }
                    ftpClient.Disconnect();

                    // Summary
                    Log(contents: $"{ successCount + failureCount } files uploaded (",
                        messageType: MessageType.INFO, withTitle: true, writeMode: WriteMode.Append);
                    Log(contents: $"success: { successCount }",
                        messageType: MessageType.INFO, withTitle: false, onlyTitleColor: false, writeMode: WriteMode.Append);
                    Log(contents: $" / ",
                        messageType: MessageType.NONE, withTitle: false, onlyTitleColor: false, writeMode: WriteMode.Append);
                    Log(contents: $"faliure: { failureCount }",
                        messageType: MessageType.ERROR, withTitle: false, onlyTitleColor: false, writeMode: WriteMode.Append);
                    Log(contents: $") in",
                        messageType: MessageType.NONE, withTitle: false, onlyTitleColor: false, writeMode: WriteMode.Append);
                    Log(contents: $" { new TimeSpan(DateTime.Now.Ticks - beginTick).TotalSeconds.ToString("0.##") } s",
                        messageType: MessageType.TIME, withTitle: false, onlyTitleColor: false, writeMode: WriteMode.Append);
                }
                catch (Exception ex) { Log(contents: ex.Message, messageType: MessageType.ERROR); }
            }
            Console.WriteLine();
        }

        static string GetHiddenConsoleInput()
        {
            var input = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && input.Length > 0) input.Remove(input.Length - 1, 1);
                else if (key.Key != ConsoleKey.Backspace) input.Append(key.KeyChar);
            }
            return input.ToString();
        }
    }
}