//
// MonoTests.Remoting.CallSeq.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Threading;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	public class CallSeq
	{
		static ArrayList calls = new ArrayList();
		static int checkPos = 0;
		static int writePos = 0;
		static string name = "";
		static ArrayList contexts = new ArrayList ();
		static int domId = 1;

		public static void Add (string msg)
		{
			writePos++;

			msg = writePos.ToString ("000") + " (d" + CommonDomainId + ",c" + CommonContextId + ") " + msg;
			calls.Add (msg);
		}

		public static int CommonContextId
		{
			get
			{
				int id = Thread.CurrentContext.ContextID;
				int idc = contexts.IndexOf (id);
				if (idc == -1)
				{
					idc = contexts.Count;
					contexts.Add (id);
				}
				return idc;
			}
		}

		public static int CommonDomainId
		{
			get { return domId; }
			set { domId = value; }
		}

		public static void Init (string str)
		{
			calls = new ArrayList();
			contexts = new ArrayList ();
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
				if (!optional) Assert.Fail ("[" + name + "] Call check failed. Expected call not made: \"" + msg + "\"");
				else return;
			}

			string call = (string) calls[checkPos++];

			if (msg.Substring (3) != call.Substring (3))
			{
				if (optional) checkPos--;
				else Assert.Fail ("[" + name + "] Call check failed in step " + (checkPos+1) + ". Expected \"" + msg + "\" found \"" + call + "\"");
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
