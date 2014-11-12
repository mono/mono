using Microsoft.Win32;
using System;
using System.Runtime.Serialization;
using System.Text;
using System.Globalization;
using System.Security.Permissions;
using System.Diagnostics.Contracts;

namespace System.Security.AccessControl
{
    [Serializable]

    public sealed class PrivilegeNotHeldException : UnauthorizedAccessException, ISerializable
    {
        private readonly string _privilegeName = null;

        public PrivilegeNotHeldException()
            : base( Environment.GetResourceString( "PrivilegeNotHeld_Default" ))
        {
        }

        public PrivilegeNotHeldException( string privilege )
            : base( string.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "PrivilegeNotHeld_Named" ), privilege ))
        {
            _privilegeName = privilege;
        }

        public PrivilegeNotHeldException( string privilege, Exception inner )
            : base( string.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "PrivilegeNotHeld_Named" ), privilege ), inner )
        {
            _privilegeName = privilege;
        }

        internal PrivilegeNotHeldException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            _privilegeName = info.GetString("PrivilegeName");
        }

        public string PrivilegeName
        {
            get { return _privilegeName; }
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            if ( info == null )
            {
                throw new ArgumentNullException( "info" );
            }
            Contract.EndContractBlock();

            base.GetObjectData(info, context);

            info.AddValue("PrivilegeName", _privilegeName, typeof( string ));
        }
    }
}
