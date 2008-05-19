#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
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