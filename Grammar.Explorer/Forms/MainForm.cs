namespace Grammar.Explorer.Forms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Hosts;
    using Grammar.Interfaces;

    using Module = Interfaces.Module;

    public partial class MainForm : Form, Helper.ICancellationSourceOwner
    {
        private readonly List<Task> savingTasks = new List<Task>();
        private readonly StringBuilder resultBuilder = new StringBuilder();
        private readonly CancellationTokenSource savingCancellationSrc = new CancellationTokenSource();
        private readonly TextWriter outputWriter;
        private readonly PythonHost pythonHost;
        private CancellationTokenSource validationCancellationSrc;
        private CancellationTokenSource executionCancellationSrc;

        private IGrammarManager grammarManager;
        private Eval selectedDelegate;
        private GetArg selectedGetArg;
        private object selectedArg;

        ref CancellationTokenSource Helper.ICancellationSourceOwner.this[Enum source]
        {
            get
            {
                switch (source)
                {
                    case CancellationSource.Validation:
                        return ref this.validationCancellationSrc;
                    case CancellationSource.Execution:
                        return ref this.executionCancellationSrc;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(source));
                }
            }
        }

        public MainForm(Type[] managerTypes = null)
        {
            this.InitializeComponent(managerTypes);
            this.outputWriter = TextWriter.Synchronized(new StringWriter(this.resultBuilder));
            this.pythonHost = new PythonHost(this.outputWriter);
        }

        private enum CancellationSource
        {
            Validation,
            Execution
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private async void compile_Click(object sender, EventArgs e)
        {
            var src = this.sourceCode.Text;
            try
            {
                var module = await Task.Run(() => this.grammarManager.GetModule(src, this.outputWriter));
                this.AddModules(new[] { module });
            }
            catch (ArgumentException exc)
            {
                await this.outputWriter.WriteLineAsync($"[{exc.GetType().Name}]: {exc.Message}");
            }
            finally
            {
                this.SetResult();
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private async void calcButton_Click(object sender, EventArgs e)
        {
            var @delegate = this.selectedDelegate;
            if (null == @delegate)
            {
                throw new InvalidOperationException("Unexpected null evaluation delegate.");
            }

            var arg = this.selectedArg;
            this.resultText.Text = await Task.Run(() => @delegate(arg));
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private async void saveButton_Click(object sender, EventArgs e)
        {
            string fileName;
            var item = (Module)this.moduleBox.SelectedItem;
            var name = item.Name;
            var bytes = item.RawAssembly;
            using (var saveFileDialog = new SaveFileDialog
                                            {
                                                AddExtension = true,
                                                DefaultExt = "dll",
                                                FileName = $"{name}.dll",
                                                Filter = @".dll|*.dll",
                                                OverwritePrompt = true
                                            })
            {
                if (DialogResult.OK == saveFileDialog.ShowDialog()
                    && !string.IsNullOrWhiteSpace(fileName = saveFileDialog.FileName))
                {
                    var synchronizationContext = SynchronizationContext.Current;
                    var saveTask = Helper.WriteBytesAsync(fileName, bytes, this.savingCancellationSrc.Token)
                        .ContinueWith(
                            antecendent =>
                                {
                                    switch (antecendent.Exception?.InnerException)
                                    {
                                        case IOException ioException:
                                            this.outputWriter.WriteLine(ioException.Message);
                                            break;
                                        case Exception exception:
                                            throw exception;
                                        default: // null => Cancelled
                                            File.Delete(fileName);
                                            break;
                                    }
                                },
                            TaskContinuationOptions.NotOnRanToCompletion);
                    this.savingTasks.Add(saveTask);
                    try
                    {
                        await saveTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // WriteBytesAsync ran to completion.
                        await this.outputWriter.WriteLineAsync($"Module '{name}' saved successfully to the '{fileName}' file.").ConfigureAwait(false);
                    }

                    this.savingTasks.Remove(saveTask);
                    synchronizationContext.Post(_ => this.SetResult(), null);
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private async void loadButton_Click(object sender, EventArgs e)
        {
            if (!Helper.TryOpenFiles(out var fileNames))
            {
                return;
            }

            var loaded = new ConcurrentBag<Module>();
            await Task.Run(
                () =>
                    {
                        Parallel.ForEach(
                            fileNames,
                            fileName =>
                                {
                                    try
                                    {
                                        var types = Assembly.Load(fileName).GetTypes();
                                        this.grammarManager.AddModulesTo(loaded, types, this.outputWriter);
                                    }
                                    catch (SystemException exc) when (exc is ReflectionTypeLoadException || exc is BadImageFormatException)
                                    {
                                        this.outputWriter.WriteLine("Could not load '{0}' assembly.", Path.GetFileName(fileName));
                                    }
                                });
                    });
            this.AddModules(loaded);
            this.SetResult();
        }

#pragma warning disable 4014
        /*
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void execButton_Click(object sender, EventArgs e)
        {
            var pythonSrc = this.pythonCode.Text;
            Helper.WrapCancellation(
                async ct =>
                    {
                        var result = await this.pythonHost.ExecuteCodeAsync(pythonSrc, ct);
                        if (!string.IsNullOrEmpty(result))
                        {
                            this.pythonCode.Text = result;
                        }

                        this.SetResult();
                    },
                this,
                CancellationSource.Execution,
                catchOperationCancelled: true);
        }
        */

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void execButton_Click(object sender, EventArgs e)
        {
            var pythonSrc = this.pythonCode.Text;
            Helper.WrapCancellation(
                ct => this.pythonHost.ExecuteCodeAsync(pythonSrc, ct).ContinueWith(
                    antecendent =>
                        {
                            var result = antecendent.Result;
                            if (!string.IsNullOrEmpty(result))
                            {
                                this.pythonCode.Text = result;
                            }

                            this.SetResult();
                        },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.FromCurrentSynchronizationContext()),
                this,
                CancellationSource.Execution,
                catchOperationCancelled: true);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void typeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (Module)this.moduleBox.SelectedItem;
            this.selectedGetArg = item.GetArg;
            this.selectedDelegate = item.Delegate;

            this.descText.Text = item.Description;
            this.varText.Text = item.Arguments;
            this.sourceCode.Text = item.SourceCode;

            this.SetButtonsEnabled(this.selectedGetArg, this.selectedDelegate, null != item.RawAssembly);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void varBox_TextChanged(object sender, EventArgs e)
        {
            this.SetButtonsEnabled(this.selectedGetArg, this.selectedDelegate);
        }
#pragma warning restore 4014

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            this.Enabled = false;
            this.savingCancellationSrc.Cancel();
            if (this.savingTasks.Count > 0)
            {
                e.Cancel = true;
                await Task.WhenAny(Task.WhenAll(this.savingTasks), Task.Delay(5_000));
                this.FormClosing -= this.MainForm_FormClosing;
                this.Close();
            }
        }

        private EventHandler GetFormShown(Type[] managerTypes)
        {
            void MainForm_FormShown(object sender, EventArgs e)
            {
                using (var selectorForm = new GrammarSelector(managerTypes))
                {
                    if (selectorForm.ShowDialog() != DialogResult.OK)
                    {
                        this.Close();
                        return;
                    }

                    this.grammarManager = selectorForm.GrammarManager;
                }

                this.Enabled = true;
            }

            if (1 == managerTypes?.Length)
            {
                try
                {
                    this.grammarManager = (IGrammarManager)Activator.CreateInstance(managerTypes[0]);
                    this.Enabled = true;
                }
                catch (Exception ex) when (ex is SystemException || ex is ApplicationException)
                {
                    managerTypes = null;
                }
            }

            return null == this.grammarManager ? MainForm_FormShown : new EventHandler((sender, e) => { });
        }

        private async Task SetButtonsEnabled(GetArg getArg, Eval @delegate, bool? bytesAvailable = null)
        {
            if (bytesAvailable.HasValue)
            {
                this.saveButton.Enabled = bytesAvailable.Value;
            }

            if (null == getArg || null == @delegate)
            {
                return;
            }

            var expression = this.varBox.Text;
            try
            {
                await Helper.WrapCancellation(
                    async ct =>
                        {
                            var arg = await Task.Run(() => getArg(expression), ct);
                            ct.ThrowIfCancellationRequested();
                            this.selectedArg = arg;
                            this.calcButton.Enabled = true;
                        },
                    this,
                    CancellationSource.Validation);
            }
            catch (FormatException)
            {
                this.calcButton.Enabled = false;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void AddModules(IReadOnlyCollection<Module> modules)
        {
            if (modules.Count > 0)
            {
                var index = this.moduleBox.Items.Count;
                foreach (var module in modules)
                {
                    this.pythonHost.Modules[module.Name] = module.ScriptObject;
                    this.moduleBox.Items.Add(module);
                }

                this.moduleBox.SelectedIndex = index;
            }
        }

        private void SetResult()
        {
            this.resultBox.Text = this.resultBuilder.ToString();
            this.resultBuilder.Clear();
        }
    }
}
