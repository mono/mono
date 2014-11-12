using System;
using System.Security.Permissions;

namespace System.Runtime.InteropServices {


    [System.Security.SecuritySafeCritical]
    public class ComAwareEventInfo : System.Reflection.EventInfo {

        #region private fields
        private System.Reflection.EventInfo _innerEventInfo;
        #endregion

        #region ctor
        public ComAwareEventInfo(Type type, string eventName) {
            _innerEventInfo = type.GetEvent(eventName);
        }
        #endregion

        #region protected overrides
        public override void AddEventHandler(object target, Delegate handler) {
            if (Marshal.IsComObject(target)) {
                // retrieve sourceIid and dispid
                Guid sourceIid;
                int dispid;
                GetDataForComInvocation(_innerEventInfo, out sourceIid, out dispid);

                // now validate the caller can call into native and redirect to ComEventHelpers.Combine
                SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                perm.Demand();
                System.Runtime.InteropServices.ComEventsHelper.Combine(target, sourceIid, dispid, handler);
            } else {
                // we are dealing with a managed object - just add the delegate through reflection
                _innerEventInfo.AddEventHandler(target, handler);
            }
        }

        public override void RemoveEventHandler(object target, Delegate handler) {
            if (Marshal.IsComObject(target)) {
                // retrieve sourceIid and dispid
                Guid sourceIid;
                int dispid;
                GetDataForComInvocation(_innerEventInfo, out sourceIid, out dispid);

                // now validate the caller can call into native and redirect to ComEventHelpers.Combine
                SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                perm.Demand();
                System.Runtime.InteropServices.ComEventsHelper.Remove(target, sourceIid, dispid, handler);
            } else {
                // we are dealing with a managed object - just add the delegate through relection
                _innerEventInfo.RemoveEventHandler(target, handler);
            }
        }
        #endregion

        #region public overrides
        public override System.Reflection.EventAttributes Attributes {
            get { return _innerEventInfo.Attributes; }
        }

        public override System.Reflection.MethodInfo GetAddMethod(bool nonPublic) {
            return _innerEventInfo.GetAddMethod(nonPublic);
        }

        public override System.Reflection.MethodInfo GetRaiseMethod(bool nonPublic) {
            return _innerEventInfo.GetRaiseMethod(nonPublic);
        }

        public override System.Reflection.MethodInfo GetRemoveMethod(bool nonPublic) {
            return _innerEventInfo.GetRemoveMethod(nonPublic);
        }

        public override Type DeclaringType {
            get { return _innerEventInfo.DeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return _innerEventInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return _innerEventInfo.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return _innerEventInfo.IsDefined(attributeType, inherit);
        }

        public override string Name {
            get { return _innerEventInfo.Name; }
        }

        public override Type ReflectedType {
            get { return _innerEventInfo.ReflectedType; }
        }
        #endregion

        #region private methods

        private static void GetDataForComInvocation(System.Reflection.EventInfo eventInfo, out Guid sourceIid, out int dispid) {
            object[] comEventInterfaces = eventInfo.DeclaringType.GetCustomAttributes(typeof(ComEventInterfaceAttribute), false);

            if (comEventInterfaces == null || comEventInterfaces.Length == 0) {
                // 
                throw new InvalidOperationException("event invocation for COM objects requires interface to be attributed with ComSourceInterfaceGuidAttribute");
            }

            if (comEventInterfaces.Length > 1) {
                // 
                throw new System.Reflection.AmbiguousMatchException("more than one ComSourceInterfaceGuidAttribute found");
            }

            Type sourceItf = ((ComEventInterfaceAttribute)comEventInterfaces[0]).SourceInterface;
            Guid guid = sourceItf.GUID;

            System.Reflection.MethodInfo methodInfo = sourceItf.GetMethod(eventInfo.Name);
            Attribute dispIdAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(DispIdAttribute));
            if (dispIdAttribute == null) {
                // 
                throw new InvalidOperationException("event invocation for COM objects requires event to be attributed with DispIdAttribute");
            }

            sourceIid = guid;
            dispid = ((DispIdAttribute)dispIdAttribute).Value;
        }
        #endregion
    }
}
