// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System;
    using System.Reflection;

    /// <summary>Indicates that field should be treated as a xml attribute for the task.</summary>
    /// <example>
    /// Examples of how to specify task attributes
    /// <code>
    /// // task XmlType default is string
    /// [TaskAttribute("out", Required=true)]
    /// string _out = null; // assign default value here
    ///
    /// [TaskAttribute("optimize")]
    /// [BooleanValidator()]
    /// // during ExecuteTask you can safely use Convert.ToBoolean(_optimize)
    /// string _optimize = Boolean.FalseString;
    ///
    /// [TaskAttribute("warnlevel")]
    /// [Int32Validator(0,4)] // limit values to 0-4
    /// // during ExecuteTask you can safely use Convert.ToInt32(_optimize)
    /// string _warnlevel = "0";
    ///
    /// [TaskFileSet("sources")]
    /// FileSet _sources = new FileSet();
    /// </code>
    /// NOTE: Attribute values must be of type of string if you want
    /// to be able to have macros.  The field stores the exact value during
    /// InitializeTask.  Just before ExecuteTask is called NAnt will expand
    /// all the macros with the current values.
    [AttributeUsage(AttributeTargets.Field, Inherited=true)]
    public class TaskAttributeAttribute : Attribute {

        string _name;
        bool _required;
        bool _expandText;

        public TaskAttributeAttribute(string name) {
            Name = name;
            Required = false;
            ExpandText = true;
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public bool Required {
            get { return _required; }
            set { _required = value; }
        }

        public bool ExpandText {
            get { return _expandText; }
            set { _expandText = value; }
        }
    }
}
