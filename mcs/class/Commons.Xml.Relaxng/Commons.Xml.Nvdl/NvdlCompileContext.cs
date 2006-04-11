using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Commons.Xml;

namespace Commons.Xml.Nvdl
{
	internal class NvdlCompileContext
	{
		NvdlRules rules;
		NvdlConfig config;
		XmlResolver resolver;
		Hashtable compiledModes = new Hashtable ();
		Hashtable modeContexts = new Hashtable ();
		Hashtable cancelledRules = new Hashtable ();
		Hashtable ruleContexts = new Hashtable ();

		public NvdlCompileContext (NvdlRules rules, NvdlConfig config, XmlResolver resolver)
		{
			this.rules = rules;
			this.config = config;
			this.resolver = resolver;
		}

		public NvdlRules Rules {
			get { return rules; }
		}

		public NvdlConfig Config {
			get { return config; }
		}

		internal XmlResolver XmlResolver {
			get { return resolver; }
		}

		internal void AddCompiledMode (string name, SimpleMode m)
		{
			compiledModes.Add (m.Name, m);
		}

		internal void AddCompiledMode (NvdlModeUsage u, SimpleMode m)
		{
			compiledModes.Add (u, m);
		}

		internal void AddCompiledMode (NvdlContext c, SimpleMode m)
		{
			compiledModes.Add (c, m);
		}

		internal SimpleMode GetCompiledMode (string name)
		{
			return compiledModes [name] as SimpleMode;
		}

		internal SimpleMode GetCompiledMode (NvdlModeUsage u)
		{
			return compiledModes [u] as SimpleMode;
		}

		internal SimpleMode GetCompiledMode (NvdlContext c)
		{
			return compiledModes [c] as SimpleMode;
		}

		internal ICollection GetCompiledModes ()
		{
			return compiledModes.Values;
		}

		internal NvdlModeCompileContext GetModeContext (SimpleMode m)
		{
			return modeContexts [m] as NvdlModeCompileContext;
		}

		internal void AddModeContext (SimpleMode m,
			NvdlModeCompileContext mctx)
		{
			modeContexts.Add (m, mctx);
		}

		internal NvdlRule GetRuleContext (SimpleRule r)
		{
			return ruleContexts [r] as NvdlRule;
		}

		internal void AddRuleContext (SimpleRule r, NvdlRule rctx)
		{
			ruleContexts.Add (r, rctx);
		}

		public Hashtable CancelledRules {
			get { return cancelledRules; }
		}
	}

	internal class NvdlModeCompileContext
	{
		ArrayList included;

		public NvdlModeCompileContext (NvdlModeBase mode)
		{
			included = new ArrayList ();
		}

		public ArrayList Included {
			get { return included; }
		}
	}
}
