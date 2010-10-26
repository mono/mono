using Mono.Cecil;

namespace CoreClr.Tools
{
    public class CecilSecurityAttributeDescriptor
    {
        public IMemberDefinition Member { get; private set; }
        public SecurityAttributeType SecurityAttributeType { get; private set; }

        public CecilSecurityAttributeDescriptor(IMemberDefinition member, SecurityAttributeType securityAttributeType)
        {
            Member = member;
            SecurityAttributeType = securityAttributeType;
        }


        public override bool Equals(object obj)
        {
            var other = obj as CecilSecurityAttributeDescriptor;
            if (other == null) return false;
            return Member == other.Member && SecurityAttributeType == other.SecurityAttributeType;
        }

        public override string ToString()
        {
            return "Member: " + Member + " Attr: " + SecurityAttributeType;
        }
    }
}