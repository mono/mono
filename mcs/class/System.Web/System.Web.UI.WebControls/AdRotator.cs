/**
 * Namespace: System.Web.UI.WebControls
 * Class:     AdRotator
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  10??%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace System.Web.UI.WebControls
{
	public class AdRotator: WebControl
	{

		private string advertisementFile;
		private string keywordFilter;
		private string target;
		private static readonly object AdCreatedEvent = new object();

		// Will be set values during (On)PreRender-ing
		private string alternateText;
		private string imageUrl;
		private string navigateUrl;

		private string fileDirctory;

		private static class AdRecord
		{
			public IDictionary adProps;
			public int         hits; // or impressions or clicks
			public string      keyword;

			public AdRecord()
			{
			}
			
			public void Initialize(IDictionary adProps)
			{
				this.adProps = adProps;
			}
		}

/*
 * Loading / Saving data from/to ad file and all the manipulations wrt to the URL...
 * are incorporated by the following functions.
 * GetData(string)
 * LoadAdFile(string)
 * IsAdMatching(AdRecord)
 * ResolveAdUrl(string)
 * SelectAd()
 * The exact control flow will be detailed. Let me first write the functions
 */

		private AdRecord[] LoadAdFile(string file)
		{
			//TODO: Implement me
			Stream      fSream;
			ArrayList   list;
			XmlReader   reader;
			XmlDocument document;
			XmlNode     topNode, innerNode;
			IDictionary hybridDict;
			AdRecord[]  adsArray = null;
			try
			{
				fStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			} catch(Exception e)
			{
				throw new HttpException("AdRotator: Unable to open file");
			}
			try
			{
				list = new ArrayList();
				reader = new XmlTextReader(fStream);
				document = new XmlDocument();
				document.Load(reader);
				if(document.DocumentElement!=null)
				{
					if(docuent.DocumentElement.LocalName=="Advertisements")
					{
						topNode = documentElement.FirstChild;
						while(topNode!=null)
						{
							if(topNode.LocalName=="Ad")
							{
								innerNode = topNode.FirstChild;
								while(innerNode!=null)
								{
									if(innerNode.NodeType==NodeType.Element)
									{
										if(hybridDic==null)
										{
											hybirdDict = new HybridDictionary();
										}
										hybridDic.Add(innerNode.LocalName, innerNode.InnerText);
									}
									innerNode = innerNode.NextSibling;
								}
								if(hybridDict!=null)
									list.Add(hybridDict);
							}
							topNode = topNode.NextSibling;
						}
					}
				}
				if(list.Count>0)
				{
					adsArray = new AdRecord[list.Count];
					for(int i=0; i < list.Count; i++)
					{
						adsArray[i] = new AdRecord((IDictionary)list.Item[i]);
					}
				}
			} catch(Excetion e)
			{
				//TODO: Write error. Parsing error has occured
				throw new HttpException("AdRotator: Unable to parse file" + file);
			} finally
			{
				fStream.close();
			}
			if(adsArray == null)
			{
				throw new HttpException("AdRotator: No Advertisements Fount");
			}
			return adsArray;
		}
		
		private AdRecord[] GetData(string file)
		{
			//TODO: Implement me
			fileDirectory = TemplateSourceDirectory + MapPathSecure(file);
			//TODO: Do I need to check caching?
		}
		
		private IDictionary SelectAd()
		{
			//TODO: Implement Me
		}
		
		private bool IsAdMatching(AdRecord currAd)
		{
			//TODO: Implement me
		}
		
		private string ResolveAdUrl(string)
		{
			//TODO: Implement me
		}
		
		public event AdCreatedEventHandler AdCreated
		{
			add
			{
				Events.AddHandler(AdCreatedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(AdCreatedEvent, value);
			}
		}
		
		public AdRotator()
		{
			base();
			advertisementFile = string.Empty;
			keywordFilter     = string.Empty;
			target            = string.Empty;
		}
		
		public string AdvertisementFile
		{
			get
			{
				return advertisementFile;
			}
			set
			{
				advertisementFile = value;
			}
		}
		
		public string KeywordFilter
		{
			get
			{
				return keywordFilter;
			}
			set
			{
				keywordFilter = value;
			}
		}
		
		public string Target
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
			}
		}
		
		protected override ControlCollection CreateControlCollection()
		{
			return new EmptyControlCollection(this);
		}
		
		protected virtual void OnAdCreated(AdCreatedEventArgs e)
		{
			if(Events!=null)
			{
				AdCreatedEventHandler aceh = (AdCreatedEventHandler)(Events[AdCreatedEvent]);
				if(aceh!=null)
					aceh(this, e);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			if(AdvertisementFile!=String.Empty)
			{
				AdCreatedEventArgs acea = new AdCreatedEventArgs(SelectAd());
				imageUrl      = acea.ImageUrl;
				navigateUrl   = acea.NavigateUrl;
				alternateText = acea.AlternateText;
			}
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			HyperLink hLink = new HyperLink();
			Image     image;

			AttributeCollection attributeColl = base.Attributes;
			ICollection keys = attributeColl.Keys;
			IEnumerator iterator = keys.GetEnumerator();
			
			
		}
	}
}
