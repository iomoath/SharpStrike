using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Models.CIM;
using Models.Common;
using ServiceLayer;
using ServiceLayer.CIM;

namespace SharpStrike
{
    internal class Program
    {
        #region Data Members
        private static volatile bool _busy;
        private static readonly object Locker = new object();

        #endregion


        #region Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                args = new [] { "--show-examples" };
                Init();
                PrintAbout();
                HandleCommand(args).Wait();
            }
            finally
            {
                Environment.Exit(1);
            }
        }

        #endregion

        #region Init

        private static void Init()
        {
            Messenger.MessageAvailable += OnMessageAvailable;

        }

        #endregion

        #region Commander
        private static async Task HandleCommand(string[] args)
        {
            if (IsAgentBusy())
                return;

            CommanderOptions options;
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                Lock();

                options = ParseUserOptions(args);

                if (options.ShowCommands)
                {
                    ClearOutput();
                    Messages.GetCommands();
                    return;
                }

                if (options.ShowExamples)
                {
                    ClearOutput();
                    Messages.GetExamples();
                    return;
                }

                if (options.Test)
                {
                    Messenger.Info(@"Test method not currently supported");
                    Messages.GetExamples();
                    return;
                }

                await Task.Run(() => { CommandHandler.Instance.Handle(options); });
            }
            catch (NullReferenceException)
            {
                Messenger.RedMessage("\n[-] Incorrect command used. Try one of these:\n");
                Messages.GetCommands();
            }
            catch (ArgumentException)
            {
                Messenger.RedMessage("\n[-] Incorrect command used. Try one of these:\n");
                Messages.GetCommands();
            }
            catch (Exception e)
            {
                ClearOutput();
                Messenger.RedMessage("[-] " + e.Message);
                Messages.GetCommands();
            }
            finally
            {
                watch.Stop();
                Messenger.Info(@"Execution time: " + watch.ElapsedMilliseconds / 1000 + @" Seconds");

                Unlock();
                Environment.Exit(1);
            }
        }


        #endregion

        #region Helpers

        private static CommanderOptions ParseUserOptions(string[] args)
        {
            // Parse arguments passed
            Parser parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.HelpWriter = null;
            });

            var options = new CommanderOptions();

            var parserResult = parser.ParseArguments<CommanderOptions>(args);

            parserResult.WithParsed(o => { options = o; }).WithNotParsed(errs => DisplayHelp(parserResult, errs));

          
            if (options.System == "localhost")
            {
                options.System = null;
                options.Username = null;
                options.Domain = null;
                options.Password = null;
            }
            
            
            
            options.NameSpace = Commands.GetCommandNameSpace(options.Command);

            return options;
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            HelpText helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = SharedGlobals.About[0];
                h.Copyright = SharedGlobals.About[1];
                h.AutoVersion = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);

            Messenger.Info(helpText);
            Environment.Exit(1);
        }

        private static void ClearOutput()
        {
            Console.Clear();
        }

        #endregion

        #region Execution Locker - Ensure Synchronous Execution

        private static bool IsAgentBusy()
        {
            lock (Locker)
            {
                if (_busy)
                {
                    Messenger.BlueMessage("[*] Agent is busy..");
                }

                return _busy;
            }
        }

        private static void Lock()
        {
            lock (Locker)
            {
                _busy = true;
            }
        }
        private static void Unlock()
        {
            lock (Locker)
            {
                _busy = false;
            }
        }
        #endregion

        #region Events Handlers

        private static void OnMessageAvailable(object sender, MessageData e)
        {
            //if (Console.BackgroundColor == ConsoleColor.Black)
            //{
            //    Console.ForegroundColor = e.ForegroundColor;
            //}
            if (e.ForegroundColor == Color.Black)
                Console.ForegroundColor = ConsoleColor.Black;
            else if (e.ForegroundColor == Color.Red)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (e.ForegroundColor == Color.Cyan)
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (e.ForegroundColor == Color.Yellow)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (e.ForegroundColor == Color.Lime)
                Console.ForegroundColor = ConsoleColor.Green;
            

            Console.WriteLine(e.Text);
            Console.ResetColor();
        }

        #endregion

        #region Print Version About Text

        private static void PrintAbout()
        {
            if (Console.BackgroundColor == ConsoleColor.Black)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            foreach (var s in SharedGlobals.About)
            {
                Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
                Console.WriteLine(s);
            }

            Console.WriteLine();
            Console.ResetColor();
        }

        #endregion
    }
}
