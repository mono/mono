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
			
		}
		
		[MonoTODO]
		public Cursor(Stream stream) 
		{
			
		}
		
		[MonoTODO]
		public Cursor(string fileName) 
		{
			
		}
		
		[MonoTODO]
		public Cursor(Type type,string resource) 
		{
			
		}
		#endregion
		
		#region Properties
		[MonoTODO]
		public static Rectangle Clip {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor Current {
			get { 
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
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
			//FIXME:
			return base.Equals(obj);
		}
		
		[MonoTODO]
		~Cursor() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override int GetHashCode() 
		{
			//FIXME:
			return base.GetHashCode();
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
			//FIXME:
			return base.ToString();
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
