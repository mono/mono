//
// XmlDsigC14NTransform.cs - C14N Transform implementation for XML Signature
// http://www.w3.org/TR/xml-c14n
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

public class XmlDsigC14NTransform : Transform {

	protected bool comments;

	public XmlDsigC14NTransform () 
	{
		comments = false;
	}

	public XmlDsigC14NTransform (bool includeComments) 
	{
		comments = includeComments;
	}

	public override Type[] InputTypes {
		get {
			if (input == null) {
				lock (this) {
					// this way the result is cached if called multiple time
					input = new Type [3];
					input[0] = typeof (System.IO.Stream);
					input[1] = typeof (System.Xml.XmlDocument);
					input[2] = typeof (System.Xml.XmlNodeList);
				}
			}
			return input;
		}
	}

	public override Type[] OutputTypes {
		get {
			if (output == null) {
				lock (this) {
					// this way the result is cached if called multiple time
					output = new Type [1];
					output[0] = typeof (System.IO.Stream);
				}
			}
			return output;
		}
	}

	protected override XmlNodeList GetInnerXml () 
	{
		return null; // THIS IS DOCUMENTED AS SUCH
	}

	public override object GetOutput () 
	{
//		return (object) new Stream ();
		return null;
	}

	public override object GetOutput (Type type) 
	{
		if (type == Type.GetType ("Stream"))
			return GetOutput ();
		throw new ArgumentException ("type");
	}

	public override void LoadInnerXml (XmlNodeList nodeList) 
	{
		// NO CHANGE
	}

	public override void LoadInput (object obj) 
	{
	//	if (type.Equals (Stream.GetType ())
	}
}

}
