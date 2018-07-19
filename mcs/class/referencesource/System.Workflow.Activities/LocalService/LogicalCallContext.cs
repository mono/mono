#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    internal sealed class IdentityContextData : ILogicalThreadAffinative, ISerializable
    {
        internal const string IdentityContext = "__identitycontext__";
        
        String identity;

        internal IdentityContextData(String identity)
        {
            this.identity = identity;
        }

        private IdentityContextData(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name.Equals("identity"))
                {
                    this.identity = (String)enumerator.Value;
                }
            }
        }

        [SecurityPermission( SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.identity != null)
                info.AddValue("identity", identity.ToString());
        }

        internal String Identity
        {
            get
            {
                return identity;
            }
        }
    }
}
