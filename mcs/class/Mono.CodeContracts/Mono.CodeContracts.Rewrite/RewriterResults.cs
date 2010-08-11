using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite {

	public class RewriterResults {

		internal static RewriterResults Warning (string warning)
		{
			return new RewriterResults (new [] { warning }, null);
		}

		internal static RewriterResults Error (string error)
		{
			return new RewriterResults (null, new [] { error });
		}

		internal RewriterResults (ICollection<string> warnings, ICollection<string> errors)
		{
			this.warnings = warnings;
			this.errors = errors;
		}

		private ICollection<string> warnings, errors;

		public bool AnyWarnings
		{
			get
			{
				return this.warnings != null && this.warnings.Count > 0;
			}
		}

		public bool AnyErrors
		{
			get
			{
				return this.errors != null && this.errors.Count > 0;
			}
		}

		public IEnumerable<string> Warnings
		{
			get
			{
				return this.warnings ?? Enumerable.Empty<string> ();
			}
		}

		public IEnumerable<string> Errors
		{
			get
			{
				return this.errors ?? Enumerable.Empty<string> ();
			}
		}

	}

}
