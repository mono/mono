#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a JavaScript constructor.
    /// </summary>
    sealed class JavaScriptConstructor
    {
        private string _name;
		private JavaScriptParameters _parameters;

		public JavaScriptParameters Parameters
		{
			get { return _parameters; }
		}

		public string Name
		{
			get { return _name; }
		}

        public JavaScriptConstructor(string name, JavaScriptParameters parameters)
        {
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Length == 0)
				throw new ArgumentException("Constructor name cannot be empty.", "name");

			_name = name;
			_parameters = parameters ?? JavaScriptParameters.Empty;
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("new ");
			sb.Append(_name);
			sb.Append("(");
			if (_parameters != null)
			{
				for (int i = 0; i < _parameters.Count; i++)
				{
					sb.Append(_parameters[i]);
				}
			}
			sb.Append(")");

			return sb.ToString();
		}
    }
}