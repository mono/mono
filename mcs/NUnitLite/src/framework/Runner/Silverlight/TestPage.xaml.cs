using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

namespace NUnitLite.Runner.Silverlight
{
    /// <summary>
    /// TestPage is the display page for the test results
    /// </summary>
    public partial class TestPage : UserControl
    {
        private Assembly callingAssembly;
        private ITestAssemblyRunner runner;
        private TextWriter writer;

        public TestPage()
        {
            InitializeComponent();

            this.runner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder());
            this.callingAssembly = Assembly.GetCallingAssembly();
            this.writer = new TextBlockWriter(this.ScratchArea);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextUI.WriteHeader(this.writer);
            TextUI.WriteRuntimeEnvironment(this.writer);

            if (!LoadTestAssembly())
                writer.WriteLine("No tests found in assembly {0}", GetAssemblyName(callingAssembly));
            else
                Dispatcher.BeginInvoke(() => ExecuteTests());
        }

        #region Helper Methods

        private bool LoadTestAssembly()
        {
            return runner.Load(callingAssembly, new Dictionary<string, string>());
        }

        private string GetAssemblyName(Assembly assembly)
        {
            return new AssemblyName(assembly.FullName).Name;
        }

        private void ExecuteTests()
        {
            ITestResult result = runner.Run(TestListener.NULL, TestFilter.Empty);
            ResultReporter reporter = new ResultReporter(result, writer);

            reporter.ReportResults();

            ResultSummary summary = reporter.Summary;

            this.Total.Text = summary.TestCount.ToString();
            this.Failures.Text = summary.FailureCount.ToString();
            this.Errors.Text = summary.ErrorCount.ToString();
            this.NotRun.Text = summary.NotRunCount.ToString();
            this.Passed.Text = summary.PassCount.ToString();
            this.Inconclusive.Text = summary.InconclusiveCount.ToString();

            this.Notice.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
