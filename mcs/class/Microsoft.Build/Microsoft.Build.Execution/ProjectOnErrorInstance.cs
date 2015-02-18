//
// ProjectOnErrorInstance.cs
//
// Author:
//    Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2013 Xamarin Inc.
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
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public class ProjectOnErrorInstance : ProjectTargetInstanceChild
	{
		internal ProjectOnErrorInstance (ProjectOnErrorElement xml)
		{
			condition = xml.Condition;
			ExecuteTargets = xml.ExecuteTargetsAttribute;
			//this.FullPath = fullPath;
			condition_location = xml.ConditionLocation;
			ExecuteTargetsLocation = xml.ExecuteTargetsAttributeLocation;
			location = xml.Location;
		}
		
		readonly string condition;
		
		public override string Condition {
			get { return condition; }
		}

		public string ExecuteTargets { get; private set; }
		
		readonly ElementLocation condition_location, location;
		
		public
		override ElementLocation ConditionLocation {
			get { return condition_location; }
		}

		public
		ElementLocation ExecuteTargetsLocation { get; private set; }

		public
		override ElementLocation Location {
			get { return location; }
		}
	}
}

