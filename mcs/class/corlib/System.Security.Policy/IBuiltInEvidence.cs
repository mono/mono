//
// System.Security.Policy.IBuiltInEvidence
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Security.Policy
{
	interface IBuiltInEvidence
	{
		int GetRequiredSize (bool verbose);
		int InitFromBuffer (char [] buffer, int position);
		int OutputToBuffer (char [] buffer, int position, bool verbose);
	}
}

