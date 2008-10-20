#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at xvision.com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace System.Web.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using System.IO;

	internal class CapabilitiesResult : System.Web.HttpBrowserCapabilities
	{
		static string[] RandomRoboBotKeywords;

		/// <summary>
		/// Initializes a new instance of the Result class.
		/// </summary>
		/// <param name="items">
		/// This is the data which this class will be handle request made though this class.
		/// </param>
		internal CapabilitiesResult(System.Collections.IDictionary items)
			: base()
		{
			base.Capabilities = items;
			Capabilities ["browsers"] = new ArrayList ();
		}

		static CapabilitiesResult () {
			//---------------------------------------------------------------
			//Copies out a list of keywords stored in an Embeded file, which 
			//will be used to help determine if a browser is 
			//IsRandomRoboBotUserAgent.
			//---------------------------------------------------------------
			Assembly asm = Assembly.GetExecutingAssembly();
			Stream CP = asm.GetManifestResourceStream("RandomRoboBotKeywords.txt");
			using (StreamReader Read = new StreamReader(CP, System.Text.Encoding.Default)) {
				RandomRoboBotKeywords = System.Text.RegularExpressions.Regex.Split(Read.ReadToEnd(), System.Environment.NewLine);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		internal void AddCapabilities(string name, string value)
		{
			this.Capabilities[name] = value;
		}

		internal void AddMatchingBrowserId (string id)
		{
			ArrayList al = Capabilities ["browsers"] as ArrayList;
			if (al != null && !al.Contains (id))
				al.Add (id);
		}
		
		internal virtual string Replace(string item)
		{
			if (item.IndexOf("$") > -1)
			{
				//nasty hack to convert regular expression replacement text into  Capability item
				//which we can use to replace with the actual values they are looking for.
				System.Text.RegularExpressions.MatchCollection regxmatch;
				regxmatch = System.Text.RegularExpressions.Regex.Matches(item, @"\$\{(?'Capability'\w*)\}");
				if (regxmatch.Count == 0)
				{
					return item;
				}
				for (int i = 0;i <= regxmatch.Count - 1;i++)
				{
					if (regxmatch[i].Success == true)
					{
						string c = regxmatch[i].Result("${Capability}");
						item = item.Replace("${" + c + "}", this[c]);
					}
				}
			}
			if (item.IndexOf("%") > -1)
			{
				//nasty hack to convert regular expression replacement text into  Capability item
				//which we can use to replace with the actual values they are looking for.
				System.Text.RegularExpressions.MatchCollection regxmatch;
				regxmatch = System.Text.RegularExpressions.Regex.Matches(item, @"\%\{(?'Capability'\w*)\}");
				if (regxmatch.Count == 0)
				{
					return item;
				}
				for (int i = 0;i <= regxmatch.Count - 1;i++)
				{
					if (regxmatch[i].Success == true)
					{
						string c = regxmatch[i].Result("${Capability}");
						item = item.Replace("%{" + c + "}", this[c]);
					}
				}
			}
			return item;
		}
		/// <summary>
		/// Gets the Operating System that the browser is running on.
		/// </summary>
		public string OS
		{
			get
			{
				return this["os"];
			}
		}
		/// <summary>
		/// Gets the browsers Build.
		/// </summary>
		public string BrowserBuild
		{
			get
			{
				return this["BrowserBuild"];
			}
		}
		/// <summary>
		/// Name of the Browser Rendering Engine, when known.
		/// </summary>
		public string BrowserRenderingEngine
		{
			get
			{
				return this["HtmlEngine"];
			}
		}
		/// <summary>
		/// Gets if the Browser was identified as a bot, as a mater of elimination of all other possible
		/// options currently availible.
		/// </summary>
		public bool IsRobot
		{
			get
			{
				if (string.Compare(this["IsMobileDevice"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return false;
				}
				else if (string.Compare(this["IsBot"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				else if (string.Compare(this["crawler"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				else if (string.Compare(this["Unknown"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				else if (string.Compare(this.Browser, "Unknown", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				else if (string.Compare(this.Browser, "IE", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					//too many fake IE's out there this should remove a few of the low
					//hanging fruit.
					if (string.IsNullOrEmpty(this.Platform) == true)
					{
						return true;
					}
					else if (string.Compare(this.Platform, "Unknown", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
					{
						return true;
					}
					else if (string.Compare(this[""], "....../1.0", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
					{
						//I hate Scrapters This one hit me today. Lets see how it like it now geting 403's
						return true;
					}

				}
				return false;
			}
		}
		public bool IsSyndicationReader
		{
			get
			{
				if (string.Compare(this["IsSyndicationReader"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				return false;
			}
		}
		public bool IsUnknown
		{
			get
			{
				if (string.Compare(this["Unknown"], "true", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
				return false;
			}
		}
		/// <summary>
		/// Used to Identify Robobots that are using randomly generated Useragents
		/// that are nonsensical in nature/gibberish.
		/// </summary>
		/// <remarks>
		/// Current implementation is more of an elimination of common traits, which
		/// most Useragent/browser have. Which leave us with what can be assumed as
		/// randomized useragent names, which serve no purpose cept to drive stats
		/// programs nuts.
		/// </remarks>
		public bool IsRandomRobobotUserAgent
		{
			get
			{
				#region  Check for Common Words in UserAgents
				//---------------------------------------------------------------
				//Quick Checks to see if the Bot has been identified by a name
				//from the headers provided.
				//---------------------------------------------------------------
				if (this.IsRobot == false)
				{
					//---------------------------------------------------------------
					//Since we can determine its not a Robot. We must have enough
					//details to prove its not a random useragent, and we move on.
					//---------------------------------------------------------------
					return false;
				}
				else if (this.IsSyndicationReader == true)
				{
					//---------------------------------------------------------------
					//Since we can determine its not a Rss/Atom Feed Reader. We must 
					//have enough details to prove its not a random useragent, and we
					//move on.
					//---------------------------------------------------------------
					return false;
				}
				else if (string.Compare(this.Browser, "Unknown", true, System.Globalization.CultureInfo.CurrentCulture) != 0)
				{
					//---------------------------------------------------------------
					//Browser name was able to be determined then the Useragent had
					//enough details, thus not a random Useragent.
					//---------------------------------------------------------------
					return false;
				}
				else if (string.Compare(this.Platform, "Unknown", true, System.Globalization.CultureInfo.CurrentCulture) != 0)
				{
					//---------------------------------------------------------------
					//Assume if a platform was able to be determine then the Useragent
					//is more then likely not randomized name.
					//---------------------------------------------------------------
					return false;
				}
				else if (string.IsNullOrEmpty(this.UserAgent) == true)
				{
					//---------------------------------------------------------------
					//Null or empty. ^he Programer was just to lazy which to give it a
					//name, which is fine with me but doesn't not count as a Randomized
					//Browser Agent, since it doesn't have a Useragent at all to begin
					//with.
					//---------------------------------------------------------------
					return false;
				}

				//---------------------------------------------------------------
				//I assume ones under 8 charactors are not really randomly named
				//but the coder was just lazy or picked a short name.
				//---------------------------------------------------------------
				if (this.UserAgent.Length < 8)
				{
					return false;
				}

				//---------------------------------------------------------------
				//Up to this point I have not seen a randomly generated Agent string
				//with a period in it.
				//---------------------------------------------------------------
				if (this.UserAgent.IndexOf('.') > -1)
				{
					return false;
				}
				//---------------------------------------------------------------
				//Compare keywords often found in useragents to the current useragent
				//and if we find one we assume its not a randomized useragent.
				//---------------------------------------------------------------
				foreach (string keyword in RandomRoboBotKeywords)
				{
					if (keyword.Length <= this.UserAgent.Length)
					{
						if (this.UserAgent.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							return false;
						}
					}
				}
				#endregion
				//---------------------------------------------------------------
				//Since it made it though all the checks I assume that the useragent
				//doesn't match any known format that I can determine, and label it
				//a randomized Useragent/browser. AKA SPAM / Scraper / Pests Bots.
				//---------------------------------------------------------------
				return true;
			}
		}
		/// <summary>
		/// Gets the keys returned from processing.
		/// </summary>
		public System.Collections.Specialized.StringCollection Keys
		{
			get
			{
				string[] a = new string[this.Capabilities.Keys.Count];
				this.Capabilities.Keys.CopyTo(a, 0);
				System.Array.Sort(a);
				System.Collections.Specialized.StringCollection l;
				l = new System.Collections.Specialized.StringCollection();
				l.AddRange(a);
				return l;
			}
		}
		public string UserAgent
		{
			get
			{
				return this[""];
			}
		}
	}
}
#endif
