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
