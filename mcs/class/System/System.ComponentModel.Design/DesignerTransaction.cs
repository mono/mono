// System.ComponentModel.Design.DesignerTransaction.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System;

namespace System.ComponentModel.Design
{
	public abstract class DesignerTransaction : IDisposable
	{
		[MonoTODO]
		public DesignerTransaction () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DesignerTransaction (string description) {
			throw new NotImplementedException ();
		}
		
		void IDisposable.Dispose () 
		{ 
			this.Dispose(); 
		}
		
		public abstract void Dispose();

		public bool Canceled 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Committed
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public string Description 
		{
			get {
				throw new NotImplementedException ();
			}
		}				
	}
}
