//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Activities.Debugger
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Activities.Debugger.Symbol;

    // Manager for supporting debugging a state machine.
    // The general usage is to call:
    //  - DefineState() for each state
    //  - Bake() once you've defined all the states you need to enter.
    //  - EnterState() / LeaveState() for each state.
    // You can Define new states and bake them, such as if the script loads a new file.
    // Baking is expensive, so it's best to define as many states in each batch.
    [DebuggerNonUserCode]
    // This class need not serialized.
    [Fx.Tag.XamlVisible(false)]
    public sealed class StateManager : IDisposable
    {
        static readonly Guid WorkflowLanguageGuid = new Guid("1F1149BB-9732-4EB8-9ED4-FA738768919C");

        static readonly LocalsItemDescription[] debugInfoDescriptions = new LocalsItemDescription[] {
                new LocalsItemDescription("debugInfo", typeof(DebugInfo))
            };

        static Type threadWorkerControllerType = typeof(ThreadWorkerController);
        static MethodInfo islandWorkerMethodInfo = threadWorkerControllerType.GetMethod("IslandWorker", BindingFlags.Static | BindingFlags.Public);
        const string Md5Identifier = "406ea660-64cf-4c82-b6f0-42d48172a799";
        internal const string MethodWithPrimingPrefix = "_";

        List<LogicalThread> threads;

        DynamicModuleManager dynamicModuleManager;

        // Don't expose this, because that would expose the setters. Changing the properties
        // after baking types has undefined semantics and would be confusing to the user.
        Properties properties;

        bool debugStartedAtRoot;

        // Simple default constructor.
        internal StateManager()
            : this(new Properties(), true, null)
        {
        }


        // Constructor.
        // Properties must be set at creation time.
        internal StateManager(Properties properties, bool debugStartedAtRoot, DynamicModuleManager dynamicModuleManager)
        {
            this.properties = properties;
            this.debugStartedAtRoot = debugStartedAtRoot;
            this.threads = new List<LogicalThread>();
            if (dynamicModuleManager == null)
            {
                dynamicModuleManager = new DynamicModuleManager(this.properties.ModuleNamePrefix);
            }
            this.dynamicModuleManager = dynamicModuleManager;
        }

        internal Properties ManagerProperties
        {
            get { return this.properties; }
        }

        internal bool IsPriming
        {
            get;
            set;
        }

        // Whether debugging is started at the root workflow (contrast to attaching in the middle 
        // of a running workflow.
        internal bool DebugStartedAtRoot
        {
            get
            {
                return this.debugStartedAtRoot;
            }
        }

        // Declare a new state associated with the given source location.
        // States should have disjoint source locations. 
        // location is Source location associated with this state.
        // This returns a state object, which can be passed to EnterState.
        internal State DefineState(SourceLocation location)
        {
            return DefineState(location, string.Empty, null, 0);
        }

        internal State DefineState(SourceLocation location, string name)
        {
            return DefineState(location, name, null, 0);
        }

        internal State DefineState(SourceLocation location, string name, LocalsItemDescription[] earlyLocals, int numberOfEarlyLocals)
        {
            return this.dynamicModuleManager.DefineState(location, name, earlyLocals, numberOfEarlyLocals);
        }

        internal State DefineStateWithDebugInfo(SourceLocation location, string name)
        {
            return DefineState(location, name, debugInfoDescriptions, debugInfoDescriptions.Length);
        }


        // Bake all states using the default type namespace.
        // States must be baked before calling EnterState().
        internal void Bake()
        {
            Bake(this.properties.TypeNamePrefix, null);
        }


        // Bake all newly defined states. States must be baked before calling EnterState().
        // typeName is the type name that the islands are contained in. This
        // may show up on the callstack. If this is not unique, it will be appended with a unique
        // identifier.
        internal void Bake(string typeName, Dictionary<string, byte[]> checksumCache)
        {
            this.dynamicModuleManager.Bake(typeName, this.properties.TypeNamePrefix, checksumCache);
        }

        internal int CreateLogicalThread(string threadName)
        {
            int threadId = -1;

            // Reuse thread if exists
            // Start from 1 since main thread never disposed earlier.
            for (int i = 1; i < this.threads.Count; ++i)
            {
                if (this.threads[i] == null)
                {
                    this.threads[i] = new LogicalThread(i, threadName, this);
                    threadId = i;
                    break;
                }
            }

            // If can't reuse old thread.
            if (threadId < 0)
            {
                threadId = this.threads.Count;
                this.threads.Add(new LogicalThread(threadId, threadName, this));
            }

            return threadId;
        }

        // Push the state onto the virtual callstack, with no locals.
        // State is the state to push onto stack.
        //internal void EnterState(int threadIndex, State state)
        //{
        //    this.EnterState(threadIndex, state, null); 
        //}


        // Enter a state and push it onto the 'virtual callstack'.
        // If the user set a a breakpoint at the source location associated with
        // this state, this call will hit that breakpoint.
        // Call LeaveState when the interpretter is finished with this state.
        //
        // State is state to enter.
        // "locals" is local variables (both early-bound and late-bound) associated with this state.
        // Early-bound locals match by name with the set passed into DefineState.
        // Late-bound will be displayed read-only to the user in the watch window.</param>
        // 
        // EnterState can be called reentrantly. If code calls Enter(A); Enter(B); Enter(C);
        // Then on the call to Enter(C), the virtual callstack will be A-->B-->C.
        // Each call to Enter() will rebuild the virtual callstack.
        //
        internal void EnterState(int threadIndex, State state, IDictionary<string, object> locals)
        {
            this.EnterState(threadIndex, new VirtualStackFrame(state, locals));
        }

        // Enter a state and push it onto the 'virtual callstack'.
        // Stackframe describing state to enter, along with the
        // locals in that state.
        internal void EnterState(int threadIndex, VirtualStackFrame stackFrame)
        {
            Fx.Assert(threadIndex < this.threads.Count, "Index out of range for thread");
            Fx.Assert(this.threads[threadIndex] != null, "LogicalThread is null");
            this.threads[threadIndex].EnterState(stackFrame);
        }

        // Pop the state most recently pushed by EnterState.
        internal void LeaveState(int threadIndex, State state)
        {
            Fx.Assert(threadIndex < this.threads.Count, "Index out of range for thread");
            Fx.Assert(this.threads[threadIndex] != null, "LogicalThread is null");
            this.threads[threadIndex].LeaveState(state);
        }

        // Common helper to invoke an Stack frame. 
        // This handles marshaling the args. 
        // islandArguments - arbitrary argument passed ot the islands. 
        // [DebuggerHidden]
        internal void InvokeWorker(object islandArguments, VirtualStackFrame stackFrame)
        {
            State state = stackFrame.State;
            if (!state.DebuggingEnabled)
            {
                // We need to short circuit and call IslandWorker because that is what the generated code
                // would have done, if we had generated it. This causes the thread to finish up.
                ThreadWorkerController.IslandWorker((ThreadWorkerController)islandArguments);
                return;
            }

            MethodInfo methodInfo = this.dynamicModuleManager.GetIsland(state, this.IsPriming);
            IDictionary<string, object> allLocals = stackFrame.Locals;

            // Package up the raw arguments array.
            const int numberOfBaseArguments = 2;
            int numberOfEarlyLocals = state.NumberOfEarlyLocals;
            object[] arguments = new object[numberOfBaseArguments + numberOfEarlyLocals]; // +1 for IslandArguments and +1 for IsPriming
            arguments[0] = this.IsPriming;
            arguments[1] = islandArguments;
            if (numberOfEarlyLocals > 0)
            {
                int i = numberOfBaseArguments;
                foreach (LocalsItemDescription localsItemDescription in state.EarlyLocals)
                {
                    string name = localsItemDescription.Name;
                    object value;
                    if (allLocals.TryGetValue(name, out value))
                    {
                        // We could assert that val.GetType() is assignable to localsItemDescription.Type.
                        // MethodInfo invoke will check this anyways; but we could check
                        // it and give a better error.
                    }
                    else
                    {
                        // Local not supplied in the array! Use a default.
                        value = Activator.CreateInstance(localsItemDescription.Type);
                    }
                    arguments[i] = value;
                    i++;
                }
            }
            methodInfo.Invoke(null, arguments);
        }



        // Release any unmanaged resources.
        // This may not necessarily unload islands or dynamic modules that were created until the calling appdomain has exited.
        public void Dispose()
        {
            ExitThreads();
        }

        internal void ExitThreads()
        {
            foreach (LogicalThread logicalThread in this.threads)
            {
                if (logicalThread != null)
                {
                    logicalThread.Exit();
                }
            }
            this.threads.Clear();
        }


        // Release any unmanaged resources.
        // This may not necessarily unload islands or dynamic modules that were created until the calling appdomain has exited.
        public void Exit(int threadIndex)
        {
            Fx.Assert(threadIndex >= 0, "Invalid thread index");
            Fx.Assert(this.threads[threadIndex] != null, "Cannot dispose null LogicalThread");
            LogicalThread thread = this.threads[threadIndex];
            thread.Exit();

            // Null the entry on the List for future reuse.
            this.threads[threadIndex] = null;
        }

        // Property bag for Manager. These provide customization hooks.
        // All properties have valid default values.
        [DebuggerNonUserCode]
        internal class Properties
        {
            public Properties() :
                this("Locals", "Script", "States", "WorkflowDebuggerThread", true)
            {
            }

            public Properties(string defaultLocalsName, string moduleNamePrefix, string typeNamePrefix, string auxiliaryThreadName, bool breakOnStartup)
            {
                this.DefaultLocalsName = defaultLocalsName;
                this.ModuleNamePrefix = moduleNamePrefix;
                this.TypeNamePrefix = typeNamePrefix;
                this.AuxiliaryThreadName = auxiliaryThreadName;
                this.BreakOnStartup = breakOnStartup;
            }

            public string DefaultLocalsName
            {
                get;
                set;
            }

            // The name of the dynamic module (not including extension) that the states are emitted to.
            // This may show up on the callstack.
            // This is a prefix because there may be multiple modules for the islands.
            public string ModuleNamePrefix
            {
                get;
                set;
            }

            // Typename that states are created in.
            // This is a prefix because there may be multiple Types for the islands
            // (such as if islands are created lazily).
            public string TypeNamePrefix
            {
                get;
                set;
            }

            // If UseAuxiliaryThread is true, sets the friendly name of that thread as visible
            // in the debugger's window.
            public string AuxiliaryThreadName
            {
                get;
                set;
            }

            // If true, the VM issues a Debugger.Break() before entering the first state.
            // This can be useful for an F11 experience on startup to stop at the first state.
            // If this is false, then the interpreter will run until it hits a breakpoint or some
            // other stopping event.
            public bool BreakOnStartup
            {
                get;
                set;
            }
        }

        [DebuggerNonUserCode]
        class LogicalThread
        {
            int threadId;
            Stack<VirtualStackFrame> callStack;
            ThreadWorkerController controller;

            public LogicalThread(int threadId, string threadName, StateManager stateManager)
            {
                this.threadId = threadId;
                this.callStack = new Stack<VirtualStackFrame>();
                this.controller = new ThreadWorkerController();
                this.controller.Initialize(threadName + "." + threadId.ToString(CultureInfo.InvariantCulture), stateManager);
            }

            // Unwind call stack cleanly.
            void UnwindCallStack()
            {
                while (this.callStack.Count > 0)
                { // LeaveState will do the popping.
                    this.LeaveState(this.callStack.Peek().State);
                }
            }

            public void Exit()
            {
                this.UnwindCallStack();
                this.controller.Exit();
            }

            // Enter a state and push it onto the 'virtual callstack'.
            // Stackframe describing state to enter, along with the
            // locals in that state.
            public void EnterState(VirtualStackFrame stackFrame)
            {
                if (stackFrame != null && stackFrame.State != null)
                {
                    this.callStack.Push(stackFrame);
                    this.controller.EnterState(stackFrame);
                }
                else
                { // signify "Uninstrumented call"
                    this.callStack.Push(null);
                }
            }

            // Pop the state most recently pushed by EnterState.
            [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Revisit for bug 36860")]
            public void LeaveState(State state)
            {
                if (this.callStack.Count > 0)
                {
                    VirtualStackFrame stackFrame = this.callStack.Pop();
                    Fx.Assert(
                        (state == null && stackFrame == null) ||
                        (stackFrame != null && stackFrame.State == state),
                        "Unmatched LeaveState: " +
                        ((state == null) ? "null" : state.Name) +
                        " with top stack frame: " +
                        ((stackFrame == null || stackFrame.State == null) ? "null" : stackFrame.State.Name));

                    if (stackFrame != null)    // Matches with an uninstrumented Activity.
                    {
                        this.controller.LeaveState();
                    }
                }
                else
                {
                    Fx.Assert("Unmatched LeaveState: " + ((state != null) ? state.Name : "null"));
                }
            }
        }

        internal class DynamicModuleManager
        {
            // List of all state that have been created with DefineState.
            List<State> states;
            Dictionary<SourceLocation, State> stateMap = new Dictionary<SourceLocation, State>();

            // Index into states array of the last set of states baked.
            // So Bake() will build islands for each state 
            // { states[x], where indexLastBaked <= x < states.Length; }
            int indexLastBaked;

            // Mapping from State --> MethodInfo for that state.
            // This gets populated as states get baked
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic assembly.")]
            [SecurityCritical]
            Dictionary<State, MethodInfo> islands;
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic assembly.")]
            [SecurityCritical]
            Dictionary<State, MethodInfo> islandsWithPriming;
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic assembly.")]
            [SecurityCritical]
            ModuleBuilder dynamicModule;
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic assembly.")]
            [SecurityCritical]
            Dictionary<string, ISymbolDocumentWriter> sourceDocuments;

            [Fx.Tag.SecurityNote(Critical = "Accesses Critical members and calling Critical method InitDynamicModule.",
                Safe = "We are only creating empty dictionaries, not populating them. And we are validating the provided moduleNamePrefix in partial trust.")]
            [SecuritySafeCritical]
            public DynamicModuleManager(string moduleNamePrefix)
            {
                this.states = new List<State>();
                this.islands = new Dictionary<State, MethodInfo>();
                this.islandsWithPriming = new Dictionary<State, MethodInfo>();
                this.sourceDocuments = new Dictionary<string, ISymbolDocumentWriter>();

                if (!PartialTrustHelpers.AppDomainFullyTrusted)
                {
                    moduleNamePrefix = State.ValidateIdentifierString(moduleNamePrefix);
                }

                InitDynamicModule(moduleNamePrefix);
            }

            public State DefineState(SourceLocation location, string name, LocalsItemDescription[] earlyLocals, int numberOfEarlyLocals)
            {
                State state;
                lock (this)
                {
                    if (!this.stateMap.TryGetValue(location, out state))
                    {
                        lock (this)
                        {
                            state = new State(location, name, earlyLocals, numberOfEarlyLocals);
                            this.stateMap.Add(location, state);
                            this.states.Add(state);
                        }
                    }
                }
                return state;
            }

            // Bake all newly defined states. States must be baked before calling EnterState().
            // typeName is the type name that the islands are contained in. This
            // may show up on the callstack. If this is not unique, it will be appended with a unique
            // identifier.
            [Fx.Tag.SecurityNote(Critical = "Accesses Critical members and invoking Critical methods.",
                Safe = "We validating the input strings - typeName and typeNamePrefix - and the checksum values in the checksumCache.")]
            [SecuritySafeCritical]
            public void Bake(string typeName, string typeNamePrefix, Dictionary<string, byte[]> checksumCache)
            {
                // In partial trust, validate the typeName and typeNamePrefix.
                if (!PartialTrustHelpers.AppDomainFullyTrusted)
                {
                    typeName = State.ValidateIdentifierString(typeName);
                    typeNamePrefix = State.ValidateIdentifierString(typeNamePrefix);

                    if (checksumCache != null)
                    {
                        bool nullifyChecksumCache = false;
                        foreach (KeyValuePair<string, byte[]> kvpair in checksumCache)
                        {
                            // We use an MD5 hash for the checksum, so the byte array should be 16 elements long.
                            if (!SymbolHelper.ValidateChecksum(kvpair.Value))
                            {
                                nullifyChecksumCache = true;
                                Trace.WriteLine(SR.DebugSymbolChecksumValueInvalid);
                                break;
                            }
                        }

                        // If we found an invalid checksum, just don't use the cache.
                        if (nullifyChecksumCache)
                        {
                            checksumCache = null;
                        }
                    }
                }

                lock (this)
                {
                    if (this.indexLastBaked < this.states.Count)    // there are newly created states.
                    {
                        // Ensure typename is unique. Append a number if needed.
                        int suffix = 1;
                        while (this.dynamicModule.GetType(typeName) != null)
                        {
                            typeName = typeNamePrefix + "_" + suffix.ToString(CultureInfo.InvariantCulture);
                            ++suffix;
                        }

                        TypeBuilder typeBuilder = this.dynamicModule.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);

                        for (int i = indexLastBaked; i < this.states.Count; i++)
                        {
                            // Only create the island if debugging is enabled for the state.
                            if (this.states[i].DebuggingEnabled)
                            {
                                MethodBuilder methodBuilder = this.CreateIsland(typeBuilder, this.states[i], false, checksumCache);
                                Fx.Assert(methodBuilder != null, "CreateIsland call should have succeeded");

                                // Always generate method with priming, for the following scenario:
                                //  1. Start debugging a workflow inside VS, workflow debug session 1 starts (debugStartedAtRoot = true, instrumentation is done)
                                //  2. Workflow persisted, workflow debug session 1 ends
                                //  3. Workflow continued, workflow debug session 2 starts (debugStartedAtRoot = false, instrumentation is skipped because the static dynamicModuleManager is being reused and the instrumentation is done)
                                //  4. PrimeCallStack is called to rebuild the call stack
                                //  5. NullReferenceException will be thrown if MethodInfo with prime is not available
                                MethodBuilder methodBuilderWithPriming = this.CreateIsland(typeBuilder, this.states[i], true, checksumCache);
                                Fx.Assert(methodBuilderWithPriming != null, "CreateIsland call should have succeeded");

                                // Save information needed to call Type.GetMethod() later.
                                this.states[i].CacheMethodInfo(typeBuilder, methodBuilder.Name);
                            }
                        }

                        // Actual baking.
                        typeBuilder.CreateType();

                        // Calling Type.GetMethod() is slow (10,000 calls can take ~1 minute).
                        // So defer that to later.

                        this.indexLastBaked = this.states.Count;
                    }
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic module.")]
            [SecurityCritical]
            internal MethodBuilder CreateMethodBuilder(TypeBuilder typeBuilder, Type typeIslandArguments, State state, bool withPriming)
            {
                // create the method            
                string methodName = (state.Name != null ? state.Name : ("Line_" + state.Location.StartLine));

                if (withPriming)
                {
                    methodName = MethodWithPrimingPrefix + methodName;
                }

                // Parameters to the islands:
                // 1. Args
                // 2. IDict of late-bound locals.
                // 3 ... N.  list of early bound locals.
                const int numberOfBaseArguments = 2;
                IEnumerable<LocalsItemDescription> earlyLocals = state.EarlyLocals;
                int numberOfEarlyLocals = state.NumberOfEarlyLocals;

                Type[] parameterTypes = new Type[numberOfBaseArguments + numberOfEarlyLocals];
                parameterTypes[0] = typeof(bool);
                parameterTypes[1] = typeIslandArguments;

                if (numberOfEarlyLocals > 0)
                {
                    int i = numberOfBaseArguments;
                    foreach (LocalsItemDescription localsItemDescription in earlyLocals)
                    {
                        parameterTypes[i] = localsItemDescription.Type;
                        i++;
                    }
                }

                Type returnType = typeof(void);
                MethodBuilder methodbuilder = typeBuilder.DefineMethod(
                    methodName,
                    MethodAttributes.Static | MethodAttributes.Public,
                    returnType, parameterTypes);

                // Need to define parameter here, otherwise EE cannot get the correct IDebugContainerField
                // for debugInfo.
                methodbuilder.DefineParameter(1, ParameterAttributes.None, "isPriming");
                methodbuilder.DefineParameter(2, ParameterAttributes.None, "typeIslandArguments");

                // Define the parameter names
                // Note that we can hide implementation-specific arguments from VS by not defining parameter 
                // info for them.  Eg., the StepInTarget argument doesn't appear to show up in VS at all.
                if (numberOfEarlyLocals > 0)
                {
                    int i = numberOfBaseArguments + 1;
                    foreach (LocalsItemDescription localsItemDescription in earlyLocals)
                    {
                        methodbuilder.DefineParameter(i, ParameterAttributes.None, localsItemDescription.Name);
                        i++;
                    }
                }

                return methodbuilder;
            }


            [Fx.Tag.InheritThrows(From = "GetILGenerator", FromDeclaringType = typeof(MethodBuilder))]
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic module.")]
            [SecurityCritical]
            MethodBuilder CreateIsland(TypeBuilder typeBuilder, State state, bool withPrimingTest, Dictionary<string, byte[]>checksumCache)
            {
                MethodBuilder methodbuilder = this.CreateMethodBuilder(typeBuilder, threadWorkerControllerType, state, withPrimingTest);
                ILGenerator ilGenerator = methodbuilder.GetILGenerator();
                const int lineHidden = 0xFeeFee; // #line hidden directive

                // Island:
                // void MethodName(Manager m)
                // {
                //    .line
                //     nop
                //     call Worker(m)
                //     ret;
                // }
                SourceLocation stateLocation = state.Location;
                ISymbolDocumentWriter document = this.GetSourceDocument(stateLocation.FileName, stateLocation.Checksum, checksumCache);
                Label islandWorkerLabel = ilGenerator.DefineLabel();

                // Hide all the opcodes before the real source line.
                // This is needed for Island which is called during priming (Attach to Process):
                // It should skip the line directive during priming, thus it won't break at user's
                // breakpoints at the beginning during priming the callstack.
                if (withPrimingTest)
                {
                    ilGenerator.MarkSequencePoint(document, lineHidden, 1, lineHidden, 100);
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Brtrue, islandWorkerLabel);
                }

                // Emit sequence point before the IL instructions to map it to a source location.
                ilGenerator.MarkSequencePoint(document, stateLocation.StartLine, stateLocation.StartColumn, stateLocation.EndLine, stateLocation.EndColumn);
                ilGenerator.Emit(OpCodes.Nop);

                ilGenerator.MarkLabel(islandWorkerLabel);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.EmitCall(OpCodes.Call, islandWorkerMethodInfo, null);
                ilGenerator.Emit(OpCodes.Ret);

                return methodbuilder;
            }

            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "The validations of the user input are done elsewhere.")]
            [Fx.Tag.SecurityNote(Critical = "Because we Assert UnmanagedCode in order to be able to emit symbols.")]
            [SecurityCritical]
            void InitDynamicModule(string asmName)
            {
                // See http://blogs.msdn.com/Microsoft/archive/2005/02/03/366429.aspx for a simple example
                // of debuggable reflection-emit.
                Fx.Assert(dynamicModule == null, "can only be initialized once");

                // create a dynamic assembly and module 
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.Name = asmName;

                AssemblyBuilder assemblyBuilder;

                // The temporary assembly needs to be Transparent.
                ConstructorInfo transparentCtor =
                    typeof(SecurityTransparentAttribute).GetConstructor(
                        Type.EmptyTypes);
                CustomAttributeBuilder transparent = new CustomAttributeBuilder(
                    transparentCtor,
                    new Object[] { });

                assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run, null, true, new CustomAttributeBuilder[] { transparent });

                // Mark generated code as debuggable. 
                // See http://blogs.msdn.com/rmbyers/archive/2005/06/26/432922.aspx for explanation.        
                Type debuggableAttributeType = typeof(DebuggableAttribute);
                ConstructorInfo constructorInfo = debuggableAttributeType.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
                CustomAttributeBuilder builder = new CustomAttributeBuilder(constructorInfo, new object[] {
                    DebuggableAttribute.DebuggingModes.DisableOptimizations |
                    DebuggableAttribute.DebuggingModes.Default
                });
                assemblyBuilder.SetCustomAttribute(builder);

                // We need UnmanagedCode permissions because we are asking for Symbols to be emitted.
                // We are protecting the dynamicModule so that only Critical code modifies it.
                PermissionSet unmanagedCodePermissionSet = new PermissionSet(PermissionState.None);
                unmanagedCodePermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
                unmanagedCodePermissionSet.Assert();
                try
                {

                    dynamicModule = assemblyBuilder.DefineDynamicModule(asmName, true); // <-- pass 'true' to track debug info.

                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }

            }

            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic module.",
                Safe = "State validates its SecurityCritical members itself")]
            [SecuritySafeCritical]
            public MethodInfo GetIsland(State state, bool isPriming)
            {
                MethodInfo island = null;

                if (isPriming)
                {
                    lock (islandsWithPriming)
                    {
                        if (!islandsWithPriming.TryGetValue(state, out island))
                        {
                            island = state.GetMethodInfo(true);
                            islandsWithPriming[state] = island;
                        }
                    }
                }
                else
                {
                    lock (islands)
                    {
                        if (!islands.TryGetValue(state, out island))
                        {
                            island = state.GetMethodInfo(false);
                            islands[state] = island;
                        }
                    }
                }
                return island;
            }

            // This method is only called from CreateIsland, which is only called from Bake.
            // Bake does a "lock(this)" before calling CreateIsland, so access to the sourceDocuments
            // dictionary is protected by that lock. If this changes, locking will need to be added
            // to this method to protect the sourceDocuments dictionary.
            [Fx.Tag.SecurityNote(Critical = "Used in generating the dynamic module.")]
            [SecurityCritical]
            private ISymbolDocumentWriter GetSourceDocument(string fileName, byte[] checksum, Dictionary<string, byte[]> checksumCache)
            {
                ISymbolDocumentWriter documentWriter;
                string sourceDocKey = fileName + SymbolHelper.GetHexStringFromChecksum(checksum);

                if (!this.sourceDocuments.TryGetValue(sourceDocKey, out documentWriter))
                {
                    documentWriter =
                        dynamicModule.DefineDocument(
                        fileName,
                        StateManager.WorkflowLanguageGuid,
                        SymLanguageVendor.Microsoft,
                        SymDocumentType.Text);
                    this.sourceDocuments.Add(sourceDocKey, documentWriter);

                    byte[] checksumBytes;

                    if (checksumCache == null || !checksumCache.TryGetValue(fileName.ToUpperInvariant(), out checksumBytes))
                    {
                        checksumBytes = SymbolHelper.CalculateChecksum(fileName);
                    }

                    if (checksumBytes != null)
                    {
                        documentWriter.SetCheckSum(new Guid(Md5Identifier), checksumBytes);
                    }
                }
                return documentWriter;
            }

        }
    }
}
