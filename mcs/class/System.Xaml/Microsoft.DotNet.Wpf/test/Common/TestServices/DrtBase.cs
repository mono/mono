using System;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Automation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text;

using Microsoft.Win32;
using Xunit.Abstractions;
using MS.Internal;

/************************************************************************

The DrtBase class provides a handy framework for running DRTs.  Here's some
terminology we use in the following explanation:

    DRT - a single exectuable, e.g. DrtUiBind.exe.  It comprises one or more
        test suites.
    Test suite - a list of DRT tests, usually related to each other.  Often
        the suite comprises all the tests that act on a given tree (which is
        either created in initialization code, or is loaded via the parser from
        a .xaml file).
    DRT test - a single method, part of a test suite.  After each test, the
        dispatcher runs to react to all pending asynchronous work the test
        may have caused.

The intent is that each suite starts off by creating (or loading) a tree, then
acts on that tree in various ways.  Often the actions (tests) occur in pairs:
one test that does something, and another test that verifies that Avalon did
the right thing in response.  Putting these in separate "tests" permits the
asynchronous parts of Avalon to run in between.

Here's how to use DrtBase.

1. Create a directory for your DRT.  E.g. devtest\drts\xxx
2. Include this file (DrtBase.cs) in the 'sources' list, using a relative path.
3. Write a class that derives from DrtBase.  Override virtuals as needed.
Follow this example:

    public sealed class MyDRTClass : DrtBase
    {
        public static int Main(string[] args)
        {
            DrtBase drt = new MyDRTClass();
            return drt.Run(args);
        }

        private MyDRTClass()
        {
            WindowTitle = "My DRT";
            Suites = new DrtTestSuite[]{
                        new MyFirstTestSuite(),
                        new MySecondTestSuite(),    // repeat as needed
                        null            // list terminator - optional
                        };
        }

        // Override this in derived classes to handle command-line arguments one-by-one.
        // Return true if handled.
        protected override bool HandleCommandLineArgument(string arg, bool option, string[] args, ref int k)
        {
            // start by giving the base class the first chance
            if (base.HandleCommandLineArgument(arg, option, args, ref k))
                return true;

            // process your own arguments here, using these parameters:
            //      arg     - current argument
            //      option  - true if there was a leading - or /.
            //      args    - all arguments
            //      k       - current index into args
            // Here's a typical sketch:

            if (option)
            {
                switch (arg)    // arg is lower-case, no leading - or /
                {
                    case "foo":             // simple boolean option:   -foo
                        _foo = true;
                        break;

                    case "use":             // option with parameter:  -use something
                        _something = args[++k];
                        break;

                    default:                // unknown option.  don't handle it
                        return false;
                }
                return true;
            }
            else                            // non-option argument:   <filename>
            {
                _files.Add(arg);
                return true;
            }

            return false;
        }

        // Print a description of command line arguments.  Derived classes should
        // override this to describe their own arguments, and then call
        // base.PrintOptions() to get the DrtBase description.
        protected override void PrintOptions()
        {
            Console.WriteLine("Options:");
            Console.WriteLine("  filename ...  examine named files");
            Console.WriteLine("  -foo          enable the foo flag");
            Console.WriteLine("  -use something   use the named thing");
            base.PrintOptions();
        }

4. Write a class that derives from DrtTestSuite.  Follow this example:
    public sealed class MyTestSuite : DrtTestSuite
    {
        public MyTestSuite() : base("MySuiteName")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            // initialize the suite here.  This includes loading the tree.

            // For example to create a tree via code:
            Visual root = CreateMyTree();
            DRT.Show(root);

            // Or to load a tree from a .xaml file:
            DRT.LoadXamlFile(@"MyTree.xaml");

            // return the lists of tests to run against the tree
            return new DrtTest[]{

                        new DrtTest( DoSomething ),
                        new DrtTest( VerifySomething ),

                        new DrtTest( DoSomethingSychronousAndVerifyItRightAway ),

                        // repeat as needed

                        null        // list terminator - optional
                        };
        }

        // Testing an action that Avalon reacts to asynchronously:
        void DoSomething()
        {
            // your action goes here
        }

        void VerifySomething()
        {
            // check that Avalon reacted correctly.  Assert if it didn't:
            DRT.Assert(condition, "message");
            DRT.AssertEqual(expected, actual, "message");
        }

        // Testing an action that Avalon reacts to synchronously:
        void DoSomethingSychronousAndVerifyItRightAway()
        {
            // your action
            // your verification
        }
    }
5. Compile it all and run.
6. DrtBase provides various other common services you can use via the DRT
property of your suite.  Look at the public properties/methods/events below.

About logging:

DrtBase handles logging in a very nice way. Feel free to sprinkle Console.WriteLines
throughout your DRT. By default, they will be buffered into a StringBuilder, and they
will not actually be displayed unless an exception occurs.

If you specify the -verbose command line option, all messages will be printed immediately.

Finally, you can use DrtBase's Verbose property to condition some of your output, e.g.:

    if (Verbose)
        Console.WriteLine("Really detailed output");

Such messages will not be printed even if an exception occurs. But they -will- be
printed if you specify the -verbose command line option.

************************************************************************/

namespace DRT
{
    // base class for a DRT application
    public abstract partial class DrtBase
    {
        //------------------------------------------------------
        //
        //  Public P/M/E
        //
        //------------------------------------------------------

        static DrtBase()
        {
            Application app = Application.Current;  // runs Application.Init(), which sets up Avalon services
        }

        protected DrtBase(ITestOutputHelper output)
        {
            string assertExit = Environment.GetEnvironmentVariable("AVALON_ASSERTEXIT");
            if (assertExit != null && assertExit.Length > 0)
            {
                _loudAsserts = true;
                _assertsAsExceptions = false;
                _catchExceptions = true;
            }

            _piHasUnhandledExceptionHandler = typeof(Dispatcher).GetProperty("HasUnhandledExceptionHandler",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (_piHasUnhandledExceptionHandler == null)
            {
                throw new InvalidOperationException("Cannot find property Dispatcher.HasUnhandledExceptionHandler");
            }

            _dispatcher = Dispatcher.CurrentDispatcher;
            _output = output;
        }

        /// <summary>
        /// Run the DRT.  This means:
        ///  1. process command-line arguments
        ///  2. create a top-level window and a Dispatcher
        ///  3. run the dispatcher
        ///  4. run each test suite under control of the dispatcher
        ///  5. catch any unhandled exceptions
        ///  6. shut down when the dispatcher returns, or on WM_CLOSE
        //
        /// </summary>
        /// <param name="args">command-line arguments</param>
        /// <returns>DRT exit code -- to be returned from Main()</returns>
        public int Run(string[] args)
        {
            _delayedConsoleOutput = new StringAndFileWriter(DrtName, _output);

            _retcode = ReadCommandLineArguments(args);

            if (_retcode != 0)
                return _retcode;

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "DRT Main";
            }

            // Add some well-known assemblies to the "used" list.  This helps catch
            // problems where the DRT fails because it's using the wrong version
            // of Avalon assemblies (this can happen when the user's environment
            // has some mixture of system GAC, razzle GAC, and local files).
            //
            // Authors of derived DRT classes can make additional calls to UseType
            // (after creating the DRT object, and before calling Run on it) if
            // they use any other assemblies.
            UseType(typeof(FrameworkElement));      // PresentationFramework.dll
            UseType(typeof(Visual));                // PresentationCore.dll
            UseType(typeof(Dispatcher));            // WindowsBase.dll
            UseType(typeof(AutomationElement));        // WindowsUIAutomation.dll
            UseType(typeof(DrtBase));               // current .exe

            if (_reportInfo)
            {
                ReportDrtInfo();
                return _retcode;
            }

            if (CatchExceptions)
            {
                // log unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
                try
                {
                    RunCore();
                }
                catch (Exception e)
                {
                    WriteDelayedOutput();
                    ReportException(e);
                }
                finally
                {
                    DelayOutput = false;
                }
            }
            else
            {
                if (DelayOutput)
                {
                    bool exceptionThrown = true;
                    try
                    {
                        RunCore();
                        exceptionThrown = false;
                    }
                    finally
                    {
                        if (exceptionThrown)
                        {
                            WriteDelayedOutput();
                        }
                        DelayOutput = false;
                    }
                }
                else
                {
                    RunCore();
                }
            }

            _delayedConsoleOutput.CloseFile();

            bool fFailed = _totalFailures > 0 || _retcode != 0;
            Console.WriteLine(fFailed ? "FAILED" : "SUCCEEDED");

            return _retcode;
        }

        void RunCore()
        {
            try
            {
                if (_blockInput)
                {
                    Win32BlockInput(true);
                }

                // Make sure if this DRT sends any input that it will reset the screen saver.
                MakeSendInputResetScreenSaver();
                DisableProcessWindowsGhosting();

                _drt = this;
                _drtStarted = true;

                SetConsoleOutput();

                // run the tests
                _dispatcher.BeginInvoke(
                    _testPriority,
                    new DispatcherOperationCallback(DoSetup),
                    null
                    );

                Dispatcher.Run();
            }
            finally
            {
                if (_blockInput)
                {
                    Win32BlockInput(false);
                }
            }
        }

