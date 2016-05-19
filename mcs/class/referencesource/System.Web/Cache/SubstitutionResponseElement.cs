using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;
using System.Web.Compilation;

namespace System.Web.Caching {
    [Serializable]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Unrestricted)]
    public class SubstitutionResponseElement: ResponseElement {
        [NonSerialized]
        private  HttpResponseSubstitutionCallback _callback;
        private  string                           _targetTypeName;
        private  string                           _methodName;

        public   HttpResponseSubstitutionCallback Callback   { get { return _callback; } }

        private SubstitutionResponseElement() { } // hide default constructor

        public SubstitutionResponseElement(HttpResponseSubstitutionCallback callback) {
            if (callback == null)
                throw new ArgumentNullException("callback");

            _callback = callback;
        }

        [OnSerializing()]
        private void OnSerializingMethod(StreamingContext context) {
            // create a string representation of the callback
            _targetTypeName = System.Web.UI.Util.GetAssemblyQualifiedTypeName(_callback.Method.ReflectedType);
            _methodName = _callback.Method.Name;
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context) {
            // re-create each ValidationCallbackInfo from its string representation
            Type target = BuildManager.GetType(_targetTypeName, true /*throwOnFail*/, false /*ignoreCase*/);
            _callback = (HttpResponseSubstitutionCallback) Delegate.CreateDelegate(typeof(HttpResponseSubstitutionCallback), target, _methodName);
        }
    }
}
