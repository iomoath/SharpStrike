using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using Models.CIM;
using Models.Common;
using ServiceLayer;
using ServiceLayer.CIM;

namespace SharpStrike
{
    public partial class FormMain : Form
    {
        #region Data Members

        private readonly Ui _ui = new Ui();
        private volatile bool _busy;
        private readonly object _lock = new object();

        private Stack<string> _commandHistoryStackUp = new Stack<string>();
        private readonly Stack<string> _commandHistoryStackDown = new Stack<string>();
        private readonly List<string> _commandHistoryList = new List<string>();

        private readonly Color _consoleBackColor;
        private readonly Color _consoleForeColor;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        #endregion

        public FormMain()
        {
            InitializeComponent();
            _ui.Initialize(this);

            _consoleBackColor = Color.Black;
            _consoleForeColor = Color.White;

            consoleControl1.HorizontalScroll.Enabled = true;
            consoleControl1.VerticalScroll.Enabled = true;
        }


        #region Form Events

        private void FormMain_Load(object sender, EventArgs e)
        {
            Messenger.MessageAvailable += OnMessageAvailable;
        }
        
        private void FormMain_Shown(object sender, EventArgs e)
        {

            foreach (var s in SharedGlobals.About)
                OnMessageAvailable(this, new MessageData(string.Format("{0, 85}", s), Color.Lime, MessageData.MessageType.Good));
            
            Messages.GetCommands();
            tbCommand.Focus();

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(@"Are you sure you want to exit?", @"Exit", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        #endregion


        #region Commander

        private async void tbCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await HandleCommand();

                // Handle
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (KeyIsDown(Keys.Up))
            {
                tbCommand.Clear();


                if (_commandHistoryStackUp.Count == 0 && _commandHistoryList.Count == 0)
                    return;

                if (_commandHistoryStackUp.Count == 0)
                    _commandHistoryStackUp = new Stack<string>(_commandHistoryList);



                var c = _commandHistoryStackUp.Pop();
                _commandHistoryStackDown.Push(c);

                tbCommand.Text = c;

                e.Handled = true;
                e.SuppressKeyPress = true;

            }

            if (KeyIsDown(Keys.Down))
            {
                tbCommand.Clear();

                if (_commandHistoryStackDown.Count == 0)
                    return;


                var c = _commandHistoryStackDown.Pop();
                _commandHistoryStackUp.Push(c);

                tbCommand.Text = c;

                e.Handled = true;
                e.SuppressKeyPress = true;

            }

            if (e.Control && KeyIsDown(Keys.K))
            {
                consoleControl1.ClearOutput();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        
        private async Task HandleCommand()
        {
            if (IsAgentBusy())
                return;

            CommanderOptions options;
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                Lock();

            
                var cmd = tbCommand.Text?.Trim();
                if (string.IsNullOrEmpty(cmd))
                {
                    Messenger.RedMessage("[-] Invalid command.");
                    _ui.Invoke(() => { tbCommand.Text = string.Empty; });
                    return;
                }

                options = ParseUserOptions();
                StoreCommandInHistory(cmd);


                if (options.ShowCommands)
                {
                    ClearOutput();
                    Messages.GetCommands();
                    return;
                }

                if (options.ShowExamples)
                {
                    ClearOutput();
                    Messages.GetExamples(true);
                    return;
                }

                if (options.Test)
                {
                    Messenger.Info(@"Test method not currently supported");
                    Messages.GetExamples(true);
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
            }
        }

        private void StoreCommandInHistory(string input)
        {
            _commandHistoryList.Add(input);
            _commandHistoryStackUp = new Stack<string>(_commandHistoryList);
            _commandHistoryStackDown.Clear();

            //_commandHistoryStack.Push(input);
            //_commandHistoryStackUp.Clear();
            //_commandHistoryStackDown.Clear();

            //var tmp = new Stack<string>();

            //foreach (var c in _commandHistoryStack)
            //{
            //    tmp.Push(c);
            //}

            //while (tmp.Count > 0)
            //{
            //    _commandHistoryStackUp.Push(tmp.Pop());
            //}

            _ui.Invoke(() => {
                tbCommand.Text = string.Empty;
                tbCommand.Focus();
            });
        }

        #endregion


        #region UI Output box

        private void ClearOutput()
        {
            _ui.Invoke(() =>
            {
                consoleControl1.ClearOutput();
            });
        }

        #endregion

        #region UI Buttons

        private void btnClear_Click(object sender, EventArgs e)
        {
            consoleControl1.ClearOutput();
        }


        #endregion

        #region UI Text Changed

        private void tbUncPath_TextChanged(object sender, EventArgs e)
        {
            tbHostname.Text = tbHostname.Text?.Replace(Environment.NewLine, "");
        }

        private void tbCommand_TextChanged(object sender, EventArgs e)
        {
            tbCommand.Text = tbCommand.Text?.Replace(Environment.NewLine, "");

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text?.Replace(Environment.NewLine, "");
        }

        private void tbUsername_TextChanged(object sender, EventArgs e)
        {
            tbUsername.Text = tbUsername.Text?.Replace(Environment.NewLine, "");

        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            tbPassword.Text = tbPassword.Text?.Replace(Environment.NewLine, "");

        }

        private void tbDomain_TextChanged(object sender, EventArgs e)
        {
            tbDomain.Text = tbDomain.Text?.Replace(Environment.NewLine, "");
        }

        #endregion



        #region Execution Locker - Ensure Synchronous Execution
        
        private bool IsAgentBusy()
        {
            lock (_lock)
            {
                if (_busy)
                {
                    Messenger.BlueMessage("[*] Agent is busy..");
                }

                return _busy;
            }
        }

        private void Lock()
        {
            lock (_lock)
            {
                _busy = true;
            }
        }
        private void Unlock()
        {
            lock (_lock)
            {
                _busy = false;
            }
        }


        #endregion

        #region Helpers

        private void ResetConsoleColor()
        {
            _ui.Invoke(() =>
            {
                consoleControl1.BackColor = _consoleBackColor;
                consoleControl1.ForeColor = _consoleForeColor;
            });
        }
    
        private CommanderOptions ParseUserOptions()
        {
            var c = tbCommand.Text.Trim();
            if (!c.StartsWith("-c"))
                c = $"-c {c}";

            var args = Helpers.ParseCommandLineArgs(c);

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

            if (string.IsNullOrEmpty(options.Password) && string.IsNullOrEmpty(options.Username))
            {
                options.System = tbHostname.Text?.Trim();
                options.Username = tbUsername.Text?.Trim();
                options.Password = tbPassword.Text?.Trim();
                options.Domain = tbDomain.Text?.Trim();
            }


            if (options.System == "localhost")
            {
                options.System = null;
                options.Username = null;
                options.Domain = null;
                options.Password = null;
            }


            var cmd = tbCommand.Text.Trim().Split(' ');
            var baseCmd = cmd[0].Trim();

            if (baseCmd.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
            {
                options.Reset = true;
                options.Command = null;
                options.Execute = null;
            }
            else if (baseCmd.Equals("show-examples", StringComparison.InvariantCultureIgnoreCase))
            {
                options.ShowExamples = true;
            }
            else if (baseCmd.Equals("show-commands", StringComparison.InvariantCultureIgnoreCase))
            {
                options.ShowCommands = true;
            }
            else if (baseCmd.Equals("enable-fallback", StringComparison.InvariantCultureIgnoreCase))
            {
                options.Fallback = true;
            }


            options.Verbose = chkVerbose.Checked;
            options.Cim = radioUseCIM.Checked;
            options.NameSpace = Commands.GetCommandNameSpace(options.Command);

            return options;
        }

        
        private void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
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
        }


        private bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        
        #endregion


        #region Events Handlers

        private void OnMessageAvailable(object sender, MessageData e)
        {
            _ui.Invoke(() =>
            {
                consoleControl1.WriteOutput(e.Text, e.ForegroundColor);
                consoleControl1.WriteOutput(Environment.NewLine, e.ForegroundColor);

                consoleControl1.InternalRichTextBox.ScrollToCaret();
            });


            ResetConsoleColor();
        }

        #endregion


    }
}
