using System;
using System.Collections.Generic;
using System.Text;

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml
#else
namespace System.Xaml
#endif 
{
    public class XamlCreatedObjectEventArgs : EventArgs
    {
        public XamlCreatedObjectEventArgs(Object createdObject)
        {
            if (createdObject == null)
            {
                throw new ArgumentNullException("createdObject");
            }

            CreatedObject = createdObject;
        }

        public Object CreatedObject { get; private set; }
    }
}
