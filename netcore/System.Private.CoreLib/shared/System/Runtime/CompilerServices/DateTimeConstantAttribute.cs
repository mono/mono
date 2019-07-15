// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    public sealed class DateTimeConstantAttribute : CustomConstantAttribute
    {
        private DateTime _date;

        public DateTimeConstantAttribute(long ticks)
        {
            _date = new DateTime(ticks);
        }

#pragma warning disable CS8608
        public override object Value => _date;
#pragma warning restore CS8608
    }
}
