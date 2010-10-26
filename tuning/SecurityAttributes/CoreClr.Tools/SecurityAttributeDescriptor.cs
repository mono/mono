
using System;

namespace CoreClr.Tools
{
	public class SecurityAttributeDescriptor
	{
		public SecurityAttributeDescriptor(SecurityAttributeType attributeType, TargetKind target, string signature)
			: this(SecurityAttributeOverride.None, attributeType, target, signature)
		{	
		}

		public SecurityAttributeDescriptor(SecurityAttributeOverride @override, SecurityAttributeType attributeType, TargetKind target, string signature)
		{
			this.Override = @override;
			this.AttributeType = attributeType;
			this.Target = target;
			this.Signature = signature;
		}

		public SecurityAttributeOverride Override { get; private set; }
		public string Signature { get; private set; }
		public TargetKind Target { get; private set; }
		public SecurityAttributeType AttributeType { get; private set; }

		public string AttributeTypeName
		{
			get
			{
			    return SecurityAttributeTypeNames.AttributeTypeNameFor(AttributeType);
			}
		}

	    public override string ToString()
		{
			return string.Format("{0}{1}-{2}: {3}", OverrideString(), AttributeTypeString(), TargetKindString(), Signature);
		}

		private string TargetKindString()
		{
			switch (Target)
			{
				case TargetKind.Type:
					return "T";
				case TargetKind.Method:
					return "M";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private string AttributeTypeString()
		{
			switch (AttributeType)
			{
				case SecurityAttributeType.Critical:
					return "SC";
				case SecurityAttributeType.SafeCritical:
					return "SSC";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private string OverrideString()
		{
			switch (Override)
			{
				case SecurityAttributeOverride.None:
					return string.Empty;
				case SecurityAttributeOverride.Add:
					return "+";
				case SecurityAttributeOverride.Remove:
					return "-";
				case SecurityAttributeOverride.Force:
					return "!";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != typeof(SecurityAttributeDescriptor))
				return false;
			var other = (SecurityAttributeDescriptor)obj;
			return Override == other.Override
				&& Signature == other.Signature
				&& Target == other.Target
				&& AttributeType == other.AttributeType;
		}

		public override int GetHashCode()
		{
			return Signature.GetHashCode() ^ Target.GetHashCode();
		}
	}

	public enum TargetKind
	{
		Type,
		Method
	}

	public enum SecurityAttributeType
	{
		Critical,
		SafeCritical
	}

	public enum SecurityAttributeOverride
	{
		None,
		Add,
		Remove,
		Force
	}

}
