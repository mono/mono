// System.ComponentModel.Design.Serialization.IDesignerSerializationManager.cs
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

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerSerializationManager : IServiceProvider
	{
		ContextStack Context {get;}

		PropertyDescriptorCollection Properties {get;}

		void AddSerializationProvider (IDesignerSerializationProvider provider);

		object CreateInstance (Type type, ICollection arguments, string name, bool addToContainer);

		object GetInstance (string name);

		string GetName (object value);

		object GetSerializer (Type objectType, Type serializerType);

		Type GetType (string typeName);

		void RemoveSerializationProvider (IDesignerSerializationProvider provider);

		void ReportError (object errorInformation);

		void SetName (object instance, string name);

		event ResolveNameEventHandler ResolveName;

		event EventHandler SerializationComplete;
	}
}