        /// <summary>
        /// Process command line arguments.
        /// </summary>
        private int ReadCommandLineArguments(string[] args)
        {
            for (int k = 0; k < args.Length; ++k)
            {
                string arg = args[k];
                bool option = (arg[0] == '-' || arg[0] == '/');
                if (option)
                    arg = arg.Substring(1).ToLower();

                if (HandleCommandLineArgument(arg, option, args, ref k))
                    continue;

                bool handled = false;
                foreach (DrtTestSuite suite in _suites)
                {
                    if (suite == null)
                        continue;
                    if (handled = suite.HandleCommandLineArgument(arg, option, args, ref k))
                        break;
                }

                if (!handled)
                {
                    Console.WriteLine("Unrecognized {0}: {1}", option ? "option" : "argument", arg);
                    PrintUsage();
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Override this in derived classes to handle command-line arguments one-by-one.
        /// </summary>
        /// <param name="arg">current argument</param>
        /// <param name="option">if there was a leading "-" or "/" to arg</param>
        /// <param name="args">the array of command line arguments</param>
        /// <param name="k">current index in the argument array.  passed by ref so you can increase it to "consume" arguments to options.</param>
        /// <returns>True if handled</returns>
        protected virtual bool HandleCommandLineArgument(string arg, bool option, string[] args, ref int k)
        {
            if (option)
            {
                switch (arg)
                {
                    case "?":
                    case "help":
                        PrintUsage();

                        Xunit.Assert.True(true);
                        Environment.Exit(0);
                        break;

                    case "k":
                    case "hold":
                        _keepAlive = true;
                        BlockInput = false;
                        break;

                    case "verbose":
                        _verbose = true;
                        DelayOutput = false;
                        break;

                    case "catchexceptions":
                        CatchExceptions = true;
                        break;

                    case "suite":
                        SelectSuite(args[++k]);
                        break;

                    case "skip":
                        DisableSuite(args[++k]);
                        break;

                    case "trace":
                        DelayOutput = false;
                        EnableTracing();
                        break;

                    case "info":
                        _reportInfo = true;
                        break;

                    case "wait":
                        Console.WriteLine("Attach debugger now.  Press return to continue.");
                        Console.ReadLine();
                        break;

                    case "quietasserts":
                        _loudAsserts = false;
                        break;

                    case "loudasserts":
                        _loudAsserts = true;
                        break;

                    case "record":
                        {
                            //
                            // MILCE: enable recording of the command stream
                            //

                            Assembly presentationCore = Assembly.GetAssembly(typeof(System.Windows.Media.Visual));
                            Type mediaSystem = presentationCore.GetType("System.Windows.Media.MediaSystem");
                            mediaSystem.InvokeMember("ForceRecord", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.SetProperty, null, null, new object[] { true });
                        }
                        break;
                    case "rmclient":
                        //This is added for DrtCompressionXForm. This option is used only on machines where client RM is setup. So this only need to be recognized in certain situations. Not intended for everyday DRTs.
                        break;
                    default:
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Print a description of command line arguments.  Derived classes should
        /// override this to describe their own arguments, and then call
        /// base.PrintOptions() to get the DrtBase description.
        /// </summary>
        protected virtual void PrintOptions()
        {
            Console.WriteLine("General options:");
            Console.WriteLine("  -k or -hold       Keep window alive after tests are done");
            Console.WriteLine("  -verbose          produce verbose output");
            Console.WriteLine("  -suite name       run the suite with the given name");
            Console.WriteLine("  -suite nnn        run the suite with the given index");
            Console.WriteLine("  -skip name        skip the suite with the given name");
            Console.WriteLine("  -skip nnn         skip the suite with the given index");
            Console.WriteLine("  -trace            enable tracing");
            Console.WriteLine("  -wait             delay to allow debugger attach before start tests");
            Console.WriteLine("  -catchexceptions  catch exceptions (don't show JIT dialog)");
            Console.WriteLine("  -quietasserts     make asserts fail with a Console.WriteLine and continue");
            Console.WriteLine("  -loudasserts      make asserts fail loudly (with assert message box)");
            Console.WriteLine("  -record           MILCE: record the command stream");
            Console.WriteLine("  -info             report environment info - do not run DRT");
        }

        /// <summary>
        /// Called when the DRT is starting up -- after the Dispatcher has been created,
        /// before any suites are started.
        /// </summary>
        protected virtual void OnStartingUp()
        {
        }

        /// <summary>
        /// Called when the last suite finishes before the DRT terminates.
        /// </summary>
        protected virtual void OnShuttingDown()
        {
        }

        /// <summary>
        /// The error code returned by the process when it terminates
        /// </summary>
        public int ReturnCode
        {
            get { return _retcode; }
            set { _retcode = value; }
        }


        /// <summary>
        /// The master list of test suites.  Derived classes should set this
        /// from the constructor.
        /// </summary>
        protected DrtTestSuite[] Suites
        {
            get { return _suites; }
            set { _suites = value; }
        }

        /// <summary>
        /// Add a suite to the 'selected' list.
        /// </summary>
        /// <param name="name">Name of the suite</param>
        protected void SelectSuite(string name)
        {
            int index = IndexFromString(name);

            if (index == -1)
                _selectedSuites.Add(name);
            else
                _selectedSuites.Add(index);
        }

        /// <summary>
        /// Add a suite to the 'selected' list.
        /// </summary>
        /// <param name="index">Index of the suite</param>
        protected void SelectSuite(int index)
        {
            _selectedSuites.Add(index);
        }

        /// <summary>
        /// Add a suite to the 'disabled' list.
        /// </summary>
        /// <param name="name">Name of the suite.</param>
        protected void DisableSuite(string name)
        {
            int index = IndexFromString(name);

            if (index == -1)
                _disabledSuites.Add(name);
            else
                _disabledSuites.Add(index);
        }

        /// <summary>
        /// Add a suite to the 'disabled' list.
        /// </summary>
        /// <param name="index">Index of the suite.</param>
        protected void DisableSuite(int index)
        {
            _disabledSuites.Add(index);
        }


        #region DRT settings (set these before DRT.Run)

        /// <summary>
        /// Produce verbose output.
        /// </summary>
        public bool Verbose
        {
            get { return _verbose;  }
            set { _verbose = value; }
        }

        /// <summary>
        ///
        /// </summary>
        public int AssertsInSuite { get { return _cAssertsInSuite; } }

        /// <summary>
        /// Keep window alive after tests complete.
        /// </summary>
        public bool KeepAlive
        {
            get { return _keepAlive;  }
            set { _keepAlive = value; }
        }

        public bool DontStartSuites
        {
            get { return _dontStartSuites; }
            set { _dontStartSuites = value; }
        }

        /// <summary>
        /// Block input (important for DRTs which need to send input).  Prevent user from sending any input to the system.  (Press CTRL-ALT-DEL to unblock if you need to).
        /// </summary>
        public bool BlockInput
        {
            get { return _blockInput; }
            set
            {
                if (_blockInput != value)
                {
                    _blockInput = value;
                    if (_blockInput)
                    {
                        // Only block if the drt has started.  Otherwise we'll block at the beginning of Run.
                        if (_drtStarted)
                        {
                            Win32BlockInput(true);
                        }
                    }
                    else
                    {
                        Win32BlockInput(false);
                    }
                }
            }
        }

        private void OnResetInputState()
        {
            if (ResetInputState != null)
            {
                ResetInputState(this, EventArgs.Empty);
            }
        }

        protected event EventHandler ResetInputState;

        /// <summary>
        /// True if this DRT wants asserts dialog boxes to be raised on error
        /// (false will display all failures that happen in the end of the test).
        /// </summary>
        protected bool LoudAsserts { get { return _loudAsserts; } set { _loudAsserts = value; } }

        /// <summary>
        /// True if this DRT wants exceptions caught and logged to the console instead of allowing debugger attach.
        /// </summary>
        protected bool CatchExceptions { get { return _catchExceptions; } set { _catchExceptions = value; } }

        /// <summary>
        /// True if Console.Out should be redirected to a StringWriter.
        /// </summary>
        protected bool DelayOutput
        {
            get { return _delayOutput; }
            set
            {
                _delayOutput = value;
                SetConsoleOutput();
            }
        }

        private void SetConsoleOutput()
        {
            if (!_drtStarted)
                return;

            if (_delayOutput)
            {
                _delayedConsoleOutput.BufferOutput = true;
                Console.SetOut(_delayedConsoleOutput);
            }
            else
            {
                _delayedConsoleOutput.BufferOutput = false;
                Console.SetOut(_delayedConsoleOutput);
            }
        }

        /// <summary>
        /// If true, DRT.Assert will throw exceptions instead of calling Debug.Assert
        /// </summary>
        protected bool AssertsAsExceptions
        {
            get { return _assertsAsExceptions; }
            set { _assertsAsExceptions = value; }
        }

        /// <summary>
        /// Set the window title.  Only works if called before DRT.Run().
        /// </summary>
        protected string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; }
        }

        /// <summary>
        /// Set the window size.  Only works if called before DRT.Run();
        /// </summary>
        protected Size WindowSize
        {
            get { return _windowSize; }
            set { _windowSize = value; }
        }

        /// <summary>
        ///     Set the window position.  Only works if called before DRT.Run();
        /// </summary>
        protected Point WindowPosition
        {
            get { return _windowPosition; }
            set { _windowPosition = value; }
        }

        /// <summary>
        /// Set whether or not this window should be topmost.  Must be set before the window is created.
        /// </summary>
        /// <value></value>
        protected bool TopMost
        {
            get { return _topMost; }
            set { _topMost = value; }
        }

        #endregion

        #region Public DRT properties

        /// <summary>
        /// The root element.
        /// </summary>
        public Visual RootElement
        {
            get { return _rootElement; }
            set { _rootElement = value; }
        }

        /// <summary>
        /// The DRT main window as a AutomationElement.
        /// </summary>
        public AutomationElement LogicalRoot
        {
            get
            {
                if (_logicalRoot == null)
                {
                    _logicalRoot = AutomationElement.FromHandle(MainWindow.Handle);
                }

                return _logicalRoot;
            }
        }

        /// <summary>
        /// Return the real Console.Out (useful if using DelayOutput, where Console.Out points to a string builder).
        /// </summary>
        public TextWriter ConsoleOut
        {
            get
            {
                if (DelayOutput)
                {
                    // Return a wrapper that writes both to the console and the buffer & log
                    return new DualWriter(_standardConsoleOutput, _delayedConsoleOutput.File);
                }
                else
                {
                    return _standardConsoleOutput;
                }
            }
        }


        /// <summary>
        /// Name of the drt
        /// </summary>
        public string DrtName
        {
            get
            {
                return _DrtName;
            }
            set
            {
                _DrtName = value;
            }
        }

        /// <summary>
        /// The base directory for relative file names.  Should end with '\'.
        /// Defaults to '.\' (i.e. current directory).
        /// </summary>
        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        /// <summary>
        /// The priority at which tests should run.  This is ApplicationIdle by
        /// default.  A suite can override this (e.g. in its PrepareTests method).
        public DispatcherPriority TestPriority
        {
            get { return _testPriority; }
            set { _testPriority = value; }
        }

        #endregion

        #region Suite Helpers

        /// <summary>
        /// Load the given .xaml file into the main window.
        /// </summary>
        /// <param name="filename">name of the file, relative to the BaseDirectory</param>
        public void LoadXamlFile(string filename)
        {
            LoadXamlFile(filename, true);
        }

        /// <summary>
        /// Load the given .xaml file (into RootElement) and optionally display it in the main window.
        /// </summary>
        /// <param name="filename">name of the file, relative to the BaseDirectory</param>
        /// <param name="show">whether to attach the root to the main window</param>
        /// <remarks>
        /// Attaching the root to the main window causes a layout pass to be run.
        /// If you need to do something with the tree before layout is run - e.g.
        /// adding event handlers, capturing pre-layout property values, etc. -
        /// you can do so by calling LoadXamlFile("myfile", false), then doing
        /// your work, and finally calling ShowRoot().
        /// </summary>
        public void LoadXamlFile(string filename, bool show)
        {

            // Some URIs require the PreloadedPackages initialization that the static Application constructor
            // performs, and some DRTs don't have an Application.  So ensure that they're initialized.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Application).TypeHandle);


            string fullname = BaseDirectory + filename;
            System.IO.Stream stream = File.OpenRead(fullname);

            RootElement = (Visual)XamlReader.Load(stream);

            if (show)
                ShowRoot();
        }

        /// <summary>
        /// Attach the current root element to the main window.
        /// </summary>
        public void ShowRoot()
        {
            MainWindow.RootVisual = RootElement;
        }

        /// <summary>
        /// Attach the given visual to the main window.
        /// </summary>
        public void Show(Visual visual)
        {
            RootElement = visual;
            ShowRoot();
        }

        /// <summary>
        /// Load the given file into return string.
        /// </summary>
        /// <param name="filename">name of the file, relative to the BaseDirectory</param>
        public string LoadStringFromFile(string filename)
        {
            string fullname = BaseDirectory + filename;
            System.IO.Stream stream = File.OpenRead(fullname);

            StreamReader sr = new StreamReader(stream);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// Repeat the current test, instead of moving to the next one.
        /// </summary>
        public void RepeatTest()
        {
            RepeatTest(0);
        }

        /// <summary>
        /// Repeat the current test after the given delay (in milliseconds), instead of moving to the next one.
        /// </summary>
        public void RepeatTest(int delay)
        {
            if (delay > 0)
                Pause(delay);

            ResumeAt(_currentTest);
        }

        /// <summary>
        /// After the current test completes, pause for the given time (in
        /// milliseconds) before starting the next test.
        /// </summary>
        /// <param name="pause">Number of milliseconds to pause.</param>
        public void Pause(int pause)
        {
            _pause = pause;
        }

        /// <summary>
        /// This will suspend the tests until the Resume method is called.
        /// </summary>
        public void Suspend()
        {
            if (!_suspend && _testsScheduled > 0)
            {
                Assert(_testsScheduled == 1);
                Console.WriteLine(
                    "Warning: Calling DrtBase.Suspend() when the next test is already scheduled to run. " +
                    "If it doesn't call Resume(), testing will stall.");
            }
            _suspend = true;
        }

        /// <summary>
        /// Starting executing tests again after a Suspend
        /// </summary>
        public void Resume()
        {
            Resume(_testPriority);
        }

        /// <summary>
        /// Starting executing tests again after a Suspend
        /// </summary>
        public void Resume(DispatcherPriority priority)
        {
            _suspend = false;
            Assert(_testsScheduled == 0 || _testsScheduled == 1, "If more than one test is scheduled at a time, low-priority operations may not run when expected.");
            if (_testsScheduled == 0)
            {
                var operation = _dispatcher.BeginInvoke(
                    priority,
                    new DispatcherOperationCallback(RunNextTestOperation),
                    null
                    );

                _testsScheduled = 1;
            }
        }

        /// <summary>
        /// After the current test completes, resume at the given test.
        /// </summary>
        /// <param name="test"></param>
        public void ResumeAt(DrtTest test)
        {
            _resumeStack.Push(test);
        }

        /// <summary>
        /// Run pending Dispatcher requests until the given priority.
        /// </summary>
        public static bool WaitForPriority(DispatcherPriority priority)
        {
            const int defaultTimeout = 30000;

            // Schedule the ExitFrame operation to end the nested pump after the timeout trigger happens
            TimeoutFrame frame = new TimeoutFrame();

            FrameTimer timeoutTimer = new FrameTimer(frame, defaultTimeout, new DispatcherOperationCallback(TimeoutFrameOperation), DispatcherPriority.Send);
            timeoutTimer.Start();

            // exit after a priortity has been processed
            DispatcherOperation opExit = Dispatcher.CurrentDispatcher.BeginInvoke(priority, new DispatcherOperationCallback(ExitFrameOperation), frame);

            // Pump the dispatcher
            Dispatcher.PushFrame(frame);

            // abort the operations that did not get processed
            if (opExit.Status != DispatcherOperationStatus.Completed)
                opExit.Abort();
            if (!timeoutTimer.IsCompleted)
                timeoutTimer.Stop();

            return !frame.TimedOut;
        }

        /// <summary>
        /// Run pending Dispatcher requests until a render happens.
        /// </summary>
        public static bool WaitForRender()
        {
            const int defaultTimeout = 30000;

            // Schedule the ExitFrame operation to end the nested pump after the timeout trigger happens
            TimeoutFrame frame = new TimeoutFrame();

            FrameTimer timeoutTimer = new FrameTimer(frame, defaultTimeout, new DispatcherOperationCallback(TimeoutFrameOperation), DispatcherPriority.Send);
            timeoutTimer.Start();

            EventHandler render = (s, e) => { ExitFrameOperation(frame); };
            CompositionTarget.Rendering += render;

            // Pump the dispatcher
            Dispatcher.PushFrame(frame);

            CompositionTarget.Rendering -= render;

            if (!timeoutTimer.IsCompleted)
                timeoutTimer.Stop();

            return !frame.TimedOut && WaitForPriority(DispatcherPriority.Background);
        }

        /// <summary>
        /// Start tests at the given frequency (in milliseconds).
        /// </summary>
        public int TestFrequency
        {
            set
            {
                _frequency = value;
                if (_frequency == 0)
                {
                    _timer.Stop();
                    _timer.Tick -= new EventHandler(RunNextTestTimerTask);
                    _timer = null;
                }
                if (_timer != null)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(_frequency);
                }
            }
            get
            {
                return _frequency;
            }
        }

        /// <summary>
        /// Assert that condition is true.
        /// </summary>
        /// <param name="cond">condition to test</param>
        public void Assert(bool cond)
        {
            this.Assert(cond, String.Empty, null, null);
        }

        /// <summary>
        /// Assert that condition is true.
        /// </summary>
        /// <param name="cond">condition to test</param>
        /// <param name="message">message to display if assert fails</param>
        /// <param name="arg">args for format tags in message</param>
        public void Assert(bool cond, string message, params object[] arg)
        {
            _cAssertsInSuite++;

            if (!cond)
            {
                string testID;
                if (0 <= _testIndex && _testIndex < _test.Length && _currentTest != null)
                {
                    if (_currentTest == _test[_testIndex])
                    {
                        testID = String.Format("Suite: {0}  Test: {1}\n",
                                        _currentSuite.Name,
                                        _currentTest.Method.Name);
                    }
                    else
                    {
                        testID = String.Format("Suite: {0}  Test: {1}/{2}\n",
                                        _currentSuite.Name,
                                        _test[_testIndex].Method.Name,
                                        _currentTest.Method.Name);
                    }
                }
                else
                {
                    testID = String.Format("Suite: {0}  Test: {1}\n",
                                    _currentSuite != null ? _currentSuite.Name : "(null)",
                                    _testIndex);
                }


                string s;

                // Don't call String.Format if we didn't get any args.  Otherwise,
                // curly brackets in the string will cause String.Format to throw.
                if (arg.Length == 0)
                {
                    s = message;
                }
                else
                {
                    s = String.Format(message, arg);
                }

                _retcode = 1;
                Console.WriteLine(" ASSERT failed [{0}]", testID);
                Console.WriteLine("   {0}", s);
                ReportDrtInfo();
                _totalFailures++;

                // Write any delayed output for debugging purposes
                WriteDelayedOutput();
                Xunit.Assert.True(cond, s);

                if (_loudAsserts)
                {
                    // throwing up an assert -- unblock just in case
                    BlockInput = false;
                    OnResetInputState();

                    // close the log file
                    _delayedConsoleOutput.CloseFile();

                    string s1 = String.Format("{0} [{1}]", s, testID);

                    if (!_assertsAsExceptions)
                    {
                        System.Diagnostics.Debug.Assert(cond, s1);
                    }
                    else
                    {
                        throw new Exception("Assert failed: " + s1);
                    }
                }
            }
        }

        /// <summary>
        /// Assert that objects are equal. The phrase "Expected: x  Got: y" is automatically added to the message.
        /// </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        /// <param name="message">message to display if assert fails</param>
        /// <param name="arg">args for format tags in message</param>
        public void AssertEqual(object expected, object actual, string message, params object[] arg)
        {
            if (!Object.Equals(expected, actual))
            {
                if (expected == null) expected = "NULL";
                if (actual == null) actual = "NULL";
                message += String.Format(" Expected: {0}  Got: {1}", expected, actual);
                this.Assert(false, message, arg);
            }
        }

        /// <summary>
        /// Assert that objects are equal. The phrase "Expected: x  Got: y" is automatically added to the message.
        /// </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        /// <param name="message">message to display if assert fails</param>
        /// <param name="arg">args for format tags in message</param>
        public void AssertEqual(int expected, int actual, string message, params object[] arg)
        {
            if (expected != actual)
            {
                message += String.Format(" Expected: {0}  Got: {1}", expected, actual);
                this.Assert(false, message, arg);
            }
        }

        /// <summary>
        /// Assert that doubles are equal (up to relative error of epsion).
        /// The phrase "Expected: x  Got: y" is automatically added to the message.
        /// </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        /// <param name="epsilon">tolerance for relative error</param>
        /// <param name="message">message to display if assert fails</param>
        /// <param name="arg">args for format tags in message</param>
        public void AssertAreClose(double expected, double actual, double epsilon, string message, params object[] arg)
        {
            double tolerance = epsilon * Math.Max(Math.Abs(expected), Math.Abs(actual));
            if (Math.Abs(expected - actual) > tolerance)
            {
                message += String.Format(" Expected: {0}  Got: {1}", expected, actual);
                this.Assert(false, message, arg);
            }
        }

        /// <summary>
        /// Print details about an exception to the console.
        /// </summary>
        /// <param name="o">exception object</param>
        public void PrintException(object o)
        {
            PrintException(o, null);
        }

        /// <summary>
        /// Print details about an exception to the console.
        /// </summary>
        /// <param name="o">exception object</param>
        /// <param name="message">message to display before printing the exception</param>
        /// <param name="arg">args for format tags in message</param>
        public void PrintException(object o, string message, params object[] args)
        {
            Exception e = o as Exception;

            Console.WriteLine();

            if (message != null)
                Console.WriteLine(message, args);

            if (e != null)
            {
                Console.WriteLine("{0}: {1}\n{2}",
                            e.GetType().FullName, e.Message, e.StackTrace);

                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    Console.WriteLine("Inner Exception was {0}: {1}\n{2}",
                                e.GetType().FullName, e.Message, e.StackTrace);
                }
            }
            else
            {
                Console.WriteLine("{0}", o);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Fails with the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public void Fail(string message, params object[] arg)
        {
            Assert(false, message, arg);
        }

        #endregion

        #region Uncommon functions

        /// <summary>
        /// Name of the current thread.
        /// </summary>
        public static string ThreadName
        {
            get
            {
                string s = Thread.CurrentThread.Name;
                if (s == null)
                    s = "<No Name>";

                return s;
            }
        }

        /// <summary>
        /// Write buffered output to the StringBuilder and the console if not just buffering
        /// </summary>
        public void LogOutput(string output)
        {
            Console.WriteLine(output);

            // Console.WriteLine will also write to the log file if _delayOutput is true.
            // If output isn't delayed then we need to write to the file as well
            if (!_delayOutput)
            {
                _delayedConsoleOutput.WriteLine(output);
            }
        }


        /// <summary>
        /// Write all output buffered to the StringBuilder (as a result of _delayOutput)
        /// and then clear the buffer.
        /// </summary>
        public void WriteDelayedOutput()
        {
            if (DelayOutput)
            {
                _standardConsoleOutput.WriteLine(_delayedConsoleOutput.Buffer);

                _delayedConsoleOutput.ClearBuffer();
            }
        }

        /// <summary>
        /// Apply the given function to each element of the given list, letting
        /// Avalon settle in between.  (LISP users will recognize MAPCAR)
        /// </summary>
        /// <param name="function"></param>
        /// <param name="list"></param>
        public void ApplyFunctionToList(DispatcherOperationCallback function, IEnumerable list)
        {
            new MapCar(function, list, this);
        }

        /// <summary>
        /// Call each function in the given list, letting Avalon settle in between.
        /// </summary>
        /// <param name="list"></param>
        public void CallFunctions(IEnumerable list)
        {
            new MapList(list, this);
        }

        /// <summary>
        ///     Wait for the UCE thread to complete rendering.  This is the MIL-blessed way
        ///     to wait for rendering to complete.
        /// </summary>
        public void WaitForCompleteRender()
        {
            Type compositionTargetType = typeof(System.Windows.Media.CompositionTarget );
            Assembly mcasm = Assembly.GetAssembly(compositionTargetType);
            Type mcType = mcasm.GetType("System.Windows.Media.MediaContext");
            object mediaContext = mcType.InvokeMember("From", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { _dispatcher });

            mcType.InvokeMember("CompleteRender", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mediaContext, new object[]{});
        }

        #endregion

        #region Tracing

        /// <summary>
        /// Enable trace output.
        /// </summary>
        public void EnableTracing()
        {
            ++ _traceDepth;
        }

        /// <summary>
        /// Disable trace output.
        /// </summary>
        public void DisableTracing()
        {
            if (_traceDepth > 0)
                -- _traceDepth;
        }

        /// <summary>
        /// Write trace message to console (if tracing is enabled).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public void Trace(string message, params object[] arg)
        {
            if (_traceDepth > 0)
            {
                DateTime time = DateTime.Now;
                string timeString = time.Hour + ":" + time.Minute + ":" + time.Second + "." + time.Millisecond;
                Console.WriteLine("[trace][" + timeString + "] " + message, arg);
            }
        }

        /// <summary>
        /// Add the given assembly to the list of "used" assemblies.
        /// (These are reported after a failure).
        /// </summary>
        /// <param name="a"></param>
        public void UseAssembly(Assembly a)
        {
            _assemblies[a] = 0;
        }

        /// <summary>
        /// Add the assembly that declares the given type to the list of "used" assemblies.
        /// (These are reported after a failure).
        /// </summary>
        /// <param name="t"></param>
        public void UseType(Type t)
        {
            if (t == null)
                throw new ArgumentNullException("t");

            UseAssembly(t.Assembly);
        }

        #endregion

        #region Tree-walking helpers

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <param name="node">starting node for the search</param>
        /// <param name="includeNode">if false, do not test the node itself</param>
        /// <returns></returns>
        public DependencyObject FindVisualByPropertyValue(DependencyProperty dp, object value, DependencyObject node, bool includeNode)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            // see if the node itself has the right value
            if (includeNode)
            {
                object nodeValue = node.GetValue(dp);
                if (Object.Equals(value, nodeValue))
                    return node;
            }

            // if not, recursively look at the visual children
            int count = VisualTreeHelper.GetChildrenCount(node);
            for(int i = 0; i < count; i++)
            {
                DependencyObject result = FindVisualByPropertyValue(dp, value, VisualTreeHelper.GetChild(node, i), true);
                if (result != null)
                    return result;
            }

            // not found
            return null;
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <param name="node">starting node for the search</param>
        /// <returns></returns>
        public DependencyObject FindVisualByPropertyValue(DependencyProperty dp, object value, DependencyObject node)
        {
            return FindVisualByPropertyValue(dp, value, node, true);
        }

        /// <summary>
        /// Do a depth-first search of the visual tree (starting at the root)
        /// looking for a node with a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <returns></returns>
        /// <example>
        /// For example, to find the element with ID "foo", call
        ///  DRT.FindVisualByPropertyValue(IDProperty, "foo");
        /// </example>
        public DependencyObject FindVisualByPropertyValue(DependencyProperty dp, object value)
        {
            return FindVisualByPropertyValue(dp, value, RootElement);
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given type.
        /// </summary>
        /// <param name="type">type of desired node</param>
        /// <param name="node">starting node for the search</param>
        /// <param name="includeNode">if false, do not test the node itself</param>
        public DependencyObject FindVisualByType(Type type, DependencyObject node, bool includeNode)
        {
            return FindVisualByType(type, node, includeNode, 0);
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given type.
        /// </summary>
        /// <param name="type">type of desired node</param>
        /// <param name="node">starting node for the search</param>
        /// <param name="includeNode">if false, do not test the node itself</param>
        /// <param name="skip">Skip this number of matches.</param>
        public DependencyObject FindVisualByType(Type type, DependencyObject node, bool includeNode, int skip)
        {
            return FindVisualByType(type, node, includeNode, ref skip);
        }

        private DependencyObject FindVisualByType(Type type, DependencyObject node, bool includeNode, ref int skip)
        {
            // see if the node itself has the right type
            if (includeNode)
            {
                if (type == node.GetType())
                {
                    if (skip == 0)
                    {
                        return node;
                    }
                    else
                    {
                        skip--;
                    }
                }
            }

            // if not, recursively look at the visual children
            int count = VisualTreeHelper.GetChildrenCount(node);
            for(int i = 0; i < count; i++)
              {
                DependencyObject result = FindVisualByType(type,  VisualTreeHelper.GetChild(node,i), true, ref skip);
                if (result != null)
                    return result;
            }

            // not found
            return null;
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given type.
        /// </summary>
        /// <param name="type">type of desired node</param>
        /// <param name="node">starting node for the search</param>
        public DependencyObject FindVisualByType(Type type, DependencyObject node)
        {
            return FindVisualByType(type, node, true);
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given ID.
        /// </summary>
        /// <param name="id">id of desired node</param>
        /// <param name="node">starting node for the search</param>
        public DependencyObject FindVisualByID(string id, DependencyObject node)
        {
            return FindVisualByPropertyValue(FrameworkElement.NameProperty, id, node);
        }

        /// <summary>
        /// Do a depth-first search of the visual tree looking for a node with
        /// a given ID.
        /// </summary>
        /// <param name="id">id of desired node</param>
        public DependencyObject FindVisualByID(string id)
        {
            return FindVisualByID(id, RootElement);
        }

        /// <summary>
        /// Find the (automation) AutomationElement with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AutomationElement FindLogicalElementByID(string id)
        {
            PropertyCondition conds = new PropertyCondition(AutomationElement.NameProperty, id);
            return LogicalRoot.FindFirst(TreeScope.Element | TreeScope.Descendants, conds);
        }

        /// <summary>
        /// Walk up the visual tree looking for a node with a given type.
        /// </summary>
        /// <param name="type">type of desired node</param>
        /// <param name="node">starting node for the search</param>
        /// <param name="includeNode">if false, do not test the node itself</param>
        /// <returns></returns>
        public DependencyObject FindAncestorByType(Type type, DependencyObject node, bool includeNode)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            // see if the node itself has the right type
            if (includeNode)
            {
                if (type == node.GetType())
                    return node;
            }

            // if not, look at the ancestors
            for (node = VisualTreeHelper.GetParent(node); node != null; node = VisualTreeHelper.GetParent(node))
            {
                if (type == node.GetType())
                    return node;
            }

            // not found
            return null;
        }

        /// <summary>
        /// Search the visual and logical trees looking for a node with
        /// a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <param name="node">starting node for the search</param>
        /// <param name="includeNode">if false, do not test the node itself</param>
        /// <returns></returns>
        public DependencyObject FindElementByPropertyValue(DependencyProperty dp, object value, DependencyObject node, bool includeNode)
        {
            if (node == null)
                return null;

            // see if the node itself has the right value
            if (includeNode)
            {
                object nodeValue = node.GetValue(dp);
                if (Object.Equals(value, nodeValue))
                    return node;
            }

            DependencyObject result;
            DependencyObject child;

            // if not, recursively look at the logical children
            foreach (object currentChild in LogicalTreeHelper.GetChildren(node))
            {
                child = currentChild as DependencyObject;
                result = FindElementByPropertyValue(dp, value, child, true);
                if (result != null)
                    return result;
            }

            // then the visual children
            Visual vNode = node as Visual;
            if (vNode != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(vNode);
                for(int i = 0; i < count; i++)
                {
                    child = VisualTreeHelper.GetChild(vNode, i) as DependencyObject;
                    result = FindElementByPropertyValue(dp, value, child, true);
                    if (result != null)
                        return result;
                }
            }

            // not found
            return null;
        }

        /// <summary>
        /// Search the visual and logical trees looking for a node with
        /// a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <param name="node">starting node for the search</param>
        /// <returns></returns>
        public DependencyObject FindElementByPropertyValue(DependencyProperty dp, object value, DependencyObject node)
        {
            return FindElementByPropertyValue(dp, value, node, true);
        }

        /// <summary>
        /// Search the visual and logical trees (starting at the root)
        /// looking for a node with a given property value.
        /// </summary>
        /// <param name="dp">property to query</param>
        /// <param name="value">desired value</param>
        /// <returns></returns>
        /// <example>
        /// For example, to find the element with ID "foo", call
        ///  DRT.FindVisualByPropertyValue(IDProperty, "foo");
        /// </example>
        public DependencyObject FindElementByPropertyValue(DependencyProperty dp, object value)
        {
            return FindElementByPropertyValue(dp, value, RootElement);
        }


        /// <summary>
        /// Search the visual and logical trees looking for a node with
        /// a given ID.
        /// </summary>
        /// <param name="id">id of desired node</param>
        /// <param name="node">starting node for the search</param>
        public DependencyObject FindElementByID(string id, DependencyObject node)
        {
            return FindElementByPropertyValue(FrameworkElement.NameProperty, id, node);
        }

        /// <summary>
        /// Search the visual and logical trees looking for a node with
        /// a given ID.
        /// </summary>
        /// <param name="id">id of desired node</param>
        public DependencyObject FindElementByID(string id)
        {
            return FindElementByID(id, RootElement);
        }

        #endregion

        /// <summary>
        /// The Dispatcher.
        /// </summary>
        public Dispatcher Dispatcher { get { return _dispatcher; } }

        /// <summary>
        /// Get the main window.  First access creates the window.  Only works if called before DRT.Run().
        /// </summary>
        /// <value></value>
        public HwndSource MainWindow
        {
            get
            {
                if (_source == null)
                {
                    HwndSourceParameters param = new HwndSourceParameters(WindowTitle);
                    param.SetPosition((int)WindowPosition.X, (int)WindowPosition.Y);

                    if (WindowSize != Size.Empty)
                    {
                        param.SetSize((int)WindowSize.Width, (int)WindowSize.Height);
                    }

                    _source = new HwndSource(param);
                    _logicalRoot = null;

                    InitializeMainWindow();
                }

                return _source;
            }
            set
            {
                _source = value;
                _logicalRoot = null;
            }
        }

        protected void InitializeMainWindow()
        {
            _source.AddHook(new HwndSourceHook(ApplicationFilterMessage));

            if (_topMost)
            {
                SetTopMost(_source.Handle, true);
            }
        }

        protected void UninitializeMainWindow()
        {
            _source.RemoveHook(new HwndSourceHook(ApplicationFilterMessage));
        }

        /// <summary>
        /// Detects if only the client sku of .NET is installed
        /// Today this means the clr and WPF only
        /// </summary>
        public static bool IsClientSKUOnly
        {
            get
            {
                DetectInstalledSKUs();
                return _isClientSKUOnly;
            }
        }

        protected static DrtBase DRT
        {
            get { return _drt; }
        }

        public void Dispose()
        {
            if (_source != null)
            {
                _source.Dispose();
                _source = null;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= new EventHandler(RunNextTestTimerTask);
                _timer = null;
            }

            _logicalRoot = null;
        }

        public WarningLevel WarningMismatchedForeground
        {
             set {_warningMismatchedForeground = value;}
        }

        public static CultureInfo InvariantEnglishUS
        {
            get
            {
                return invariantEnglishUS;
            }
        }

        #region Private Implementation

        //------------------------------------------------------
        //
        //  Private implementation
        //
        //------------------------------------------------------

        int IndexFromString(string s)
        {
            int index;

            if (!Int32.TryParse(s, out index))
            {
                index = -1;
            }

            return index;
        }

        bool SuiteMatches(DrtTestSuite suite, object o)
        {
            if (o is int && _suiteIndex == (int)o)
                return true;

            if (o is string && suite.Name.ToLower() == ((string)o).ToLower())
                return true;

            return false;
        }

        bool IsSelected(DrtTestSuite suite)
        {
            int k;

            if (suite == null)
                return false;

            // if there are selected suites, see if our suite is one of them
            if (_selectedSuites.Count > 0)
            {
                for (k = _selectedSuites.Count-1;  k >= 0;  --k)
                {
                    if (SuiteMatches(suite, _selectedSuites[k]))
                        return true;
                }

                return false;
            }

            // otherwise see if our suite is on the disabled list
            for (k = _disabledSuites.Count-1;  k >= 0;  --k)
            {
                if (SuiteMatches(suite, _disabledSuites[k]))
                    return false;
            }

            return true;
        }

        object DoSetup(object arg)
        {
            OnStartingUp();
            if (!_dontStartSuites)
            {
                StartSuites();
            }
            return null;
        }

        protected void StartSuites()
        {
            _dontStartSuites = false;
            _dispatcher.BeginInvoke(
                _testPriority,
                new DispatcherOperationCallback(RunNextSuite),
                null
                );
        }

        object RunNextSuite(object arg)
        {
            for (; _suiteIndex < _suites.Length; ++_suiteIndex)
            {
                DrtTestSuite suite = _suites[_suiteIndex];

                if (IsSelected(suite))
                {
                    if (_currentSuite != null)
                        _currentSuite.ReleaseResources();

                    OnResetInputState();

                    // reset "test priority" to default.  Suite can override this in PrepareTests
                    _testPriority = DispatcherPriority.ApplicationIdle;

                    ConsoleOut.WriteLine(" >Suite: {0}", suite.Name);
                    suite.DRT = this;
                    _currentSuite = suite;
                    _suiteInfoReported = false;

                    _test = suite.PrepareTests();

                    _testIndex = 0;
                    _cAssertsInSuite = 0;

                    ScheduleNextTest();

                    _suites[_suiteIndex] = null;     // release suite's memory
                    ++_suiteIndex;
                    return null;
                }
            }

            // all suites are done - close the app
            if (!_keepAlive)
            {
                if (_currentSuite != null)
                {
                    _currentSuite.ReleaseResources();
                    _currentSuite = null;
                }

                _dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new DispatcherOperationCallback(Quit),
                    null
                    );
            }
            else
            {
                // Unblock input so the user can interact with the DRT.
                Win32BlockInput(false);
            }

            OnShuttingDown();

            return null;
        }

        void ScheduleNextTest()
        {
            Assert(_testsScheduled == 0, "If more than one test is scheduled at a time, low-priority background operations may not run when expected.");
            _testsScheduled++;
            if (_frequency == 0)
            {
                // treat frequency 0 as max speed; use tasks at "test priority" for that
                _dispatcher.BeginInvoke(
                    _testPriority,
                    new DispatcherOperationCallback(RunNextTestOperation),
                    null
                    );
            }
            else
            {
                // use timer for frequencies other than 0
                if (_timer == null)
                {
                    _timer = new DispatcherTimer(DispatcherPriority.Normal);
                    _timer.Tick += new EventHandler(RunNextTestTimerTask);
                    _timer.Interval = TimeSpan.FromMilliseconds(_frequency);
                    _timer.Start();
                }
            }
        }

        object RunNextTestOperation(object arg)
        {
            RunNextTest();
            return null;
        }

        void RunNextTestTimerTask(object sender, EventArgs e)
        {
            RunNextTest();
        }

        void RunNextTest()
        {
            Assert(_testsScheduled == 1, "If more than one test is scheduled at a time, low-priority background operations may not run when expected.");

            // honor a request to pause before starting the next test
            if (_pause > 0)
            {
                // queue a timer item and schedule it to "test" priority in _pause milliseconds
                Trace("Pause {0} test {1} - {2}", _pause, _testIndex, _currentTest.Method.Name);
                DispatcherTimer callbackTimer = new DispatcherTimer(_testPriority);
                callbackTimer.Interval = TimeSpan.FromMilliseconds(_pause);
                callbackTimer.Tick +=
                    delegate(object sender, EventArgs e) {
                        ((DispatcherTimer)sender).Stop();
                        RunNextTest();
                    };
                callbackTimer.Start();

                _pause = 0;
                return;
            }

            _testsScheduled--;

            bool aborting = true;
            string action;

            if (_resumeStack.Count > 0)
            {
                _currentTest = (DrtTest)_resumeStack.Pop();
                action = "  continuing";
            }
            else
            {
                _currentTest = (_testIndex < _test.Length) ? _test[_testIndex] : null;
                action = "Starting";
            }

            if (_currentTest != null)
            {
                Trace("{0} test {1} - {2}", action, _testIndex, _currentTest.Method.Name);

                // run the current test. Do this in a try..finally so that we're
                // sure to move to the next text even if there's an exception
                // that's caught by the ContextExceptionHandler.
                try
                {
                    _currentTest();
                    aborting = false;
                }
                finally
                {
                    if (_resumeStack.Count == 0)
                    {
                        Trace("{0} test {1}", aborting ? "Aborting" : "Ending", _testIndex);
                        _testIndex++;
                    }

                    Xunit.Assert.False(aborting);

                    // Schedule the next test.
                    // Special situation: The current test may have called Suspend() and then
                    // Resume(), which means the next test is already scheduled.
                    if (!_suspend && _testsScheduled == 0)
                        ScheduleNextTest();
                }
            }
            else
            {
                // Suites can set a ContextExceptionHandler, but they must remove it
                // before they're done.  Otherwise an exception in a subsequent suite
                // will get handled (and probably ignored) by the stale handler,
                // and the DRT might even hang without reporting the problem.
                bool dispatcherHasUnhandledExceptionHandler = (bool)_piHasUnhandledExceptionHandler.GetValue(_dispatcher, null);
                if (dispatcherHasUnhandledExceptionHandler)
                {
                    // can't throw an exception here - it'll get swallowed by the
                    // DispatcherExceptionHandler.  So just report to the console.
                    Console.WriteLine("Error: Suite '{0}' set a DispatcherExceptionHandler but failed to remove it.", _currentSuite.Name);
                    if (_retcode == 0)
                        _retcode = 1;

                   _suiteIndex = _suites.Length;     // skip the remaining suites
                }

                // the suite is finished
                RunNextSuite(null);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            ReportException(args.ExceptionObject);
        }

        private void ReportException(object o)
        {
            Exception e = o as Exception;

            if (_retcode == 0)
                _retcode = 1;

            PrintException(o, "Unhandled exception on thread '{0}'", ThreadName);

            ReportDrtInfo();

            Xunit.Assert.True(_retcode == 0);
            Environment.Exit(_retcode);
        }

        private void ReportDrtInfo()
        {
            if (!_drtInfoReported)
            {
                Console.WriteLine();
                Console.WriteLine(">>> DRT Information:");

                Console.WriteLine("Assemblies used (make sure these come from the expected location):");
                IDictionaryEnumerator ie = _assemblies.GetEnumerator();
                while (ie.MoveNext())
                {
                    Assembly a = ie.Key as Assembly;
                    Console.WriteLine("  {0}\n    from {1}", a.FullName, a.Location);
                }

                Console.WriteLine("<<< End of DRT Information");
                _drtInfoReported = true;
            }

            if (!_suiteInfoReported && _currentSuite != null)
            {
                Console.WriteLine("  >> Suite Information:");
                Console.WriteLine("  currently running suite '{0}'", _currentSuite.Name);
                Console.WriteLine("  << End of suite Information");
                _suiteInfoReported = true;
            }
        }

        private static IntPtr ApplicationFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Quit the application if the source window is closed.
            if (msg == WM_CLOSE)
            {
                _drt.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new DispatcherOperationCallback(Quit),
                    null
                    );

                handled = true;
            }

            if (msg == WM_ACTIVATE && wParam == IntPtr.Zero )
            {
                _drt.OnDeactivated();
            }

            return IntPtr.Zero ;
        }


        //
        // This method turns on managed tracing that goes nowhere, for the purpose of testing
        // the trace code itself.  I.e. all the trace code runs, but it doesn't log to a file
        // anywhere.
        //

        protected void EnableNoopTracing()
        {
            PresentationTraceSources.Refresh();

            PropertyInfo[] propertyInfos = typeof(PresentationTraceSources).GetProperties( BindingFlags.Static | BindingFlags.Public );
            foreach( PropertyInfo propertyInfo in propertyInfos )
            {
                if( propertyInfo.PropertyType != typeof(TraceSource) )
                {
                    continue;
                }

                TraceSource traceSource = propertyInfo.GetValue(null, null) as TraceSource;
                traceSource.Switch.Level = SourceLevels.All;
            }
        }

        /// <summary>
        /// Event which occurs when the main window is deactivated.
        /// </summary>
        protected virtual void OnDeactivated()
        {
        }

        private static void DetectInstalledSKUs()
        {
            if (!_isSKUDetected)
            {
                MultiTargetUtilities.ExecuteIfNetDesktop(() =>
                {
                    bool isFullSKU = 
                        IsRegDWordValuePresent(Registry.LocalMachine, @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5\Setup", "InstallSuccess", 1)
                        || IsRegDWordValuePresent(Registry.LocalMachine, @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup", "InstallSuccess", 1);

                    bool isClientSKU = !isFullSKU;

                    _isClientSKUOnly = !isFullSKU && isClientSKU;
                    _isSKUDetected = true;
                }, 
                () =>
                {
                    _isClientSKUOnly = false;
                    _isSKUDetected = true;
                });
            }
        }

        private static bool IsRegDWordValuePresent(RegistryKey key, string path, string valueName, int value)
        {
            using(RegistryKey subKey = key.OpenSubKey(path))
            {
                if(subKey != null)
                {
                    object valueAsObject = subKey.GetValue(valueName);
                    if(valueAsObject != null && valueAsObject is int)
                    {
                        int valueAsInt = (int)valueAsObject;
                        if(valueAsInt == value && subKey.GetValueKind(valueName) == RegistryValueKind.DWord)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static object Quit(object arg)
        {
            _drt.Dispatcher.InvokeShutdown();
            _drt.Dispose();
            return null;
        }

        private void PrintUsage()
        {
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine("Usage:  {0} [options]", args[0]);
            PrintOptions();

            // Pretty print the Suite Names into columns
            Console.WriteLine("Suite names: ");

            int longestName = 0;
            foreach (DrtTestSuite suite in _suites)
            {
                if (suite != null && suite.Name != null)
                    longestName = Math.Max(longestName, suite.Name.Length);
            }

            string indent = "    ";
            int namesPerRow = ((80 - indent.Length) / (longestName + 1));
            string columnPaddingString = "{0,-" + longestName + "} ";

            int currentColumn = 0;
            foreach (DrtTestSuite suite in _suites)
            {
                if (suite == null)
                    continue;

                if (currentColumn == 0)
                {
                    Console.Write(indent);
                }

                Console.Write(columnPaddingString, suite.Name);
                if ((++currentColumn % namesPerRow) == 0)
                {
                    Console.WriteLine();
                    currentColumn = 0;
                }
            }

            if (currentColumn != 0)
            {
                Console.WriteLine();
            }

            foreach (DrtTestSuite suite in _suites)
            {
                if (suite != null)
                    suite.PrintOptions();
            }
        }

        //
        // Private nested classes
        //

        /// <summary>
        /// apply a function to each element of a list, allowing Avalon to settle in between
        /// </summary>
        class MapCar
        {
            public MapCar(DispatcherOperationCallback function, IEnumerable list, DrtBase drt)
            {
                _function = function;
                _ie = list.GetEnumerator();
                _drt = drt;
                _map = new DrtTest(Map);
                Test();
            }

            void Test()
            {
                if (_ie.MoveNext())
                    _drt.ResumeAt(_map);
            }

            void Map()
            {
                // call Test before applying the function - this resumes the mapcar
                // after the function is done, even if the function itself calls ResumeAt.
                object x = _ie.Current;
                Test();
                _drt.Trace("  MapCar calls {0}", _function.Method.Name);
                _function(x);
            }

            DispatcherOperationCallback _function;
            IEnumerator _ie;
            DrtBase _drt;
            DrtTest _map;
        }

        /// <summary>
        /// call each function on a list, allowing Avalon to settle in between
        /// </summary>
        class MapList
        {
            public MapList(IEnumerable list, DrtBase drt)
            {
                _ie = list.GetEnumerator();
                _drt = drt;
                _map = new DrtTest(Map);
                Test();
            }

            void Test()
            {
                if (_ie.MoveNext())
                    _drt.ResumeAt(_map);
            }

            void Map()
            {
                // call Test before applying the function - this resumes the mapcar
                // after the function is done, even if the function itself calls ResumeAt.
                DrtTest function = (DrtTest)_ie.Current;
                Test();
                _drt.Trace("  MapList calls {0}", function.Method.Name);
                function();
            }

            IEnumerator _ie;
            DrtBase _drt;
            DrtTest _map;
        }


        /// <summary>
        ///     Check if SendInput will stop the screensaver, and if it's not then make it so.
        /// </summary>
        private void MakeSendInputResetScreenSaver()
        {
            bool blockSendInputResets = false;

            // BLOCKSENDINPUTRESETS should be false -- "don't block sendinput resets".
            if (SystemParametersInfo(SPI_GETBLOCKSENDINPUTRESETS, 0, ref blockSendInputResets, 0))
            {
                if (blockSendInputResets)
                {
                    blockSendInputResets = !blockSendInputResets;
                    Console.WriteLine("Warning: SendInput does not reset the screensaver; setting SPI_SETBLOCKSENDINPUTRESETS to {0}", blockSendInputResets);
                    if (!SystemParametersInfo(SPI_SETBLOCKSENDINPUTRESETS, blockSendInputResets ? 1:0, ref blockSendInputResets, 1 /* update ini file */))
                    {
                        Console.WriteLine("Could not set SPI_SETBLOCKSENDINPUTRESETS to {0}", blockSendInputResets);
                    }
                }
            }
        }

        /// <summary>
        /// Does some preliminary checks to make sure it's okay to send input.
        /// Checks to see if the screen saver is running or any power-save mode is active.
        /// Checks if the DRT window is hung.  Checks if the DRT window is foreground.
        /// </summary>
        public void PrepareToSendInput()
        {
            BlockInput = true;

            IntPtr hwnd = GetForegroundWindow();
            if (hwnd != _source.Handle)
            {
                string error = String.Format("Warning: Foreground window {0:X} ({1},{2},{3}) did not match DRT window {4:X}. SentInput can not be processed correctly.", hwnd, GetWindowTitle(hwnd), GetWindowClassName(hwnd), GetWindowProcessImageFileName(hwnd), _source.Handle);
                if (_warningMismatchedForeground >= WarningLevel.Warning)
                {
                    Console.WriteLine(error);
                }
                if (_warningMismatchedForeground == WarningLevel.Error)
                {
                    throw new Exception(error);
                }
            }

            if (IsHungAppWindow(_source.Handle))
            {
                string error = String.Format("Main window hung and has been ghosted. This is a bad time to be sending input!");
                Console.WriteLine(error);
                Console.Write("Allowing dispatcher to pump input messages to un-ghost...");
                _dispatcher.Invoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object arg) { return null; }, null);
                Console.WriteLine("done.");
            }

            if (_warningScreenSaving >= WarningLevel.Warning)
            {
                CheckScreenSaver(SPI_GETSCREENSAVETIMEOUT, "Screen Saver");
                CheckScreenSaver(SPI_GETLOWPOWERTIMEOUT, "Low Power Saving");
                CheckScreenSaver(SPI_GETPOWEROFFTIMEOUT, "Power Off Saving");
            }
        }

        private void CheckScreenSaver(int spiGetTimeout, string mode)
        {
            int timeout = 0;

            if (SystemParametersInfo(spiGetTimeout, 0, ref timeout, 0))
            {
                if (timeout > 0)
                {
                    // timeout is given to us in seconds, not milliseconds
                    timeout *= 1000;

                    int idleTime = GetIdleTime();
                    //Console.WriteLine("{0} -- idleTime is {1} and timeout is {2}", mode, idleTime, timeout);
                    if (idleTime >= 0)
                    {
                        if (timeout > idleTime)
                        {
                            //Console.WriteLine("Warning: {0} has non-zero timeout. Timeout will expire in {1} s", mode, (timeout - idleTime) / 1000.0);
                        }
                        else
                        {
                            string error = String.Format(_warningScreenSaving.ToString() + ": {0} timeout expired {1} s ago", mode, (idleTime - timeout) / 1000.0);
                            if (_warningScreenSaving >= WarningLevel.Warning)
                            {
                                Console.WriteLine(error);
                            }
                            if (_warningScreenSaving == WarningLevel.Error)
                            {
                                throw new Exception(error);
                            }
                        }
                    }
                }
            }
        }

#endregion

#region Win32 interop helpers

        public static bool SetTopMost(IntPtr WindowHandle, bool topmost)
        {
            return SetWindowPos(WindowHandle, topmost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_DRAWFRAME | SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        /// <summary>
        ///     Returns diff b/c GetTickCount and GetLastInputInfo in ms
        /// </summary>
        private int GetIdleTime()
        {
            int currTime = GetTickCount();
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = Marshal.SizeOf(lii);
            if (GetLastInputInfo(ref lii))
            {
                return (currTime - lii.dwTime);
            }
            else
            {
                return -1;
            }
        }

        internal struct LASTINPUTINFO
        {
            public int cbSize;
            public int dwTime;
        }

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern int GetTickCount();

        [DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO lii);


        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x = 0;
            public int y = 0;

            public POINT()
            {
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref bool value, int ignore);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, [Out] StringBuilder lpString, int nMaxCount);

        public static string GetWindowTitle(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(500);
            int hr = GetWindowText(new HandleRef(null, hwnd), sb, 500);
            if (hr < 0)
                throw new Win32Exception();
            return sb.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        public static string GetWindowClassName(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(500);
            int hr = GetClassName(new HandleRef(null, hwnd), sb, 500);
            if (hr < 0)
                throw new Win32Exception();
            return sb.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetWindowThreadProcessId(HandleRef hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool CloseHandle(HandleRef hObject);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        public static extern int GetProcessImageFileName(HandleRef hProcess, StringBuilder lpString, int nMaxCount);

        public static string GetWindowProcessImageFileName(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(500);
            uint processId;
            GetWindowThreadProcessId(new HandleRef(null, hwnd), out processId);
            if (processId != 0)
            {
                IntPtr hProcess = OpenProcess(0x400, // PROCESS_QUERY_INFORMATION
                                              false,
                                              processId);
                if (hProcess != IntPtr.Zero)
                {
                    int hr = GetProcessImageFileName(new HandleRef(null, hProcess), sb, 500);
                    if (hr < 0)
                        throw new Win32Exception();
                    if (!CloseHandle(new HandleRef(null, hProcess)))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            return sb.ToString();
        }


        int SPI_GETBLOCKSENDINPUTRESETS = 0x1026;
        int SPI_SETBLOCKSENDINPUTRESETS = 0x1027;
        int SPI_GETSCREENSAVETIMEOUT = 0x000E;
        int SPI_GETLOWPOWERTIMEOUT = 0x004F;
        int SPI_GETPOWEROFFTIMEOUT = 0x0050;

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "BlockInput")]
        private static extern int Win32BlockInput(int fBlockIt);

        /// <summary>
        /// Prevent user from sending any input to the system.  (Press CTRL-ALT-DEL to unblock if you need to).
        /// </summary>
        private static void Win32BlockInput(bool blockIt)
        {
            int hr = Win32BlockInput(blockIt ? 1 : 0);
            if (hr < 0)
                throw new Win32Exception();
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        private static IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_DRAWFRAME = 0x0020;
        public const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern bool IsHungAppWindow(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        internal extern static void DisableProcessWindowsGhosting();

#endregion

#region Text/StringWriter helpers

        private class StringAndFileWriter : StringWriter
        {
            public StringAndFileWriter(string drtName, ITestOutputHelper output) : base(new StringBuilder())
            {
                _buffer = GetStringBuilder();
                _output = output;
                if (string.IsNullOrEmpty(drtName))
                {
                    throw new ArgumentException("DrtName must be defined");
                }


                _drtName = drtName;
            }

            public bool BufferOutput
            {
                get
                {
                    return _bufferOutput;
                }
                set
                {
                    lock (_instanceLock)
                    {
                        _bufferOutput = value;
                    }
                }
            }

            public string Buffer
            {
                get
                {
                    lock (_instanceLock)
                    {
                        return _buffer.ToString();
                    }
                }
            }

            public void ClearBuffer()
            {
                lock (_instanceLock)
                {
                    _buffer.Remove(0, _buffer.Length);
                }
            }

            private bool EnsureFileOpen()
            {
                if (!_isClosed)
                {
                    const int retries = 10;
                    for (int i = 0; i < retries && _logFile == null; i++)
                    {
                        var fileName = _drtName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".log";
                        try
                        {
                            _logFile = System.IO.File.CreateText(fileName);
                        }
                        catch(IOException)
                        {
                        }
                    }
                }

                return _logFile != null;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                CloseFile();
            }

            public override void Write(char value)
            {
                lock (_instanceLock)
                {
                    if (_bufferOutput)
                    {
                        base.Write(value);
                    }

                    if (EnsureFileOpen())
                    {
                        _logFile.Write(value);
                        _logFile.Flush();
                    }

                    try
                    {
                        _output.WriteLine($"{value}");
                    }
                    catch
                    {
                    }
                }
            }

            public override void Write(char[] buffer, int index, int count)
            {
                lock (_instanceLock)
                {
                    if (_bufferOutput)
                    {
                        base.Write(buffer, index, count);
                    }

                    if (EnsureFileOpen())
                    {
                        _logFile.Write(buffer, index, count);
                        _logFile.Flush();
                    }

                    var sb = new StringBuilder();
                    sb.Append(buffer, index, count);
                    try
                    {
                        _output.WriteLine(sb.ToString());
                    }
                    catch { }
                }
            }

            public TextWriter File
            {
                get
                {
                    lock (_instanceLock)
                    {
                        EnsureFileOpen();
                        return TextWriter.Synchronized(_logFile);
                    }
                }
            }

            public override void Write(string value)
            {
                lock (_instanceLock)
                {
                    if (_bufferOutput)
                    {
                        base.Write(value);
                    }

                    if (EnsureFileOpen())
                    {
                        _logFile.Write(value);
                        _logFile.Flush();
                    }

                    try
                    {
                        _output.WriteLine(value);
                    }
                    catch { }
                }
            }

            public void CloseFile()
            {
                lock(_instanceLock)
                {
                    if (_logFile != null)
                    {
                        _logFile.Close();
                        _logFile = null;
                        _isClosed = true;
                    }
                }
            }


            private bool _bufferOutput = true;
            private string _drtName;
            private StreamWriter _logFile;
            private StringBuilder _buffer;
            private bool _isClosed = false;
            private object _instanceLock = new object();

            private ITestOutputHelper _output;
        }

        private class DualWriter : StringWriter
        {
            public DualWriter(TextWriter writer1, TextWriter writer2)
            {
                _writer1 = writer1;
                _writer2 = writer2;
            }

            public override void Write(char value)
            {
                _writer1.Write(value);
                _writer2.Write(value);
            }

            public override void Write(char[] buffer, int index, int count)
            {
                _writer1.Write(buffer, index, count);
                _writer2.Write(buffer, index, count);

                StringBuilder sb = new StringBuilder();
                sb.Append(buffer, index, count);
            }

            public override void Write(string value)
            {
                _writer1.Write(value);
                _writer2.Write(value);
            }

            public override void WriteLine(string value)
            {
                _writer1.WriteLine(value);
                _writer2.WriteLine(value);
            }

            TextWriter _writer1;
            TextWriter _writer2;
        }

#endregion

#region WaitForPriority support

        private static object ExitFrameOperation(object obj)
        {
            DispatcherFrame frame = obj as DispatcherFrame;
            frame.Continue = false;
            return null;
        }

        private static object TimeoutFrameOperation(object obj)
        {
            TimeoutFrame frame = obj as TimeoutFrame;
            frame.Continue = false;
            frame.TimedOut = true;
            return null;
        }

        private class FrameTimer : DispatcherTimer
        {
            DispatcherFrame frame;
            DispatcherOperationCallback callback;
            bool isCompleted = false;

            public FrameTimer(DispatcherFrame frame, int milliseconds, DispatcherOperationCallback callback, DispatcherPriority priority)
                : base(priority)
            {
                this.frame = frame;
                this.callback = callback;
                Interval = TimeSpan.FromMilliseconds(milliseconds);
                Tick += new EventHandler(OnTick);
            }

            public DispatcherFrame Frame
            {
                get { return frame; }
            }

            public bool IsCompleted
            {
                get { return isCompleted; }
            }

            void OnTick(object sender, EventArgs args)
            {
                isCompleted = true;
                Stop();
                callback(frame);
            }
        }

        private class TimeoutFrame : DispatcherFrame
        {
            bool timedout = false;

            public bool TimedOut
            {
                get { return timedout; }
                set { timedout = value; }
            }
        }

#endregion

#region Private Fields
        //
        //  Private fields
        //

        private Dispatcher _dispatcher;
        private ITestOutputHelper _output;
       

        private HwndSource _source = null;
        private Visual _rootElement;
        private bool _topMost = false;
        private AutomationElement _logicalRoot;
        private static DrtBase _drt;

        private static bool _isSKUDetected = false;
        private static bool _isClientSKUOnly = false;

        private const int WM_CLOSE = 0x0010;
        private const int WM_ACTIVATE = 0x0006;
        private static bool _drtStarted;
        private PropertyInfo _piHasUnhandledExceptionHandler;

        private string _windowTitle = "DRT";
        private Size _windowSize = new Size(800, 600);
        private Point _windowPosition = new Point(50, 50);
        private string _baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private int _retcode;
        private int _traceDepth;
        private bool _keepAlive;
        private bool _dontStartSuites;
        private bool _blockInput;
        private bool _verbose;
        private bool _catchExceptions = false;
        private bool _reportInfo;

        private DrtTestSuite[] _suites = new DrtTestSuite[0];
        private DrtTest[] _test = new DrtTest[0];
        private Stack _resumeStack = new Stack();
        private bool _drtInfoReported;
        private bool _suiteInfoReported;

        private DispatcherTimer _timer;
        private int _frequency = 0;
        private DispatcherPriority _testPriority=DispatcherPriority.ApplicationIdle;

        private int _suiteIndex, _testIndex=0;
        /// <summary>
        /// This counter, normally expected to take on the values of only 0 and 1, is used to make
        /// sure no more than one test is in the Dispatcher's queue. If this happens, any other
        /// Dispatcher operations with priority lower than the test priority may not run when expected.
        /// </summary>
        private int _testsScheduled;
        private DrtTestSuite _currentSuite;
        private DrtTest _currentTest;
        private int _pause;
        private bool _suspend;

        private ArrayList _selectedSuites = new ArrayList();
        private ArrayList _disabledSuites = new ArrayList();
        private Hashtable _assemblies = new Hashtable();

        private int _cAssertsInSuite;
        private bool _loudAsserts = true;
        private int _totalFailures = 0;

        private TextWriter _standardConsoleOutput = Console.Out;
        private StringAndFileWriter _delayedConsoleOutput;

        private bool _delayOutput = true;
        private bool _assertsAsExceptions = true;

        private WarningLevel _warningMismatchedForeground = WarningLevel.Warning;
        private WarningLevel _warningScreenSaving = WarningLevel.Warning;

        private string _DrtName;

        private static CultureInfo invariantEnglishUS = CultureInfo.ReadOnly(new CultureInfo("en-us", false));

#endregion
    }

    public enum WarningLevel { Ignore, Warning, Error, TentativelySucceed };

    public struct KeyStatePair
    {
        public KeyStatePair(Key key, bool press) { Key = key; Press = press; }
        public Key Key;
        public bool Press;
    }

#region DrtTestSuite class

    // A "suite" of tests, typically operating on a single tree.
    public class DrtTestSuite
    {
        protected DrtTestSuite(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Return the list of individual tests (i.e. callback methods).
        /// Derived classes should override this.
        /// A null entry in the list serves as a list terminator (as does the
        /// end of the list).
        /// </summary>
        /// <returns></returns>
        public virtual DrtTest[] PrepareTests()
        {
            return new DrtTest[0];
        }

        /// <summary>
        /// Called when the suite is completed.  Suite should release memory
        /// and other resources
        /// </summary>
        public virtual void ReleaseResources() {}

        /// <summary>
        /// The name of the suite.
        /// </summary>
        public string Name { get { return _name; } }
        string _name;

        /// <summary>
        /// The DrtBase that is running this suite.
        /// </summary>
        public DrtBase DRT { get { return _drt; } set { _drt = value; } }
        DrtBase _drt;

        /// <summary>
        /// Override this to handle per-suite command line arguments
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="option"></param>
        /// <param name="args"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public virtual bool HandleCommandLineArgument(string arg, bool option, string[] args, ref int k)
        {
            return false;
        }

        /// <summary>
        /// Override this to print help for per-suite options.
        /// </summary>
        public virtual void PrintOptions()
        {
        }

        /// <summary>
        /// Convert an object to String, using the Culture/Language of the given DependencyObject
        /// </summary>
        public string ToStringHelper(object o, System.Windows.DependencyObject d)
        {
            string result;

            try
            {
                System.Windows.Markup.XmlLanguage xmlLang = (d == null) ? null :
                    (System.Windows.Markup.XmlLanguage)d.GetValue(System.Windows.FrameworkElement.LanguageProperty);

                System.Globalization.CultureInfo culture = (xmlLang == null) ? null :
                    xmlLang.GetSpecificCulture();

                result = (o is System.IConvertible) ? (String)System.Convert.ChangeType(o, typeof(String), culture)
                                                    : o.ToString();
            }
            catch (Exception)
            {
                result = "Conversion Error";
            }

            return result;
        }

    }

    public class DrtSuite<T> : DrtTestSuite where T : DrtBase
    {
        protected DrtSuite(string name)
            : base(name)
        {
        }

        public T Drt
        {
            get { return (T)DRT; }
        }
    }

#endregion

    // the delegate to use for an individual "test"
    public delegate void DrtTest();
}
