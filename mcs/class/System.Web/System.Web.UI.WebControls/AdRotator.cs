/**
 * Namespace: System.Web.UI.WebControls
 * Class:     AdRotator
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Xml;
using System.Web.Utils;

namespace System.Web.UI.WebControls
{
	public class AdRotator: WebControl
	{

		private string advertisementFile;
		private static readonly object AdCreatedEvent = new object();

		// Will be set values during (On)PreRender-ing
		private string alternateText;
		private string imageUrl;
		private string navigateUrl;

		private string fileDirectory;

		private class AdRecord
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
				throw new HttpException("AdRotator_Parse_Error" + file);
			} finally
			{
				fStream.close();
			}
			if(adsArray == null)
			{
				throw new HttpException("AdRotator_No_Advertisements_Found");
			}
			return adsArray;
		}
		
		private AdRecord[] GetData(string file)
		{
			string physPath = MapPathSecure(file);
			string AdKey = "AdRotatorCache: " + physPath;
			fileDirectory = UrlUtils.GetDirectory(UrlUtils.Combine(TemplateSourceDirectory, file));
			CacheInternal ci = HttpRuntime.CacheInternal;
			AdRecord[] records = (AdRecord[])ci[AdKey];
			if(!(records))
			{
				records = LoadAdFile(physPath);
				if(!(records))
				{
					return null;
				}
				ci.Insert(AdKey, records, new CacheDependency(physPath));
			}
			return records;
		}
		
		private IDictionary SelectAd()
		{
			AdRecord[] records = GetFileData(AdvertisementFile);
			if(records!=null && records.Length!=0)
			{
				int impressions = 0;
				for(int i=0 ; i < records.Length; i++)
				{
					if(IsAdMatching(records[i]))
						impressions += records[1].hits;
				}
				if(impressions!=0)
				{
					int rnd = Random.Next(impressions) + 1;
					int counter = 0;
					int index = 0;
					for(int i=0; i < records.Length; i++)
					{
						if(IsAdMaching(records[i]))
						{
							if(rnd <= (counter + records[i].hits))
							{
								index = i;
								break;
							}
							counter += records[i].hits;
						}
					}
					return records[index].adProps;
				}
			}
			return null;
		}
		
		private bool IsAdMatching(AdRecord currAd)
		{
			if(KeywordFilter!=String.Empty)
			{
				if(currAd.keyword.ToLower() == KeywordFilter.ToLower())
					return false;
			}
			return true;
		}
		
		private string ResolveAdUrl(string relativeUrl)
		{
			if(relativeUrl.Length==0 || !UrlUtils.IsRelativeUrl(relativeUrl))
				return relativeUrl;
			string fullUrl = String.Empty;
			if(fileDirectory != null)
				fullUrl = fileDirectory;
			if(fullUrl.Length == 0)
				fullUrl = TemplateSourceDirectory;
			if(fullUrl.Length == 0)
				return relativeUrl;
			return (fullUrl + relativeUrl);
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
			fileDirectory     = null;
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
				object o = ViewState["KeywordFilter"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				if(value!=null)
					ViewState["KeywordFilter"] = value.Trim();
			}
		}

		public string Target
		{
			get
			{
				object o = ViewState["Target"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Target"] = value;
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
			Image adImage = new Image();
			foreach(IEnumerable current in Attributes.Keys)
			{
				hLink[(string)current] = Attributes[(string)current];
			}
			if(ID != null && ID.Length > 0)
				hLink.ID = ID;
			hLink.Target    = Target;
			hLink.AccessKey = AccessKey;
			hLink.Enabled   = Enabled;
			hLink.TabIndex  = TabIndex;
			hLink.RenderBeginTag(writer);
			if(ControlStyleCreated)
			{
				adImage.ApplyStyle(ControlStyle);
			}
			if(imageUrl!=null && imageUrl.Length > 0)
				adImage.ImageUrl = ResolveAdUrl(imageUrl);
			adImage.AlternateText = alternateText;
			adImage.ToolTip       = ToolTip;
			adImage.RenderControl(writer);
			hLink.RenderEndTag(writer);
		}
	}
}
