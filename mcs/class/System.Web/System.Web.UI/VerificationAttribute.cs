//
// System.Web.UI.VerificationAttribute.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
//
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

#if NET_2_0
namespace System.Web.UI
{
	[AttributeUsageAttribute (AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public sealed class VerificationAttribute : Attribute
	{
		public VerificationAttribute (string guideline,
										string checkpoint,
										VerificationReportLevel reportLevel,
										int priority,
										string message) {
		}
		public VerificationAttribute (string guideline,
										string checkpoint,
										VerificationReportLevel reportLevel,
										int priority,
										string message,
										VerificationRule rule,
										string conditionalProperty) {
		}
		public VerificationAttribute (string guideline,
										string checkpoint,
										VerificationReportLevel reportLevel,
										int priority,
										string message,
										VerificationRule rule,
										string conditionalProperty,
										VerificationConditionalOperator conditionalOperator,
										string conditionalValue,
										string guidelineUrl) {
		}
		public string Checkpoint {
			get {
				throw new NotImplementedException ();
			}
		}
		public string ConditionalProperty {
			get {
				throw new NotImplementedException ();
			}
		}
		public string ConditionalValue {
			get {
				throw new NotImplementedException ();
			}
		}
		public string Guideline {
			get {
				throw new NotImplementedException ();
			}
		}
		public string GuidelineUrl {
			get {
				throw new NotImplementedException ();
			}
		}
		public string Message {
			get {
				throw new NotImplementedException ();
			}
		}
		public int Priority {
			get {
				throw new NotImplementedException ();
			}
		}
		public VerificationConditionalOperator VerificationConditionalOperator {
			get {
				throw new NotImplementedException ();
			}
		}
		public VerificationReportLevel VerificationReportLevel {
			get {
				throw new NotImplementedException ();
			}
		}
		public VerificationRule VerificationRule {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
#endif
