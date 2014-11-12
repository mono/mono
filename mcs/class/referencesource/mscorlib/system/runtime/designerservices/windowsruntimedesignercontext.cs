using System;
using System.Collections.Generic;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Diagnostics.Contracts;
using StackCrawlMark = System.Threading.StackCrawlMark;

namespace System.Runtime.DesignerServices
{
#if !FEATURE_CORECLR
    // -------------------------
    // IMPORTANT!
    // The shared and designer context binders are ONLY to be used in tool
    // scenarios. There are known issues where use of these binders will
    // cause application crashes, and undefined behaviors.
    // -------------------------
    public sealed class WindowsRuntimeDesignerContext
    {
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [ResourceExposure(ResourceScope.AppDomain)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr CreateDesignerContext([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] paths, int count, bool shared);

        [SecurityCritical]
        internal static IntPtr CreateDesignerContext(IEnumerable<string> paths, [MarshalAs(UnmanagedType.Bool)] bool shared)
        {
            List<string> pathList = new List<string>(paths);
            string[] pathArray = pathList.ToArray();
            foreach (string path in pathArray)
            {
                // Each path has to be absolute path - check it
                if (path == null)
                {
                    throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_Path"));
                }
                if (System.IO.Path.IsRelative(path))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
                }
            }
            return CreateDesignerContext(pathArray, pathArray.Length, shared);
        }

        // isDesignerContext = !shared
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [ResourceExposure(ResourceScope.AppDomain)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void SetCurrentContext([MarshalAs(UnmanagedType.Bool)] bool isDesignerContext, IntPtr context);

        private static object s_lock = new object();
        private static IntPtr s_sharedContext;

        private IntPtr m_contextObject;
        private string m_name;

        // This private constructor is called either by the public constructor or by the debugger via FuncEval.
        [SecurityCritical]
        private WindowsRuntimeDesignerContext(IEnumerable<string> paths, string name, bool designModeRequired)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (paths == null)
                throw new ArgumentNullException("paths");

            // WindowsRuntimeDesignerContext is only supported in the default domain.
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                throw new NotSupportedException();

            // WindowsRuntimeDesignerContext is only supported when running in AppX.
            if (!AppDomain.IsAppXModel())
                throw new NotSupportedException();

            // WindowsRuntimeDesignerContext is only supported when running in Designer Mode on in the debugger when called via funceval.
            // This is true when called via the public constructor,
            // but likely false when called directly by the debugger via FuncEval.
            if (designModeRequired && !AppDomain.IsAppXDesignMode())
                throw new NotSupportedException();

            m_name = name;

            lock (s_lock)
            {
                if (s_sharedContext == IntPtr.Zero)
                    InitializeSharedContext(new string[] {});
            }

            m_contextObject = CreateDesignerContext(paths, false);
        }

        // This is the public constructor that may be used when running in a process in DesignMode.
        [SecurityCritical]
        public WindowsRuntimeDesignerContext(IEnumerable<string> paths, string name)
            : this(paths, name, true)
        {}

        [SecurityCritical]
        public static void InitializeSharedContext(IEnumerable<string> paths)
        {
            // WindowsRuntimeDesignerContext is only supported in the default domain.
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                throw new NotSupportedException();

            if (paths == null)
                throw new ArgumentNullException("paths");

            lock (s_lock)
            {
                if (s_sharedContext != IntPtr.Zero)
                    throw new NotSupportedException();

                IntPtr sharedContext = CreateDesignerContext(paths, true);
                SetCurrentContext(false, sharedContext);
                s_sharedContext = sharedContext;
            }
        }

        [SecurityCritical]
        public static void SetIterationContext(WindowsRuntimeDesignerContext context)
        {
            // WindowsRuntimeDesignerContext is only supported in the default domain.
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                throw new NotSupportedException();

            if (context == null)
                throw new ArgumentNullException("context");

            lock (s_lock)
            {
                SetCurrentContext(true, context.m_contextObject);
            }
        }

        // Locate an assembly by the long form of the assembly name. 
        // eg. "Toolbox.dll, version=1.1.10.1220, locale=en, publickey=1234567890123456789012345678901234567890"
        [SecurityCritical]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public Assembly GetAssembly(String assemblyName)
        {
            Contract.Ensures(Contract.Result<Assembly>() != null);
            Contract.Ensures(!Contract.Result<Assembly>().ReflectionOnly);

            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyName, null, ref stackMark, m_contextObject, false /*forIntrospection*/);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        [SecurityCritical]
        public Type GetType(String typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            Contract.EndContractBlock();

            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;

            return RuntimeTypeHandle.GetTypeByName(
                typeName, false /*throwOnError*/, false /*ignoreCase*/, false /*reflectionOnly*/, ref stackMark, m_contextObject, false /*loadTypeFromPartialName*/);
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }
    }
#endif
}
