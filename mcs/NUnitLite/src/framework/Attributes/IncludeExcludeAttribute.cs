// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// Abstract base for Attributes that are used to include tests
	/// in the test run based on environmental settings.
	/// </summary>
	public abstract class IncludeExcludeAttribute : NUnitAttribute
	{
		private string include;
		private string exclude;
		private string reason;

		/// <summary>
		/// Constructor with no included items specified, for use
		/// with named property syntax.
		/// </summary>
		public IncludeExcludeAttribute() { }

		/// <summary>
		/// Constructor taking one or more included items
		/// </summary>
		/// <param name="include">Comma-delimited list of included items</param>
		public IncludeExcludeAttribute( string include )
		{
			this.include = include;
		}

		/// <summary>
		/// Name of the item that is needed in order for
		/// a test to run. Multiple itemss may be given,
		/// separated by a comma.
		/// </summary>
		public string Include
		{
			get { return this.include; }
			set { include = value; }
		}

		/// <summary>
		/// Name of the item to be excluded. Multiple items
		/// may be given, separated by a comma.
		/// </summary>
		public string Exclude
		{
			get { return this.exclude; }
			set { this.exclude = value; }
		}

		/// <summary>
		/// The reason for including or excluding the test
		/// </summary>
		public string Reason
		{
			get { return reason; }
			set { reason = value; }
		}
	}
}
