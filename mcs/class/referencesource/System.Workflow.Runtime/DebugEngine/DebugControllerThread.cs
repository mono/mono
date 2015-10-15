#region Using directives

using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;
#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    internal sealed class DebugControllerThread
    {
        #region Data members

        private Thread controllerThread = null;
        private int threadId = 0;
        private ManualResetEvent threadInitializedEvent = null;
        private volatile bool runThread = false;
        private static readonly string ExpressionEvaluationFrameTypeName = "ExpressionEvaluationFrameTypeName";
        #endregion

        #region Methods

        public DebugControllerThread()
        {
            this.threadInitializedEvent = new ManualResetEvent(false);
            this.threadInitializedEvent.Reset();

            this.controllerThread = new Thread(ControllerThreadFunction);
            this.controllerThread.IsBackground = true;
            this.controllerThread.Priority = ThreadPriority.Lowest;
            this.controllerThread.Name = "__dct__";
        }

        public void RunThread(IInstanceTable instanceTable)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugControllerThread.DebugControllerThread():"));
            if (this.controllerThread == null)
                return;

            this.runThread = true;
            this.controllerThread.Start(instanceTable);
            this.threadInitializedEvent.WaitOne();
        }

        public void StopThread()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugControllerThread.StopThread():"));

            try
            {
                if (this.controllerThread != null)
                {
                    this.runThread = false;
                    Thread.Sleep(10);

                    // On x64 we put the debug controller thread to Sleep(Timeout.Infinite) in 
                    // ExpressionEvaluationFunction. This thread needs to be started before it
                    // a Join can execute.
                    if (this.controllerThread.IsAlive && IntPtr.Size == 8)
                    {
                        while (this.controllerThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                        {
                            this.controllerThread.Start();
                            this.controllerThread.Join();
                        }
                    }
                    else
                        this.controllerThread.Join();
                }
            }
            catch (ThreadStateException)
            {
                // Ignore the ThreadStateException which will be thrown when StopThread() is called during 
                // AppDomain unload.
            }
            finally
            {
                this.controllerThread = null;
            }

            this.controllerThread = null;
            this.threadId = 0;
            this.threadInitializedEvent = null;
        }

        private void ControllerThreadFunction(object instanceTable)
        {
            try
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugControllerThread.ControllerThreadFunction():"));

                IExpressionEvaluationFrame expressionEvaluationFrame = null;

                try
                {
                    RegistryKey debugEngineSubKey = Registry.LocalMachine.OpenSubKey(RegistryKeys.DebuggerSubKey);
                    if (debugEngineSubKey != null)
                    {
                        string evaluationFrameTypeName = debugEngineSubKey.GetValue(ExpressionEvaluationFrameTypeName, String.Empty) as string;
                        if (!String.IsNullOrEmpty(evaluationFrameTypeName) && Type.GetType(evaluationFrameTypeName) != null)
                            expressionEvaluationFrame = Activator.CreateInstance(Type.GetType(evaluationFrameTypeName)) as IExpressionEvaluationFrame;
                    }
                }
                catch { }

                if (expressionEvaluationFrame == null)
                {
                    Type eeFrameType = null;

                    const string eeFrameTypeNameFormat = "Microsoft.Workflow.DebugEngine.ExpressionEvaluationFrame, Microsoft.Workflow.ExpressionEvaluation, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

                    // Try versions 12.0.0.0, 11.0.0.0, 10.0.0.0
                    for (int version = 12; eeFrameType == null && version >= 10; --version)
                    {
                        try
                        {
                            eeFrameType = Type.GetType(string.Format(CultureInfo.InvariantCulture, eeFrameTypeNameFormat, version));
                        }
                        catch (TypeLoadException)
                        {
                            // Fall back to next-lower version
                        }
                    }

                    if (eeFrameType != null)
                    {
                        expressionEvaluationFrame = Activator.CreateInstance(eeFrameType) as IExpressionEvaluationFrame;
                    }
                }

                Debug.Assert(expressionEvaluationFrame != null, "Failed to create Expression Evaluation Frame.");

                if (expressionEvaluationFrame != null)
                    expressionEvaluationFrame.CreateEvaluationFrame((IInstanceTable)instanceTable, (DebugEngineCallback)Delegate.CreateDelegate(typeof(DebugEngineCallback), this, "ExpressionEvaluationFunction"));
            }
            catch
            {
                // Don't throw exceptions to the Runtime, it would terminate the debugee.
            }
        }

        // This thread spins forever. It is used by the Debug Engine to perform expression evaluation as a "carrier
        // wave". It is created only when the debugger is attached and terminates itself when the debugger isn't
        // attached anymore.
        public void ExpressionEvaluationFunction()
        {
            this.threadId = NativeMethods.GetCurrentThreadId();
            this.threadInitializedEvent.Set();


            using (new DebuggerThreadMarker())
            {
                // If an exception occurs somehow, continue to spin.
                while (this.runThread)
                {
                    try
                    {
                        // Expression eval on x64 does not work (

                        if (IntPtr.Size == 8)
                        {
                            Thread.Sleep(Timeout.Infinite);
                        }
                        else
                            // Spin within the try catch.
                            while (this.runThread);
                    }
                    catch (ThreadAbortException)
                    {
                        Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugControllerThread.ExpressionEvaluationFunction(): ThreadAbortException"));

                        // Explicitly do not call ResetAbort().
                        throw;
                    }
                    catch
                    {
                        Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugControllerThread.ExpressionEvaluationFunction(): other exception"));
                    }
                }
            }
        }

        #endregion

        #region Properties

        public int ThreadId
        {
            get
            {
                return this.threadId;
            }
        }

        public int ManagedThreadId
        {
            get
            {
                return this.controllerThread.ManagedThreadId;
            }
        }

        #endregion
    }
}
