// System.ComponentModel.Design.Serialization.CodeDomSerializer.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.CodeDom;
using System.Web.UI.Design;

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
