//
// System.Windows.Forms.Cursor.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the image used to paint the mouse pointer.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	[Serializable]
	public sealed class Cursor : IDisposable, ISerializable {

		#region Fields
		#endregion
		
		
		
		#region Constructors
		[MonoTODO]
		public Cursor(IntPtr handle) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Cursor(Stream stream) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Cursor(string fileName) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Cursor(Type type,string resource) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		
		
		
		#region Properties
		[MonoTODO]
		public static Rectangle Clip {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static Cursor Current {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public IntPtr Handle {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static Point Position {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public Size Size {
			get { throw new NotImplementedException (); }
		}
		#endregion
		
		
		
		
		#region Methods
		[MonoTODO]
		public IntPtr CopyHandle() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Dispose() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Draw(Graphics g,Rectangle targetRect) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DrawStretched(Graphics g,Rectangle targetRect) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool Equals(object obj) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		~Cursor() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void Hide() 
		{
			throw new NotImplementedException ();
		}
		
		/// ISerializable.GetObjectData only supports .NET framework:
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo si,StreamingContext context) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void Show() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		
		
		#region Operators
		[MonoTODO]
		public static bool operator ==(Cursor left, Cursor right) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool operator !=(Cursor left, Cursor right) 
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
