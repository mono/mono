// System.ComponentModel.Design.Serialization.CodeDomSerializer.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public abstract class CodeDomSerializer
	{
		[MonoTODO]
		protected CodeDomSerializer() {
			throw new NotImplementedException ();
		}

		public abstract object Deserialize (IDesignerSerializationManager manager, object codeObject);

		[MonoTODO]
		protected void DeserializePropertiesFromResources (IDesignerSerializationManager manager, object value, Attribute[] filter) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object DeserializeExpression (IDesignerSerializationManager manager, string name, CodeExpression expression) 
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]		
		protected void DeserializeStatement (IDesignerSerializationManager manager, CodeStatement statement) 
		{	
			throw new NotImplementedException ();
		}

		public abstract object Serialize (IDesignerSerializationManager manager, object value);

		[MonoTODO]
		protected void SerializeEvents (IDesignerSerializationManager manager, CodeStatementCollection statements, object value, Attribute[] filter) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SerializeProperties (IDesignerSerializationManager manager, CodeStatementCollection statements, object value, Attribute[] filter) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SerializePropertiesToResources (IDesignerSerializationManager manager, CodeStatementCollection statements, object value, Attribute[] filter) 
		{
			throw new NotImplementedException ();
		}
			 

		[MonoTODO]
		protected void SerializeResource (IDesignerSerializationManager manager, string resourceName, object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SerializeResourceInvariant (IDesignerSerializationManager manager, string resourceName, object value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected CodeExpression SerializeToExpression (IDesignerSerializationManager manager, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected CodeExpression SerializeToReferenceExpression (IDesignerSerializationManager manager, object value)
		{
			throw new NotImplementedException ();
		}
	}
}
