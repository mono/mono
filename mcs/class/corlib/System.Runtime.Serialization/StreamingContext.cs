//
// System.Runtime.Serialization.StreamingContext.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Serialization {

	public struct StreamingContext {
		StreamingContextStates state;
		object additional;
		
		public StreamingContext (StreamingContextStates state)
		{
			this.state = state;
			additional = null;
		}

		public StreamingContext (StreamingContextStates state, object additional)
		{
			this.state = state;
			this.additional = additional;
		}

		public object Context {
			get {
				return additional;
			}
		}

		public StreamingContextStates {
			get {
				return state;
			}
		}

		public bool Equals (Object o)
		{
			StreamingContext other;
			
			if (!(o is StreamingContext))
				return false;

			other = (StreamingContext) o;

			return (other.state == this.state) && (other.additional == this.additional);
		}

		public int GetHashCode ()
		{
			// FIXME: Improve this?  Is this worth it?
			
			return o.GetHashCode ();
		}
	}
}
