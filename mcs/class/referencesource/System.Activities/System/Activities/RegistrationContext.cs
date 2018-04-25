//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class RegistrationContext
    {
        ExecutionPropertyManager properties;
        IdSpace currentIdSpace;

        internal RegistrationContext(ExecutionPropertyManager properties, IdSpace currentIdSpace)
        {
            this.properties = properties;
            this.currentIdSpace = currentIdSpace;
        }

        public object FindProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (this.properties == null)
            {
                return null;
            }
            else
            {
                return this.properties.GetProperty(name, this.currentIdSpace);
            }
        }
    }
}


