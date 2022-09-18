using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using NetScript.Compiler;
using NetScript.Interpreter;
using System.Threading;

namespace NetScript.Editor
{
    public partial class Form1 : Form
    {
        public readonly TextStyle Error = new(null, Brushes.Pink, FontStyle.Bold);
        public readonly TextStyle Comment = new(Brushes.DarkGreen, null, FontStyle.Underline | FontStyle.Italic);
        public readonly TextStyle String = new(Brushes.Brown, null, FontStyle.Regular);
        public readonly TextStyle Keyword = new(Brushes.SteelBlue, null, FontStyle.Bold);

        private string Path { get; set; }
        private ConsoleTextBox IOBox { get; }
        private NetScriptCompiler Compiler { get; }
        private CancellationTokenSource Canceler { get; }
        private Task<(TimeSpan, Exception)> Execution { get; set; }

        public Form1()
        {
            InitializeComponent();

            Path = string.Empty;
            Compiler = new();
            Canceler = new();
            splitContainer1.Panel2.Controls.Add(IOBox = new ConsoleTextBox() { Dock = DockStyle.Fill });

            Editor.AddStyle(Error);
            Editor.AddStyle(Comment);
            Editor.AddStyle(String);
            Editor.AddStyle(Keyword);

            Console.SetIn(new BoxReader(IOBox));
            Console.SetOut(new BoxWriter(IOBox));
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            var r = Editor.Range;

            r.ClearFoldingMarkers();
            r.SetFoldingMarkers("{(?=\\s*(\\n|$))", "(?<=(\\n|^)\\s*)}");
            r.SetFoldingMarkers("\\((?=\\s*(\\n|$))", "(?<=(\\n|^)\\s*)\\)");
            r.SetFoldingMarkers("\\[(?=\\s*(\\n|$))", "(?<=(\\n|^)\\s*)\\]");

            r.ClearStyle(Comment, String, Keyword, Error);
            r.SetStyle(Comment, @"//.*|/\*(.|\n)*?\*/", RegexOptions.Multiline);
            r.SetStyle(String, @"""(\\.|[^""\\\n])*""|'(\\.|[^'\\\n])*'");
            r.SetStyle(Keyword, @"\b(true|false|null|is(\s+not)?|to|nameof|typeof|default|import|var|func|if|else|while|for|in|return|break|continue|loop|loaddll|object|expando|string|char|bool|byte|s?byte|u?short|u?int|u?long|float|decimal|double|Console|Math|Range)\b");
        }

        private void NewFile(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new() { Filter = "NetScript file|*.ns|Any file|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Path = dialog.FileName;
                Text = "NetScript editor - " + Path;
                File.Create(Path).Dispose();
                Editor.Clear();
            }
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new() { Filter = "NetScript file|*.ns|Any file|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Path = dialog.FileName;
                Text = "NetScript editor - " + Path;
                Editor.Text = File.ReadAllText(Path);
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Path))
            {
                SaveFileAs(sender, e);
            }
            else
            {
                File.WriteAllText(Path, Editor.Text);
            }
        }

        private void SaveFileAs(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new() { Filter = "NetScript file|*.ns|Any file|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Path = dialog.FileName;
                Text = "NetScript editor - " + Path;
                File.WriteAllText(Path, Editor.Text);
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            MemoryStream memory = new();
            IOBox.Clear();
            Editor.Range.ClearStyle(Error);

            if (Execution is not null && !Execution.IsCompleted)
            {
                Canceler.Cancel();
            }

            try
            {
                Compiler.Compile(Editor.Text, memory);
                memory.Position = 0;
            }
            catch (CompilerException ex) when (ex.Index >= 0)
            {
                string text = Editor.Text;
                int line = 0;

                for (int i = 0; i < ex.Index + 1; i++)
                {
                    if (Editor.Text[i] == '\n')
                    {
                        line++;
                    }
                }

                var r = new FastColoredTextBoxNS.Range(Editor, line);
                r.ClearStyle(Keyword);
                r.SetStyle(Error);
                IOBox.SetText(ex.Message);
                memory.Dispose();
                return;
            }
            catch (Exception ex)
            {
                IOBox.SetText("Compilation error\n\n" + ex);
                memory.Dispose();
                return;
            }

            Execution = Task.Run(() =>
            {
                DateTime execBegin = DateTime.Now;
                Exception exception = null;
                try
                {
                    InterpreterNS.Interpret(memory);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                TimeSpan execTime = DateTime.Now - execBegin;
                memory.Dispose();
                return (execTime, exception);
            }, Canceler.Token);

            ExecutionCheckTimer.Start();
        }

        private void ExecutionCheckTimer_Tick(object sender, EventArgs e)
        {
            if (Execution is null)
            {
                ExecutionCheckTimer.Stop();
            }
            else if (Execution.IsCompleted)
            {
                ExecutionCheckTimer.Stop();
                if (Execution.IsCompletedSuccessfully)
                {
                    if (Execution.Result.Item2 is null)
                    {
                        IOBox.WriteLine($"Execution completed in {Execution.Result.Item1.TotalSeconds:0.000} seconds");
                    }
                    else
                    {
                        IOBox.WriteLine(Execution.Result.Item2.ToString());
                    }
                }
                Execution = null;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Canceler.Cancel();
        }
    }

    public class BoxReader : TextReader
    {
        private ConsoleTextBox TextBox { get; }

        private delegate string Invoker();

        public BoxReader(ConsoleTextBox tbox)
        {
            TextBox = tbox;
        }

        public override string ReadLine()
        {
            return TextBox.InvokeRequired ?
                (string)TextBox.Invoke(new Invoker(() => TextBox.ReadLine())) :
                TextBox.ReadLine();
        }

        public override int Read()
        {
            return (TextBox.InvokeRequired ?
                (string)TextBox.Invoke(new Invoker(() => TextBox.ReadLine())) :
                TextBox.ReadLine()).LastOrDefault();
        }
    }

    public class BoxWriter : TextWriter
    {
        private ConsoleTextBox TextBox { get; }

        public override Encoding Encoding => Encoding.UTF8;

        private delegate void Invoker();

        public BoxWriter(ConsoleTextBox tbox)
        {
            TextBox = tbox;
        }

        public override void Write(string value)
        {
            if (TextBox.InvokeRequired)
            {
                TextBox.Invoke(new Invoker(() => Write(value)));
            }
            else
            {
                TextBox.WriteLine((value ?? string.Empty) + "\n");
            }
        }

        public override void WriteLine(string value)
        {
            if (TextBox.InvokeRequired)
            {
                TextBox.Invoke(new Invoker(() => WriteLine(value)));
            }
            else
            {
                TextBox.WriteLine(value ?? string.Empty);
            }
        }

        public override void WriteLine()
        {
            if (TextBox.InvokeRequired)
            {
                TextBox.Invoke(new Invoker(() => WriteLine()));
            }
            else
            {
                TextBox.WriteLine("\n");
            }
        }
    }
}
