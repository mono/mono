//
// MonoTests.System.Runtime.Remoting.CallSeq.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Threading;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	public class CallSeq
	{
		static ArrayList calls = new ArrayList();
		static int checkPos = 0;
		static int writePos = 0;
		static string name = "";

		public static void Add (string msg)
		{
			writePos++;
			msg = writePos.ToString ("000") + " (d" + Thread.GetDomainID() + ",c" + Thread.CurrentContext.ContextID + ") " + msg;
			calls.Add (msg);
		}

		public static void Init (string str)
		{
			calls = new ArrayList();
			name = str;
			checkPos = 0;
			writePos = 0;
		}

		public static void Check (string msg, int domain)
		{
			bool optional = false;
			if (msg.StartsWith ("#"))
			{
				optional = true;
				msg = msg.Substring (1);
			}

			if (msg[6].ToString() != domain.ToString()) return;

			if (checkPos >= calls.Count)
			{
				if (!optional) Assertion.Fail ("[" + name + "] Call check failed. Expected call not made: \"" + msg + "\"");
				else return;
			}

			string call = (string) calls[checkPos++];

			if (msg.Substring (3) != call.Substring (3))
			{
				if (optional) checkPos--;
				else Assertion.Fail ("[" + name + "] Call check failed in step " + (checkPos+1) + ". Expected \"" + msg + "\" found \"" + call + "\"");
			}
		}

		public static void Check (string[] msgs, int domain)
		{
			foreach (string msg in msgs)
				Check (msg, domain);
		}

		public static ArrayList Seq
		{
			get { return calls; }
			set { calls = value; }
		}
	}
}
