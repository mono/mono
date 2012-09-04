//
// System.Diagnostics.EventSourceCreationData
//
// Author:
//	Gert Driesen <driesen@users.sourceforge.net>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Diagnostics
{
	public class EventSourceCreationData
	{
		string _source;
		string _logName;
		string _machineName;
		string _messageResourceFile;
		string _parameterResourceFile;
		string _categoryResourceFile;
		int _categoryCount;

		public EventSourceCreationData (string source, string logName)
		{
			_source = source;
			_logName = logName;
			_machineName = ".";
		}

		internal EventSourceCreationData (string source, string logName, string machineName)
		{
			_source = source;
			if (logName == null || logName.Length == 0) {
				_logName = "Application";
			} else {
				_logName = logName;
			}
			_machineName = machineName;
		}

		public int CategoryCount
		{
			get { return _categoryCount; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				_categoryCount = value;
			}
		}

		public string CategoryResourceFile
		{
			get { return _categoryResourceFile; }
			set { this._categoryResourceFile = value; }
		}

		public string LogName
		{
			get { return _logName; }
			set { this._logName = value; }
		}

		public string MachineName
		{
			get { return _machineName; }
			set { _machineName = value; }
		}

		public string MessageResourceFile
		{
			get { return _messageResourceFile; }
			set { _messageResourceFile = value; }
		}

		public string ParameterResourceFile
		{
			get { return _parameterResourceFile; }
			set { _parameterResourceFile = value; }
		}

		public string Source
		{
			get { return _source; }
			set { _source = value; }
		}
	}
}
