//
// System.Drawing.Message.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
//TODO uncomment and implment GetLParam.
using System;

namespace System.Windows.Forms {
	[Serializable]
	public struct Message { 

		private Msg msg;
		private IntPtr hwnd;
		private IntPtr lparam;
		private IntPtr wparam;
		private IntPtr result;


		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Message objects. The return value is
		///	based on the equivalence of the Msg, HWnd, LParam,
		///	 WParam, and Result properties of the two objects.
		/// </remarks>

		public static bool operator == (Message msg_a, 
			Message msg_b) {

			return ((msg_a.msg == msg_b.msg) &&
				(msg_a.hwnd == msg_b.hwnd) &&
				(msg_a.lparam == msg_b.lparam) &&
				(msg_a.wparam == msg_b.wparam) &&
				(msg_a.result == msg_b.result));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Message objects. The return value is
		///	based on the equivalence of the Msg, HWnd, LParam,
		///	 WParam, and Result properties of the two objects.
		/// </remarks>

		public static bool operator != (Message msg_a, 
			Message msg_b) {
			return ((msg_a.msg != msg_b.msg) ||
				(msg_a.hwnd != msg_b.hwnd) ||
				(msg_a.lparam != msg_b.lparam) ||
				(msg_a.wparam != msg_b.wparam) ||
				(msg_a.result != msg_b.result));
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------

		public Msg Msg {
			get{
				return msg;
			}
			set{
				msg = value;
			}
		}

		public IntPtr HWnd {
			get{
				return hwnd;
			}
			set{
				hwnd = value;
			}
		}

		public IntPtr LParam {
			get{
				return lparam;
			}
			set{
				lparam = value;
			}
		}

		public IntPtr WParam {
			get{
				return wparam;
			}
			set{
				wparam = value;
			}
		}

		public IntPtr Result {
			get{
				return result;
			}
			set{
				result = value;
			}
		}

		internal uint HiWordWParam {
			get {
				return ((uint)WParam.ToInt32() & 0xFFFF0000) >> 16;
			}
		}

		internal uint LoWordWParam {
			get {
				return (uint)((uint)WParam.ToInt32() & 0x0000FFFFL);
			}
		}

		internal int HiWordLParam {
			get {
				return (int)(((uint)LParam.ToInt32() & 0xFFFF0000) >> 16);
			}
		}

		internal int LoWordLParam {
			get {
				return LParam.ToInt32() & 0x0000FFFF;
			}
		}

		public static Message create(IntPtr hWnd, Msg msg, IntPtr wparam, IntPtr lparam)
		{
			Message NewMessage =  new Message();
			NewMessage.msg = msg;
			NewMessage.wparam = wparam;
			NewMessage.lparam = lparam;
			NewMessage.hwnd = hWnd;
			return NewMessage;
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Message and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is Message))
				return false;

			return (this == (Message) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return base.GetHashCode();// (int)( msg ^ lparam ^ wparam ^ result ^ whnd);
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Message as a string.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1},{2},{3},{4}]", msg.ToString(), lparam.ToString(), wparam.ToString(), result.ToString(), hwnd.ToString());
		}

//		public object GetLParam(Type cls){
//			//	throw new NotImplementedException ();
//			//return (object) lparam;
//		}
	}
}
