//
// System.Data.MergeFailedEventArgs.cs
//
// Authors:
//   John Dugaw <jdugaw@unizenconsulting.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) John Dugaw
//

using System;
using System.Data;

namespace System.Data
{
	public class MergeFailedEventArgs : EventArgs
	{
		private DataTable table;
		private string conflict;

		public MergeFailedEventArgs ( DataTable newTable, string newConflict) {
			table = newTable;
			conflict = newConflict;
		}

		public string Conflict {
			get {
				return conflict;
			}
			/* TODO
			 * set included as some versions of mcs fail without them 
			 * must be removed before going live
			 */
			set {
				System.Console.WriteLine("ERROR: <instance> System.Data.MerFailedEventArgs.Conflict is a read only property!\n");
			}

		}
		
		public DataTable Table {
			get {
				return table;
			}
			/* TODO
			 * set included as some versions of mcs fail without them 
			 *    should be removed
			 */
			set {
				System.Console.WriteLine("ERROR: <instance> System.Data.MergeFailedEventArgs.Table is a read only property!\n");
			}
		}

		[Serializable]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		public override bool Equals (MergeFailedEventsArgs eq) {
			throw new NotImplementedException ();
		}

		[Serializable]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		public static override bool Equals(MergeFailedEventArgs a, MergeFailedEventArgs b) {
			throw new NotImplementedException ();
		}

		/* TODO
		 * These can be uncommented and should be implemented or removed
		 * before production.
		 [Serializable]
		 [ClassInterface(ClassInterfaceType.AutoDual)]
		 public override int GetHashCode() {
		 System.Console.WriteLine("<instance> System.Data.MergeFailedEventArgs.GetHashCode() called");
		 }

		 [Serializable]
		 [ClassInterface(ClassInterfaceType.AutoDual)]
		 public override Type GetType()
		 {
		 System.Console.WriteLine("<instance> System.Data.MergeFailedEventArgs.GetType() called");
		 }
		*/

		public override string ToString() {
			throw new NotImplementedException ();
		}

		protected object MemberwiseClone() {
			throw new NotImplementedException ();
		}
   }
} // end namespace declaration
