//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using SafeHGlobalHandle = System.IdentityModel.SafeHGlobalHandle;


    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0000010c-0000-0000-C000-000000000046")]
    internal interface IPersist
    {
        void GetClassID( /* [out] */ out Guid pClassID);
    };

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00000109-0000-0000-C000-000000000046")]
    internal interface IPersistStream : IPersist
    {
        new void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load([In] IStream pStm);
        void Save([In] IStream pStm, [In,
        MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        void GetSizeMax(out long pcbSize);
    };

    internal class PersistHelper
    {
        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal static byte[] ConvertHGlobalToByteArray(SafeHGlobalHandle hGlobal)
        {
            // this has to be Int32, even on 64 bit machines since Marshal.Copy takes a 32 bit integer
            Int32 sizeOfByteArray = (SafeNativeMethods.GlobalSize(hGlobal)).ToInt32();
            if (sizeOfByteArray > 0)
            {
                byte[] byteArray = new Byte[sizeOfByteArray];
                IntPtr pBuff = SafeNativeMethods.GlobalLock(hGlobal);
                if (IntPtr.Zero == pBuff)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new OutOfMemoryException());

                try
                {
                    Marshal.Copy(pBuff, byteArray, 0, sizeOfByteArray);
                }
                finally
                {
                    SafeNativeMethods.GlobalUnlock(hGlobal);
                }
                return byteArray;
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal static Byte[] PersistIPersistStreamToByteArray(IPersistStream persistableObject)
        {
            IStream stream = SafeNativeMethods.CreateStreamOnHGlobal(SafeHGlobalHandle.InvalidHandle, false);
            try
            {
                persistableObject.Save(stream, true);
                SafeHGlobalHandle hGlobal = SafeNativeMethods.GetHGlobalFromStream(stream);
                if (null == hGlobal || IntPtr.Zero == hGlobal.DangerousGetHandle())
                {
                    throw Fx.AssertAndThrow("HGlobal returned from  GetHGlobalFromStream is NULL");
                }

                return ConvertHGlobalToByteArray(hGlobal);
            }
            finally
            {
                Marshal.ReleaseComObject(stream);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal static void LoadIntoObjectFromByteArray(IPersistStream persistableObject, Byte[] byteStream)
        {
            SafeHGlobalHandle hGlobal = SafeHGlobalHandle.AllocHGlobal(byteStream.Length);

            IntPtr pBuff = SafeNativeMethods.GlobalLock(hGlobal);
            if (IntPtr.Zero == pBuff)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new OutOfMemoryException());
            try
            {
                Marshal.Copy(byteStream, 0, pBuff, byteStream.Length);
                IStream stream = SafeNativeMethods.CreateStreamOnHGlobal(hGlobal, false);
                try
                {
                    persistableObject.Load(stream);
                }
                finally
                {
                    Marshal.ReleaseComObject(stream);
                }
            }
            finally
            {
                SafeNativeMethods.GlobalUnlock(hGlobal);
            }
        }

        internal static object ActivateAndLoadFromByteStream(Guid clsid, byte[] byteStream)
        {
            IPersistStream persistableObject = SafeNativeMethods.CoCreateInstance(
                            clsid,
                            null,
                            CLSCTX.INPROC_SERVER,
                            typeof(IPersistStream).GUID) as IPersistStream;
            if (null != persistableObject)
            {
                LoadIntoObjectFromByteArray(persistableObject, byteStream);
                return persistableObject;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CLSIDDoesNotSupportIPersistStream, clsid.ToString("B"))));
        }
    }

    [DataContract]
    public class PersistStreamTypeWrapper : IExtensibleDataObject
    {
        [DataMember]
        internal Guid clsid;
        [DataMember]
        internal byte[] dataStream;

        public PersistStreamTypeWrapper() { }

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void SetObject<T>(T obj)
        {
            if (Marshal.IsComObject(obj))
            {
                IntPtr punk = Marshal.GetIUnknownForObject(obj);
                if (IntPtr.Zero == punk)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.UnableToRetrievepUnk)));
                }
                try
                {
                    IntPtr persistStream = IntPtr.Zero;
                    Guid iidPersistStream = typeof(IPersistStream).GUID;
                    int hr = Marshal.QueryInterface(punk, ref iidPersistStream, out persistStream);
                    if (HR.S_OK == hr)
                    {
                        try
                        {
                            if (IntPtr.Zero == persistStream)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PersistWrapperIsNull)));
                            }
                            IPersistStream persistableObject = (IPersistStream)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(persistStream);
                            try
                            {
                                this.dataStream = PersistHelper.PersistIPersistStreamToByteArray(persistableObject);
                                this.clsid = typeof(T).GUID;
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(persistableObject);
                            }
                        }
                        finally
                        {
                            Marshal.Release(persistStream);
                        }
                    }
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CLSIDDoesNotSupportIPersistStream, typeof(T).GUID.ToString("B"))));
                }
                finally
                {
                    Marshal.Release(punk);
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.NotAComObject)));

            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void GetObject<T>(ref T obj)
        {
            if (clsid == typeof(T).GUID)
            {
                IntPtr punk = Marshal.GetIUnknownForObject(obj);
                if (IntPtr.Zero == punk)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.UnableToRetrievepUnk)));
                }
                try
                {
                    IntPtr persistStream = IntPtr.Zero;
                    Guid iidPersistStream = typeof(IPersistStream).GUID;
                    int hr = Marshal.QueryInterface(punk, ref iidPersistStream, out persistStream);
                    if (HR.S_OK == hr)
                    {
                        try
                        {
                            if (IntPtr.Zero == persistStream)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PersistWrapperIsNull)));
                            }
                            IPersistStream persistableObject = (IPersistStream)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(persistStream);
                            try
                            {
                                PersistHelper.LoadIntoObjectFromByteArray(persistableObject, dataStream);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(persistableObject);
                            }
                        }
                        finally
                        {
                            Marshal.Release(persistStream);
                        }
                    }
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CLSIDDoesNotSupportIPersistStream, typeof(T).GUID.ToString("B"))));
                }
                finally
                {
                    Marshal.Release(punk);
                }
            }
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CLSIDOfTypeDoesNotMatch, typeof(T).GUID.ToString(), clsid.ToString("B"))));
        }
    }

    internal class DataContractSurrogateForPersistWrapper : IDataContractSurrogate
    {
        Guid[] allowedClasses;
        public DataContractSurrogateForPersistWrapper(Guid[] allowedClasses)
        {
            this.allowedClasses = allowedClasses;
        }

        bool IsAllowedClass(Guid clsid)
        {
            foreach (Guid classID in allowedClasses)
                if (clsid == classID)
                    return true;
            return false;
        }

        public Type GetDataContractType(Type type)
        {
            if (type.IsInterface)
                return typeof(PersistStreamTypeWrapper);
            else
                return type;
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            if (targetType == typeof(object) || (targetType.IsInterface))
            {
                IPersistStream streamableObject = obj as IPersistStream;
                if (null != streamableObject)
                {
                    PersistStreamTypeWrapper objToSerialize = new PersistStreamTypeWrapper();
                    streamableObject.GetClassID(out objToSerialize.clsid);
                    objToSerialize.dataStream = PersistHelper.PersistIPersistStreamToByteArray(streamableObject);
                    return objToSerialize;
                }
                if (targetType.IsInterface)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TargetObjectDoesNotSupportIPersistStream)));
                return obj;
            }
            return obj;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            if (targetType == typeof(object) || (targetType.IsInterface))
            {
                PersistStreamTypeWrapper streamWrapper = obj as PersistStreamTypeWrapper;
                if (null != streamWrapper)
                {
                    if (IsAllowedClass(streamWrapper.clsid))
                    {
                        return PersistHelper.ActivateAndLoadFromByteStream(streamWrapper.clsid, streamWrapper.dataStream);
                    }
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NotAllowedPersistableCLSID, streamWrapper.clsid.ToString("B"))));
                }
                if (targetType.IsInterface)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TargetTypeIsAnIntefaceButCorrespoindingTypeIsNotPersistStreamTypeWrapper)));
            }
            return obj;
        }

        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {
            customDataTypes.Add(typeof(PersistStreamTypeWrapper));
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            return null;
        }

        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            return null;
        }
    }
}
