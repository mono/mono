//
// System.ComponentModel.ProvidePropertyAttribute.cs
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ProvidePropertyAttribute : Attribute
	{

		private string Property;
		private string Receiver;

		public ProvidePropertyAttribute (string propertyName, string receiverTypeName)
		{
			Property = propertyName;
			Receiver = receiverTypeName;
		}

		public ProvidePropertyAttribute (string propertyName, Type receiverType)
		{
			Property = propertyName;
			Receiver = receiverType.AssemblyQualifiedName;
		}

		public string PropertyName {
			get { return Property; }
		}

		public string ReceiverTypeName {
			get { return Receiver; }
		}

		public override object TypeId {
			get { 
				// seems to be MS implementation
				return base.TypeId + Property; 
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ProvidePropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return (((ProvidePropertyAttribute) obj).PropertyName == Property) &&
				(((ProvidePropertyAttribute) obj).ReceiverTypeName == Receiver);
		}

		public override int GetHashCode()
		{
			return (Property + Receiver).GetHashCode ();
		}
	}
}
