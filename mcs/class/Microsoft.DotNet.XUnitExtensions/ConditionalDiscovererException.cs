using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.DotNet.XUnitExtensions
{
    internal class ConditionalDiscovererException : Exception
    {
        public ConditionalDiscovererException(string message) : base(message) { }
    }
}
