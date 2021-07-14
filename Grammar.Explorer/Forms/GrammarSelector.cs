namespace Grammar.Explorer.Forms
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows.Forms;

    using Grammar.Interfaces;

    public partial class GrammarSelector : Form
    {
        public GrammarSelector(Type[] managerTypes = null)
        {
            var loadTypes = managerTypes?.Length > 0;
            if (!loadTypes)
            {
                this.Shown += (sender, e) => this.OpenFile();
            }

            this.InitializeComponent();
            if (loadTypes)
            {
                // ReSharper disable once CoVariantArrayConversion
                this.managerBox.Items.AddRange(managerTypes);
                this.managerBox.SelectedIndex = 0;
            }
        }

        public IGrammarManager GrammarManager { get; private set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void browseButton_Click(object sender, EventArgs e) => this.OpenFile();

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void managerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.okButton.Enabled = null != this.managerBox.SelectedItem;
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Event handler.")]
        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.GrammarManager = (IGrammarManager)Activator.CreateInstance((Type)this.managerBox.SelectedItem);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex) when (ex is SystemException || ex is ApplicationException)
            {
                this.messageBox.Text = $@"Cannot create an instance of the '{this.managerBox.Text}' type.";
                var index = this.managerBox.SelectedIndex;
                this.managerBox.Items.RemoveAt(index);
                var count = this.managerBox.Items.Count;
                this.managerBox.SelectedIndex = index < count ? index : count - 1;
            }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Inline out.")]
        private void OpenFile()
        {
            if (Helper.TryOpenFiles(out var fileNames, false))
            {
                var fileName = fileNames[0];
                if (!Helper.TryGetGrammarManagers(fileName, out var types))
                {
                    this.messageBox.Text = $@"Could not load '{Path.GetFileName(fileName)}' assembly.";
                    return;
                }

                if (0 == types.Length)
                {
                    this.messageBox.Text = $@"No grammar managers found in the '{Path.GetFileName(fileName)}' assembly.";
                    return;
                }

                this.messageBox.Text = string.Empty;
                this.managerBox.Items.Clear();
                // ReSharper disable once CoVariantArrayConversion
                this.managerBox.Items.AddRange(types);
                this.managerBox.SelectedIndex = 0;
            }
        }
    }
}
