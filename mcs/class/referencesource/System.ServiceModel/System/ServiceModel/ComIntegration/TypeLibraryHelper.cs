//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    sealed class TypeLibraryHelper
    {

        internal static Assembly GenerateAssemblyFromNativeTypeLibrary(Guid iid, Guid typeLibraryID, ITypeLib typeLibrary)
        {
            TypeLibraryHelper helper = GetHelperInstance();
            try
            {
                return helper.GenerateAssemblyFromNativeTypeLibInternal(iid, typeLibraryID, typeLibrary);

            }
            finally
            {
                ReleaseHelperInstance();
            }


        }
        private static object instanceLock = new object();
        private static TypeLibraryHelper instance;
        private static int instanceCount = 0;
        private static TypeLibraryHelper GetHelperInstance()
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    TypeLibraryHelper tlhTemp = new TypeLibraryHelper();
                    Thread.MemoryBarrier();
                    instance = tlhTemp;
                }
            }

            Interlocked.Increment(ref instanceCount);
            return instance;
        }

        private static void ReleaseHelperInstance()
        {
            if (0 == Interlocked.Decrement(ref instanceCount))
                instance = null;
        }


        internal class ConversionEventHandler : ITypeLibImporterNotifySink
        {

            Guid iid;
            Guid typeLibraryID;
            public ConversionEventHandler(Guid iid, Guid typeLibraryID)
            {
                this.iid = iid;
                this.typeLibraryID = typeLibraryID;
            }

            void ITypeLibImporterNotifySink.ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
            {
                ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTLBImportConverterEvent,
                            SR.TraceCodeComIntegrationTLBImportConverterEvent, iid, typeLibraryID, eventKind, eventCode, eventMsg);
            }

            Assembly ITypeLibImporterNotifySink.ResolveRef(object typeLib)
            {

                ITypeLib tlb = typeLib as ITypeLib;
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    tlb.GetLibAttr(out ptr);
                    System.Runtime.InteropServices.ComTypes.TYPELIBATTR attr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR)Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                    return TypeLibraryHelper.GenerateAssemblyFromNativeTypeLibrary(iid, attr.guid, typeLib as ITypeLib);

                }
                finally
                {
                    if ((ptr != IntPtr.Zero) && (tlb != null))
                        tlb.ReleaseTLibAttr(ptr);
                }
            }

        }

        TypeLibConverter TypelibraryConverter = new TypeLibConverter();
        Dictionary<Guid, Assembly> TypelibraryAssembly = new Dictionary<Guid, Assembly>();


        private string GetRandomName()
        {
            Guid guid = Guid.NewGuid();
            String strGuid = guid.ToString();
            return strGuid.Replace('-', '_');
        }
        private Assembly GenerateAssemblyFromNativeTypeLibInternal(Guid iid, Guid typeLibraryID, ITypeLib typeLibrary)
        {
            Assembly asm = null;

            try
            {
                lock (this)
                {
                    TypelibraryAssembly.TryGetValue(typeLibraryID, out asm);
                    if (asm == null)
                    {
                        string assemblyName = "";
                        string notused1 = "";
                        string notused2 = "";
                        int notused3;
                        string namespaceName;
                        typeLibrary.GetDocumentation(-1, out namespaceName, out notused1, out notused3, out notused2);
                        if (String.IsNullOrEmpty(namespaceName))
                        {
                            throw Fx.AssertAndThrowFatal("Assembly cannot be null");
                        }
                        assemblyName = String.Concat(namespaceName, GetRandomName(), ".dll");
                        asm = TypelibraryConverter.ConvertTypeLibToAssembly(typeLibrary, assemblyName, TypeLibImporterFlags.SerializableValueClasses, new ConversionEventHandler(iid, typeLibraryID), null, null, namespaceName, null);
                        TypelibraryAssembly[typeLibraryID] = asm;
                    }
                }

            }
            catch (ReflectionTypeLoadException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FailedToConvertTypelibraryToAssembly)));

            }

            if (asm == null)
            {
                throw Fx.AssertAndThrowFatal("Assembly cannot be null");
            }
            return asm;
        }
    }
}
