// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;

namespace System.Workflow.Activities.Rules.Design
{
    public partial class RuleConditionDialog : Form
    {
        RuleExpressionCondition ruleExpressionCondition = new RuleExpressionCondition();
        private IServiceProvider serviceProvider;
        private Parser ruleParser;
        private Exception syntaxException;
        private bool wasOKed;

        public RuleConditionDialog(Activity activity, CodeExpression expression)
        {
            if (activity == null)
                throw (new ArgumentNullException("activity"));

            InitializeComponent();

            ITypeProvider typeProvider;
            serviceProvider = activity.Site;
            if (serviceProvider != null)
            {
                IUIService uisvc = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (uisvc != null)
                    this.Font = (Font)uisvc.Styles["DialogFont"];
                typeProvider = (ITypeProvider)serviceProvider.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ITypeProvider).FullName);
                    throw new InvalidOperationException(message);
                }

                WorkflowDesignerLoader loader = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (loader != null)
                    loader.Flush();
            }
            else
            {
                // no service provider, so make a TypeProvider that has all loaded Assemblies
                TypeProvider newProvider = new TypeProvider(null);
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    newProvider.AddAssembly(a);
                typeProvider = newProvider;
            }

            RuleValidation validation = new RuleValidation(activity, typeProvider, false);
            this.ruleParser = new Parser(validation);

            InitializeDialog(expression);
        }

        public RuleConditionDialog(Type activityType, ITypeProvider typeProvider, CodeExpression expression)
        {
            if (activityType == null)
                throw (new ArgumentNullException("activityType"));

            InitializeComponent();

            RuleValidation validation = new RuleValidation(activityType, typeProvider);
            this.ruleParser = new Parser(validation);

            InitializeDialog(expression);
        }

        private void InitializeDialog(CodeExpression expression)
        {
            HelpRequested += new HelpEventHandler(OnHelpRequested);
            HelpButtonClicked += new CancelEventHandler(OnHelpClicked);

            if (expression != null)
            {
                this.ruleExpressionCondition.Expression = RuleExpressionWalker.Clone(expression);
                this.conditionTextBox.Text = ruleExpressionCondition.ToString().Replace("\n", "\r\n");
            }
            else
                this.conditionTextBox.Text = string.Empty;

            this.conditionTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(ConditionTextBox_PopulateAutoCompleteList);
            this.conditionTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(ConditionTextBox_PopulateAutoCompleteList);

            try
            {
                this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
            }
            catch (RuleSyntaxException ex)
            {
                conditionErrorProvider.SetError(this.conditionTextBox, ex.Message);
            }
        }


        public CodeExpression Expression
        {
            get
            {
                return this.ruleExpressionCondition.Expression;
            }
        }


        private void ConditionTextBox_PopulateAutoCompleteList(object sender, AutoCompletionEventArgs e)
        {
            e.AutoCompleteValues = this.ruleParser.GetExpressionCompletions(e.Prefix);
        }


        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void conditionTextBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                this.ruleExpressionCondition = (RuleExpressionCondition)this.ruleParser.ParseCondition(this.conditionTextBox.Text);

                if (!string.IsNullOrEmpty(this.conditionTextBox.Text))
                    this.conditionTextBox.Text = this.ruleExpressionCondition.ToString().Replace("\n", "\r\n");
                conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                syntaxException = null;
            }
            catch (Exception ex)
            {
                syntaxException = ex;
                conditionErrorProvider.SetError(this.conditionTextBox, ex.Message);
            }
        }


        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ShowHelp();
        }


        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            ShowHelp();
        }


        private void ShowHelp()
        {
            if (serviceProvider != null)
            {
                IHelpService helpService = serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
                if (helpService != null)
                {
                    helpService.ShowHelpFromKeyword(this.GetType().FullName + ".UI");
                }
                else
                {
                    IUIService uisvc = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if (uisvc != null)
                        uisvc.ShowError(Messages.NoHelp);
                }
            }
            else
            {
                IUIService uisvc = (IUIService)GetService(typeof(IUIService));
                if (uisvc != null)
                    uisvc.ShowError(Messages.NoHelp);
            }
        }


        private void okButton_Click(object sender, EventArgs e)
        {
            wasOKed = true;
        }

        private void RuleConditionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wasOKed && syntaxException != null)
            {
                e.Cancel = true;
                DesignerHelpers.DisplayError(Messages.Error_ConditionParser + "\n" + syntaxException.Message, this.Text, this.serviceProvider);
                if (syntaxException is RuleSyntaxException)
                    this.conditionTextBox.SelectionStart = ((RuleSyntaxException)syntaxException).Position;
                this.conditionTextBox.SelectionLength = 0;
                this.conditionTextBox.ScrollToCaret();
                wasOKed = false;
            }
        }
    }
}
