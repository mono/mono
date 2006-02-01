// This is used to debug an ordering dependent bug.
//
// Compiler options: support-388.cs -out:test-388.exe

namespace Schemas {
    [System.Xml.Serialization.XmlType("base-field-type"),
    System.Xml.Serialization.XmlInclude(typeof(compoundfield)),
    System.Xml.Serialization.XmlInclude(typeof(fieldtype))]
    public partial class basefieldtype {

        [System.Xml.Serialization.XmlAttribute(DataType="ID")]
        public string id;

        [System.Xml.Serialization.XmlAttribute()]
        public string datatype;
    }

    [System.Xml.Serialization.XmlType("field-type")]
    public partial class fieldtype: basefieldtype {}

    [System.Xml.Serialization.XmlType("compound-field")]
    public partial class compoundfield: basefieldtype {}

    public partial class field {

        [System.Xml.Serialization.XmlAttribute()]
        public string id;

        [System.Xml.Serialization.XmlAttribute()]
        public string type;
    }

    [System.Xml.Serialization.XmlType("form-data")]
    public partial class formdata {

        [System.Xml.Serialization.XmlArray(ElementName="form-fields"),
        System.Xml.Serialization.XmlArrayItem(Type=typeof(field),IsNullable=false)]
        public field[] formfields;
        
		[System.Xml.Serialization.XmlElement("field-type",Type=typeof(fieldtype)),
        System.Xml.Serialization.XmlElement("compound-field",Type=typeof(compoundfield))]
        public basefieldtype[] Items;
    }

    public class M {
	public static void Main () {}
    }
}
