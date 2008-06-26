using System;
using System.Configuration;

namespace ConsoleAppConfigSection
{


	public class UrlsSection : ConfigurationSection
	{
		[ConfigurationProperty ("urls", IsDefaultCollection = false)]
		[ConfigurationCollection (typeof (RootUrlsCollection))]
		public RootUrlsCollection Urls {
			get { return (RootUrlsCollection) base ["urls"]; }
			internal set { base ["urls"] = value; }
		}
	}

	public class UrlsCollection : ConfigurationElementCollection
	{
		public UrlsCollection () {
		}

		protected override ConfigurationElement CreateNewElement () {
			//Console.WriteLine (Environment.StackTrace);
			return new UrlConfigElement ();
		}

		protected override Object GetElementKey (ConfigurationElement element) {
			return ((UrlConfigElement) element).Name;
		}

		public UrlConfigElement this [int index] {
			get {
				return (UrlConfigElement) BaseGet (index);
			}
			set {
				if (BaseGet (index) != null) {
					BaseRemoveAt (index);
				}
				BaseAdd (index, value);
			}
		}
	}

	public class RootUrlsCollection : UrlsCollection
	{
		public RootUrlsCollection () {
		}

		[ConfigurationProperty ("", IsDefaultCollection = true, IsKey= true)]
		
		public GroupCollection GroupSettings {
			get { return (GroupCollection) base [String.Empty]; }
			internal set { base [String.Empty] = value; }
		}
	}

	// Define the UrlsCollection that will contain the UrlsConfigElement
	// elements.
	[ConfigurationCollection (typeof (GroupCollection), AddItemName = "group")]
	public class GroupCollection : ConfigurationElementCollection
	{

		protected override ConfigurationElement CreateNewElement () {
			//Console.WriteLine (Environment.StackTrace);
			return new GroupElement ();
		}

		protected override Object GetElementKey (ConfigurationElement element) {
			return ((GroupElement) element).Name;
		}

		public GroupElement this [int index] {
			get {
				return (GroupElement) BaseGet (index);
			}
			set {
				if (BaseGet (index) != null) {
					BaseRemoveAt (index);
				}
				BaseAdd (index, value);
			}
		}
	}

	public class GroupElement : ConfigurationElement
	{
		[ConfigurationProperty ("name", IsRequired = true, IsKey = true)]
		public string Name {
			get { return (string) base ["name"]; }
			internal set { base ["name"] = value; }
		}

		[ConfigurationProperty ("", IsDefaultCollection = true, IsRequired = true)]
		[ConfigurationCollection (typeof (UrlsCollection))]
		public UrlsCollection PropertySettings {
			get { return (UrlsCollection) base [String.Empty]; }
			internal set { base [String.Empty] = value; }
		}
	}

	// Define the UrlConfigElement for the types contained by the 
	// UrlsSection.
	public class UrlConfigElement : ConfigurationElement
	{
		public UrlConfigElement (String name, String url) {
			this.Name = name;
			this.Url = url;
		}

		public UrlConfigElement () {
			// Initialize as follows, if no attributed 
			// values are provided.
			// this.Name = "Microsoft";
			// this.Url = "http://www.microsoft.com";
			// this.Port = 0;
		}

		[ConfigurationProperty ("name1", DefaultValue = "Microsoft",
			IsRequired = true, IsKey = true)]
		public string Name {
			get {
				return (string) this ["name1"];
			}
			set {
				this ["name1"] = value;
			}
		}

		[ConfigurationProperty ("url", DefaultValue = "http://www.microsoft.com",
			IsRequired = true)]
		[RegexStringValidator (@"\w+:\/\/[\w.]+\S*")]
		public string Url {
			get {
				return (string) this ["url"];
			}
			set {
				this ["url"] = value;
			}
		}

		[ConfigurationProperty ("port", DefaultValue = (int) 0, IsRequired = false)]
		[IntegerValidator (MinValue = 0, MaxValue = 8080, ExcludeRange = false)]
		public int Port {
			get {
				return (int) this ["port"];
			}
			set {
				this ["port"] = value;
			}
		}
	}

	class TestingConfigurationCollectionAttribute
	{
		static void PrintUrls (RootUrlsCollection col) {
			for (int i = 0; i < col.Count; i++) {
				ConfigurationElement e = col [i];
				UrlConfigElement ue = e as UrlConfigElement;
				if (ue != null) {
					Console.WriteLine ("  #{0} {1}: {2}", i,
						ue.Name,
						ue.Url + " port " +
						ue.Port);
				}
			}
		}

		static void PrintUrls (UrlsCollection col) {
			for (int i = 0; i < col.Count; i++) {
				ConfigurationElement e = col [i];
				UrlConfigElement ue = e as UrlConfigElement;
				if (ue != null) {
					Console.WriteLine ("  #{0} {1}: {2}", i,
						ue.Name,
						ue.Url + " port " +
						ue.Port);
				}
			}
		}

		static void PrintUrls (GroupElement col) {
			Console.WriteLine ("Group name: " + col.Name);
			PrintUrls (col.PropertySettings);
		}

		static void PrintUrls (GroupCollection col) {
			for (int i = 0; i < col.Count; i++) {
				ConfigurationElement e = col [i];
				GroupElement ue = e as GroupElement;
				if (ue != null) {
					PrintUrls (ue);
				}
			}
		}

		static void ShowUrls () {

			try {
				UrlsSection myUrlsSection =
				   ConfigurationManager.GetSection ("MyUrls") as UrlsSection;

				if (myUrlsSection == null)
					Console.WriteLine ("Failed to load UrlsSection.");
				else {
					Console.WriteLine ("My URLs:");
					PrintUrls (myUrlsSection.Urls);
					PrintUrls (myUrlsSection.Urls.GroupSettings);
				}
			}
			catch (Exception e) {
				Console.WriteLine (e.ToString ());
			}
		}

		// Create a custom section.
		// It will contain a nested section as 
		// deined by the UrlsSection (<urls>...</urls>).
		static void CreateSection (string sectionName) {
			// Get the current configuration (file).
			System.Configuration.Configuration config =
					ConfigurationManager.OpenExeConfiguration (
					ConfigurationUserLevel.None);

			UrlsSection urlsSection;

			// Create an entry in the <configSections>. 
			if (config.Sections [sectionName] == null) {
				urlsSection = new UrlsSection ();
				config.Sections.Add (sectionName, urlsSection);
				config.Save ();
			}

			// Create the actual target section and write it to 
			// the configuration file.
			if (config.Sections ["/configuration/" + sectionName] == null) {
				urlsSection = config.GetSection (sectionName) as UrlsSection;
				urlsSection.SectionInformation.ForceSave = true;
				config.Save (ConfigurationSaveMode.Full);
			}
		}


		static void Main (string [] args) {
			Console.WriteLine ("[Current URLs]");
			ShowUrls ();
		}
	}
}