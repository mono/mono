//
// System.Management.Instrumentation.InstrumentationClassAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | 
			AttributeTargets.Struct)]
	public class InstrumentationClassAttribute : Attribute {
		public InstrumentationClassAttribute (InstrumentationType instrumentationType)
		{
			_instrumentationType = instrumentationType;
		}

		public InstrumentationClassAttribute (InstrumentationType instrumentationType, string managedBaseClassName)
		{
			_instrumentationType = instrumentationType;
			_managedBaseClassName = managedBaseClassName;
		}

		public InstrumentationType InstrumentationType {
			get {
				return _instrumentationType;
			}
		}

		public string ManagedBaseClassName {
			get {
				if (_managedBaseClassName == null || _managedBaseClassName.Length == 0)
					return null;

				return _managedBaseClassName;
			}
		}

		private InstrumentationType _instrumentationType;
		private string _managedBaseClassName;
	}
}
