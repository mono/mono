//
// System.Windows.Forms.Cursor
//
// Author:
//		Alberto Fernandez (infjaf00@yahoo.es)
//


namespace System.Windows.Forms{

	using System;
	using System.Drawing;
	using System.Runtime.Serialization;
	using System.IO;

	public sealed class Cursor : IDisposable, ISerializable {
	
		internal Cursor(){
		}
		[MonoTODO]
		public Cursor (IntPtr handle){
			if (handle == IntPtr.Zero){
				throw new ArgumentException();
			}
		}
		[MonoTODO]
		public Cursor (Stream stream){
		}
		[MonoTODO]
		public Cursor (string fileName){
		}
		[MonoTODO]
		public Cursor (Type type, string resource){
		}
		[MonoTODO]
		public static Rectangle Clip { 
			// Rectángulo por el que se puede mover el cursor.
			// Generalmente toda la pantalla.
			get {throw new NotImplementedException();}
			set {throw new NotImplementedException();}
		}
		[MonoTODO]
		public static Cursor Current {
			// Si se pone en un valor que no sea default, la aplicación
			// deja de oir a los eventos de mouse.
			// cuando este en Cursor.Default, la aplicación vuelve
			// a escuchar los eventos de ratón.

			get {throw new NotImplementedException();}
			set {throw new NotImplementedException();}
		}
		[MonoTODO]
		public IntPtr Handle {
			get {throw new Exception();}
		}
		[MonoTODO]
		public static Point Position{
			get {throw new NotImplementedException();}
			set {throw new NotImplementedException();}
		}
		[MonoTODO]
		public Size Size{
			get {throw new NotImplementedException();}
		}
		[MonoTODO]
		public IntPtr CopyHandle (){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Dispose (){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Draw (Graphics g, Rectangle targetRect){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void DrawStretched(Graphics g, Rectangle targetRect){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override bool Equals (Object obj){
			throw new NotImplementedException();
		}
		[MonoTODO]
		~Cursor(){
		}
		[MonoTODO]
		public override int GetHashCode(){
			throw new NotImplementedException();
		}
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static void Hide(){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static void Show(){
			throw new NotImplementedException();	
		}
		[MonoTODO]
		public override String ToString(){
			return base.ToString();
		}
		public static bool operator==(Cursor left, Cursor right){
			return left.Equals(right);
		}
		public static bool operator!=(Cursor left, Cursor right) {
			return ( !(left == right));
		}
	}
}
