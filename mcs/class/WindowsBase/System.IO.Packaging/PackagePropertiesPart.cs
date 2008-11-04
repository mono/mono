// PackagePropertiesPart.cs created with MonoDevelop
// User: alan at 11:07 04/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace System.IO.Packaging
{
	class PackagePropertiesPart : PackageProperties
	{
		public override string Category {
			get; set;
		}
		
		public override string ContentStatus {
			get; set;
		}
		
		public override string ContentType {
			get; set;
		}
		
		public override Nullable<DateTime> Created {
			get; set;
		}
		
		public override string Creator {
			get; set;
		}
		
		public override string Description {
			get; set;
		}
		
		public override string Identifier {
			get; set;
		}
		
		public override string Keywords {
			get; set;
		}
		
		public override string Language {
			get; set;
		}
		
		public override string LastModifiedBy {
			get; set;
		}

		public override Nullable<DateTime> LastPrinted {
			get; set;
		}
		
		public override Nullable<DateTime> Modified {
			get; set;
		}
		
		public override string Revision {
			get; set;
		}

		public override string Subject {
			get; set;
		}
		
		public override string Title {
			get; set;
		}

		public override string Version {
			get; set;
		}
		
		public override void Dispose (bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
