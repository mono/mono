// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;


namespace System.Windows.Markup
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class NameScopePropertyAttribute : Attribute
    {
        private string _name;
        private Type _type;

        // Methods
        public NameScopePropertyAttribute (string name)
        {
            _name = name;
        }

        public NameScopePropertyAttribute (string name, Type type)
        {
            _name = name;
            _type = type;
        }


        // Properties
        public string Name { get{ return _name;} }
        public Type Type { get{ return _type;} }
    }
}