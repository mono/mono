//
// TransformChain.cs - TransformChain implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;

namespace System.Security.Cryptography.Xml { 

public class TransformChain {

	private ArrayList chain;

	public TransformChain() 
	{
		chain = new ArrayList ();
	}

	public int Count {
		get { return chain.Count; }
	}

	public Transform this [int index] {
		get { return (Transform) chain [index]; }
	}

	public void Add (Transform transform) 
	{
		chain.Add (transform);
	}

	public IEnumerator GetEnumerator () 
	{
		return chain.GetEnumerator ();
	}
}

}
