//
// System.ServiceProcess.ServiceProcessDescriptionAttribute.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.ComponentModel;

namespace System.ServiceProcess {

        [Serializable]
        [AttributeUsage (AttributeTargets.All)]
        public class ServiceProcessDescriptionAttribute : DescriptionAttribute
        {
                string description;
                
                public ServiceProcessDescriptionAttribute (string description)
                {
                        this.description = description;
                }

                public override string Description {

                        get { return description; }

                }
        }
}
