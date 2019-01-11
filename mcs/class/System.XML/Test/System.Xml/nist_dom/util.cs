//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//
//
// Ported to System.Xml by: Mizrahi Rafael rafim@mainsoft.com
// Mainsoft Corporation (c) 2003-2004
//
//**************************************************************************

using System;
using System.Xml;

using MonoTests.Helpers;

namespace nist_dom
{
        public class XmlNodeArrayList : XmlNodeList
        {
            System.Collections.ArrayList _rgNodes;

            public XmlNodeArrayList (System.Collections.ArrayList rgNodes)
            {
                _rgNodes = rgNodes;
            }

            public override int Count { get { return _rgNodes.Count; } }

            public override System.Collections.IEnumerator GetEnumerator ()
            {
                return _rgNodes.GetEnumerator ();
            }

            public override XmlNode Item (int index)
            {
                // Return null if index is out of range. by  DOM design.
                if (index < 0 || _rgNodes.Count <= index)
                    return null;

                return (XmlNode) _rgNodes [index];
            }
        }

    public class testResults
	{
		public string name;
		public string description;
		public string expected;
		public string actual;
		public testResults(string name)
		{
			//      get the name of the calling function
			this.name     = name;
			//
			//      define some methods
			//
			//this.description = set_description();
			//this.expected = set_expected();
			//this.actual = set_actual();
			//        this.toString = set_toString;
		}
		public void set_description(string description) { this.description = description; }
		public void set_expected (string expected) { this.expected = expected;}
		public void set_actual (string actual) { this.actual = actual; }

		public void set_toString()
		{
			System.Console.WriteLine("name = "+this.name+"<br>");
			System.Console.WriteLine("Description = "+this.description+"<br>");
			System.Console.WriteLine("Expected Value = "+this.expected+"<br>");
			System.Console.WriteLine("Actual Value = "+this.actual+"<br>");
		}
	}

	/// <summary>
	/// Summary description for util.
	/// </summary>
	public class util
	{
		public static System.Xml.XmlDocument masterDoc = new System.Xml.XmlDocument();//"files/staff.xml"
		public static System.Xml.XmlDocument originalDoc = new System.Xml.XmlDocument();//"files/staff.xml"
		public static System.Xml.XmlDocument masterXML = new System.Xml.XmlDocument();//"files/staff.html"
		public static System.Xml.XmlDocument otherDoc = new System.Xml.XmlDocument();//"files/otherDoc.xml"
		public static System.Xml.XmlDocument HTMLDoc = new System.Xml.XmlDocument();//"files/staff.html"
		public static System.Xml.XmlDocument noDTDXMLObject = new System.Xml.XmlDocument();//"files/noDTDXMLfile.xml"

//		cWin = self.parent.viewer;
		//public static System.Xml.XmlDocument vdoc;			// = cWin.document;
        public static System.IO.StreamWriter vdoc = null;

//		dWin = self.parent.test;
		public static System.Xml.XmlDocument testdoc;		// = dWin.document;

//		fWin = self.parent.frameddoc;
		public static System.Xml.XmlDocument framesetdoc;	// = fWin.document;

//		oWin = self.parent.original;
		public static System.Xml.XmlDocument originaldoc;	// = oWin.document;
//
//		var Interfaces = new Object();
//
//		iform = parent.interfaces.selectInterface;
//		testInterface = iform.interface.options[iform.interface.selectedIndex].Value;
//		cform = parent.categories.selectCategory;
//		testCategory = cform.category.options[cform.category.selectedIndex].Value;
//
		public static string passColor = "#00FFFF";
		public static string failColor = "#FF0000";

//		public static System.Xml.XmlNode _node;
//        public static System.Xml.XmlAttributeCollection _attributes;
//        public static System.Xml.XmlNodeList _subNodes;

		static util()
		{
			try
			{
				//System.Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
				masterDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/staff.xml"));
				originalDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/staff.xml"));
				masterXML.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/staff.html"));
				otherDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/otherDoc.xml"));
				HTMLDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/staff.html"));
				noDTDXMLObject.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/nist_dom/files/noDTDXMLfile.xml"));
			}
			catch (System.Exception ex)
			{
				System.Console.WriteLine(ex.Message);
				System.Console.WriteLine(ex.StackTrace);
			}

		}

		// ***********************************************************************
		//     SUPPORTING ROUTINES AND DEFINITIONS
		//************************************************************************

        public static string SUCCESS = "Passed";
        public static string FAILED = "Failed";

		//
		// General defs.
		//
		public static string saved = "";
		public static int MAXEMPLOYEES = 5;
		public static int NODE = 1;
		public static int INDEXING = 2;
		public static string employee = "employee";
		public static string rootNode = "staff";
		public static string pass = "Passed";
		public static string fail = "Failed";
		public static int FIRST = 0;
		public static int SECOND = 1;
		public static int THIRD = 2;
		public static int FOURTH = 3;
		public static int FIFTH = 4;
		public static int SIXTH = 5;
		public static int SEVENTH = 6;
		public static int EIGHT = 7;
		public static int NINETH = 8;
		public static int TENTH = 9;
		public static int ELEVENTH = 10;
		public static int TWELVETH = 11;
		public static int THIRDTEENTH = 12;

		public static string BODY = "BODY";
		public static string TITLE = "TITLE";
		public static string H1 = "H1";
		public static string H2 = "H2";
		public static string H3 = "H3";
		public static string H4 = "H4";
		public static string H5 = "H5";
		public static string H6 = "H6";
		public static string BR = "BR";
		public static string TABLE = "TABLE";
		public static string IMG = "IMG";
		public static string OBJECT = "OBJECT";
		public static string FONT = "FONT";
		public static string BASEFONT = "BASEFONT";
		public static string MAP = "MAP";
		public static string AREA = "AREA";
		public static string P = "P";
		public static string OL = "OL";
		public static string UL = "UL";
		public static string DL = "DL";
		public static string HR = "HR";
		public static string HTML = "HTML";
		public static string HEAD = "HEAD";
		public static string LINK = "LINK";
		public static string META = "META";
		public static string BASE = "BASE";
		public static string FORM = "FORM";
		public static string SELECT = "SELECT";
		public static string OPTION = "OPTION";
		public static string Q = "Q";
		public static string BLOCKQUOTE = "BLOCKQUOTE";
		public static string COL = "COL";
		public static string COLGROUP = "COLGROUP";
		public static string PRE = "PRE";
		public static string DIV = "DIV";
		public static string INS = "INS";
		public static string DEL = "DEL";
		public static string DIR = "DIR";
		public static string APPLET = "APPLET";
		public static string OPTGROUP = "OPTGROUP";
		public static string SCRIPT = "SCRIPT";
		public static string PARAM = "PARAM";
		public static string ISINDEX = "ISINDEX";
		public static string INPUT = "INPUT";
		public static string BUTTON = "BUTTON";
		public static string LABEL = "LABEL";
		public static string TEXTAREA = "TEXTAREA";
		public static string FIELDSET = "FIELDSET";
		public static string ANCHOR = "A";
		public static string FRAMESET = "FRAMESET";
		public static string FRAME = "FRAME";
		public static string IFRAME = "IFRAME";
		public static string LEGEND = "LEGEND";
		public static string MENU = "MENU";
		//public static string IFRAME = "IFRAME";
		public static string STYLE = "STYLE";
		public static string LI = "LI";
		public static string SUB = "SUB";
		public static string SUP = "SUP";
		public static string SPAN = "SPAN";
		public static string BDO = "BDO";
		public static string TT = "TT";
		public static string I = "I";
		public static string B = "B";
		public static string U = "U";
		public static string S = "S";
		public static string STRIKE = "STRIKE";
		public static string BIG = "BIG";
		public static string SMALL = "SMALL";
		public static string EM = "EM";
		public static string STRONG = "STRONG";
		public static string DFN = "DFN";
		public static string CODE = "CODE";
		public static string SAMP = "SAMP";
		public static string KBD = "KBD";
		public static string VAR = "VAR";
		public static string CITE = "CITE";
		public static string ACRONYM = "ACRONYM";
		public static string ABBR = "ABBR";
		public static string DT = "DT";
		public static string DD = "DD";
		public static string NOFRAMES = "NOFRAMES";
		public static string NOSCRIPT = "NOSCRIPT";
		public static string ADDRESS = "ADDRESS";
		public static string CENTER = "CENTER";
		public static string TD = "TD";
		public static string TBODY = "TBODY";
		public static string TFOOT = "TFOOT";
		public static string THEAD = "THEAD";
		public static string TR = "TR";
		public static string TH = "TH";

		//
		// Base URL's for tests depending on specific URL's  
		//
		// BASE1 - The URI returned by the "referrer" attribute (Document interface).
		// BASE2 - The string returned by the "domain" attribute (Document interface).
		// BASE3 - The URI returned by the "URL" attribute (Document interface).
		// BASE4 - The URI returned by the "codebase" attribute (Applet interface). 
		// BASE5 - ThE URI returned by the "codeBase" attribute (Object interface). 
		// BASE6 - The URI returned by the "href" attribute (Base interface).
		//

		public static string BASE1 = "HTTP://XW2K.SDCT.ITL.NIST.GOV/BRADY/DOM/INDEX.HTML"; 
		public static string BASE2 = "XW2K.SDCT.ITL.NIST.GOV";
		public static string BASE3 = "HTTP://XW2K.SDCT.ITL.NIST.GOV/BRADY/DOM/FILES/TEST.HTML";
		public static string BASE4 = "HTTP://XW2K.SDCT.ITL.NIST.GOV/BRADY/DOM/FILES/";
		public static string BASE5 = "HTTP://XW2K.SDCT.ITL.NIST.GOV/BRADY/DOM/";
		public static string BASE6 = "HTTP://XW2K.SDCT.ITL.NIST.GOV/BRADY/DOM/";

		//
		// Exception codes
		//

		public static string EOL = "\n"; //String.fromCharCode(13,10);
		public static string INDEX_SIZE_ERR = "StartIndex cannot be less than zero.\r\nParameter name: startIndex";
		public static string DOMSTRING_SIZE_ERR = " ";
		public static string INVALID_CHARACTER_ERR  = "A name contained an invalid character." + EOL;
		public static string NOT_DATA_ALLOWED_ERR = " ";
		public static string NO_MODIFICATION_ALLOWED_ERR = "Attempt to modify a read-only node." + EOL;
		public static string NOT_FOUND1_ERR = "Invalid procedure call or argument";
		public static string NOT_FOUND2_ERR = "Insert position Node must be a Child of the Node to " + "insert under." + EOL;
		public static string NOT_FOUND3_ERR = "The parameter Node is not a child of this Node." + EOL;
		public static string NOT_SUPPORTED_ERR = " ";
		public static string INUSE_ATTRIBUTE_ERR = "The Attribute node cannot be inserted because it is already an attribute of another element.";//"Attributes must be removed before adding them " + "to a different node." + EOL;

		//
		// nodeType values
		//

		public const int ELEMENT_NODE = (int)System.Xml.XmlNodeType.Element;
		public const int ATTRIBUTE_NODE = (int)System.Xml.XmlNodeType.Attribute;
		public const int TEXT_NODE = (int)System.Xml.XmlNodeType.Text;
		public const int CDATA_SECTION_NODE = (int)System.Xml.XmlNodeType.CDATA;
		public const int ENTITY_REFERENCE_NODE = (int)System.Xml.XmlNodeType.EntityReference;
		public const int ENTITY_NODE = (int)System.Xml.XmlNodeType.Entity;
		public const int PROCESSING_INSTRUCTION_NODE = (int)System.Xml.XmlNodeType.ProcessingInstruction;
		public const int COMMENT_NODE = (int)System.Xml.XmlNodeType.Comment;
		public const int DOCUMENT_NODE = (int)System.Xml.XmlNodeType.Document;
		public const int DOCUMENT_TYPE_NODE = (int)System.Xml.XmlNodeType.DocumentType;
		public const int DOCUMENT_FRAGMENT_NODE = (int)System.Xml.XmlNodeType.DocumentFragment;
		public const int NOTATION_NODE = (int)System.Xml.XmlNodeType.Notation;
		public const int XML_DECLARATION_NODE = (int)System.Xml.XmlNodeType.XmlDeclaration;


	public static System.Xml.XmlDocument getDOMDocument()
	{
		return masterDoc; 
	}

	public static System.Xml.XmlDocument getHTMLDocument()
	{
		return testdoc;
	}

	public static System.Xml.XmlDocument getFramesetDocument()
	{
		return framesetdoc;
	}

	public static System.Xml.XmlDocument getDOMHTMLDocument()
	{
		return HTMLDoc;
	}

	public static System.Xml.XmlDocument getnoDTDXMLDocument()
	{
		return noDTDXMLObject;
	}

	public static System.Xml.XmlDocument getOriginalDOMDocument()
	{
		return originalDoc;
	}

	public static System.Xml.XmlDocument getOtherDOMDocument()
	{
		return otherDoc;
	}

	public static System.Xml.XmlNode createNode(int type,string data)
	{
	System.Xml.XmlNode node = null;

	switch(type) {
		case ATTRIBUTE_NODE:
			node = getDOMDocument().CreateAttribute(data);
			break;
		case CDATA_SECTION_NODE:
			node = getDOMDocument().CreateCDataSection(data);
			break;
		case ELEMENT_NODE:
			node = getDOMDocument().CreateElement(data);
			break;
		case ENTITY_REFERENCE_NODE:
			node = getDOMDocument().CreateEntityReference(data);
			break;
		case TEXT_NODE:
			node = getDOMDocument().CreateTextNode(data);
			break;
		default:
			break;
	}
	return node;
	}

	public static void resetData()
	{
		try
		{
			if (getDOMDocument().DocumentElement != null)
			{
				getDOMDocument().RemoveChild(getDOMDocument().DocumentElement);
			}

			System.Xml.XmlNode tmpNode = getDOMDocument().ImportNode(getOriginalDOMDocument().DocumentElement,true);
			getDOMDocument().AppendChild(tmpNode);
			//getDOMDocument().AppendChild(getOriginalDOMDocument().DocumentElement.CloneNode(true));
		}
		catch (NotImplementedException ex)
		{
			throw ex;
		}
	}

/*	public static void resetHTMLData()
	{
		System.Xml.XmlNode newdoc = originalHTMLDocument(HTML);
		testdoc = ( System.Xml.XmlDocument) newdoc.CloneNode(true);
	}
*/
	public static System.Xml.XmlElement getRootNode()
	{
		return getDOMDocument().DocumentElement;
	}

/*	public void HTMLNodeObject(string argFirst,int argSecond)
	{
	string tagName = argFirst;//arguments[0];
	//int one = 1;
	//int two = 2;
	System.Xml.XmlNodeList nodeList=null;

	if (tagName==FRAMESET || tagName==FRAME)
		nodeList = framesetdoc.GetElementsByTagName(tagName);
	else
		nodeList = testdoc.GetElementsByTagName(tagName);

	if (argFirst != "") //if (arguments.length == one)
		this.node = nodeList.Item(util.FIRST);
	if (argSecond != -1) //else if (arguments.length == two)
		this.node = nodeList.Item(argSecond);//arguments[SECOND]);
	}

	public System.Xml.XmlNode originalHTMLDocument(string arg)
	{
		string tagName = arg;
		//int one = 1;
		//int two = 2;

		System.Xml.XmlNodeList nodeList = originaldoc.GetElementsByTagName(tagName);
		this.node = nodeList.Item(util.FIRST).CloneNode(true);
		return this.node;
	}
*/

//	public string getTableCaption(object table)
//	{
//		return table.caption;
//	}
//
//	public void getTableHead(object table)
//	{
//		return table.tHead;
//	}
//
//	public void getTableFoot(object table)
//	{
//	return table.tFoot;
//	}

	public static System.Xml.XmlNode nodeObject(int argFirst,int argSecond)//args)
	{
	string tagName = employee;
	System.Xml.XmlNodeList nodeList = null;
    System.Xml.XmlNode _node = null;

	nodeList = getRootNode().GetElementsByTagName(tagName);
	System.Xml.XmlElement parentNode = (System.Xml.XmlElement)nodeList.Item(argFirst);//arguments[FIRST]);

	if (argFirst != -1)//if (arguments.length == one)
		_node = parentNode;
    
    if (argSecond != -1)//else if (arguments.length == two)
        {
            System.Xml.XmlNodeList list = getSubNodes(parentNode);
            _node = getElement(getSubNodes(parentNode), argSecond);//arguments[SECOND]);
            //_node = getElement((System.Xml.XmlNodeList)getSubNodes(parentNode), argSecond);//arguments[SECOND]);
        }

        //_attributes = getAttributes(_node);
	    //_subNodes = getSubNodes((System.Xml.XmlElement)_node);

        return _node;
	}
	public static System.Xml.XmlNodeList getSubNodes(System.Xml.XmlDocument node)
	{
// GHT alternative for GetElementsByTagName("*")
//        System.Collections.ArrayList nodeArrayList = new System.Collections.ArrayList ();
//        getAllNodesRecursively (node, nodeArrayList);
//        return new XmlNodeArrayList (nodeArrayList);
// GHT alternative for GetElementsByTagName("*")
	    return node.GetElementsByTagName("*");
	}
    public static System.Xml.XmlNodeList getSubNodes(System.Xml.XmlElement node)
    {
// GHT alternative for GetElementsByTagName("*")
//       System.Collections.ArrayList nodeArrayList = new System.Collections.ArrayList ();
//       getAllNodesRecursively (node, nodeArrayList);
//       return new XmlNodeArrayList (nodeArrayList);
// GHT alternative for GetElementsByTagName("*")
       return node.GetElementsByTagName("*");
    }

    private static void getAllNodesRecursively (XmlNode argNode, System.Collections.ArrayList argArrayList)
    {
        XmlNodeList xmlNodeList = argNode.ChildNodes;
        foreach (XmlNode node in xmlNodeList)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                argArrayList.Add (node);
                getAllNodesRecursively (node, argArrayList);
            }
        }
    }
 

	public static System.Xml.XmlNode getElement(System.Xml.XmlNodeList subNodes,int elementAt)
	{
	    return (System.Xml.XmlNode)subNodes.Item(elementAt);
	}

	public static System.Xml.XmlAttributeCollection  getAttributes(System.Xml.XmlNode node)
	{
		return node.Attributes;
	}

	public static System.Xml.XmlDocumentType getDocType()
	{
		return (System.Xml.XmlDocumentType)getDOMDocument().ChildNodes.Item(SECOND); 
	}

	public static System.Xml.XmlEntity getEntity(string name)
	{
		return (System.Xml.XmlEntity)getDocType().Entities.GetNamedItem(name);
	}

	public static System.Xml.XmlNode getNotation(string name)
	{
		return getDocType().Notations.GetNamedItem(name);
	}

/*
		public void specPtr(index) 
		{
			Spec = new Object();
			Spec['DOMImplementation'] = 'ID-102161490';
			Spec['DocumentFragment'] = 'ID-B63ED1A3';
			Spec['Document'] = 'i-Document';
			Spec['Node'] = 'ID-1950641247';
			Spec['NodeList'] = 'ID-536297177';
			Spec['NamedNodeMap'] = 'ID-1780488922';
			Spec['CharacterData'] = 'ID-FF21A306';
			Spec['Attr'] = 'ID-637646024';
			Spec['Element'] = 'ID-745549614';
			Spec['Text'] = 'ID-1312295772';
			Spec['Comment'] = 'ID-1728279322';
			Spec['CDATASection'] = 'ID-667469212';
			Spec['DocumentType'] = 'ID-412266927';
			Spec['Notation'] = 'ID-5431D1B9';
			Spec['Entity'] = 'ID-527DCFF2';
			Spec['EntityReference'] = 'ID-11C98490';
			Spec['ProcessingInstruction'] = 'ID-1004215813';
			Spec['HTMLCollection'] = 'ID-75708506';
			Spec['HTMLDocument'] = 'ID-26809268';
			Spec['HTMLElement'] = 'ID-58190037';
			Spec['HTMLHtmlElement'] = 'ID-33759296';
			Spec['HTMLHeadElement'] = 'ID-77253168';
			Spec['HTMLLinkElement'] = 'ID-35143001';
			Spec['HTMLTitleElement'] = 'ID-79243169';
			Spec['HTMLMetaElement'] = 'ID-37041454';
			Spec['HTMLBaseElement'] = 'ID-73629039';
			Spec['HTMLIsIndexElement'] = 'ID-85283003';
			Spec['HTMLStyleElement'] = 'ID-16428977';
			Spec['HTMLBodyElement'] = 'ID-62018039';
			Spec['HTMLFormElement'] = 'ID-40002357';
			Spec['HTMLSelectElement'] = 'ID-94282980'; 
			Spec['HTMLOptGroupElement'] = 'ID-38450247';
			Spec['HTMLOptionElement'] = 'ID-70901257';
			Spec['HTMLInputElement'] = 'ID-6043025';
			Spec['HTMLTextAreaElement'] = 'ID-24874179';
			Spec['HTMLButtonElement'] = 'ID-34812697';
			Spec['HTMLLabelElement'] = 'ID-13691394';
			Spec['HTMLFieldSetElement'] = 'ID-7365882';
			Spec['HTMLLegendElement'] = 'ID-21482039';
			Spec['HTMLUListElement'] = 'ID-86834457';
			Spec['HTMLOListElement'] = 'ID-58056027';
			Spec['HTMLDListElement'] = 'ID-52368974';
			Spec['HTMLDirectoryElement'] = 'ID-71600284';
			Spec['HTMLMenuElement'] = 'ID-72509186';
			Spec['HTMLLIElement'] = 'ID-74680021';
			Spec['HTMLBlockquoteElement'] = 'ID-40703765';
			Spec['HTMLDivElement'] = 'ID-22445964';
			Spec['HTMLParagraphElement'] = 'ID-84675076';
			Spec['HTMLHeadingElement'] = 'ID-43345119';
			Spec['HTMLQuoteElement'] = 'ID-70319763';
			Spec['HTMLPreElement'] = 'ID-11383425';
			Spec['HTMLBRElement'] = 'ID-56836063';
			Spec['HTMLBaseFontElement'] = 'ID-32774408';
			Spec['HTMLFontElement'] = 'ID-43943847';
			Spec['HTMLHRElement'] = 'ID-68228811';
			Spec['HTMLModElement'] = 'ID-79359609';
			Spec['HTMLAnchorElement'] = 'ID-48250443';
			Spec['HTMLImageElement'] = 'ID-17701901';
			Spec['HTMLObjectElement'] = 'ID-9893177';
			Spec['HTMLParamElement'] = 'ID-64077273';
			Spec['HTMLAppletElement'] = 'ID-31006348';
			Spec['HTMLMapElement'] = 'ID-94109203';
			Spec['HTMLAreaElement'] = 'ID-26019118';
			Spec['HTMLScriptElement'] = 'ID-81598695';
			Spec['HTMLTableElement'] = 'ID-64060425';
			Spec['HTMLTableCaptionElement'] = 'ID-12035137';
			Spec['HTMLTableColElement'] = 'ID-84150186';
			Spec['HTMLTableSectionElement'] = 'ID-67417573';
			Spec['HTMLTableRowElement'] = 'ID-6986576';
			Spec['HTMLTableCellElement'] = 'ID-82915075';
			Spec['HTMLFrameSetElement'] = 'ID-43829095';
			Spec['HTMLFrameElement'] = 'ID-97790553';
			Spec['HTMLIFrameElement'] = 'ID-50708718';
			return Spec[index];
		}

		public void setInfo() 
		{
			dWin = self.parent.info;
			infodoc = dWin.document;

			iform = parent.interfaces.selectInterface;
			testInterface = iform.interface.options[iform.interface.selectedIndex].Value;

			cform = parent.categories.selectCategory;
			testCategory = cform.category.options[cform.category.selectedIndex].Value;

			sr_file = testCategory.toLowerCase()+"/"+testInterface+"/Requirements.html";
			src_file = testCategory.toLowerCase()+"/"+testInterface+"/"+testInterface+".html";
			util_file = "util.html";
			core_file = "spec/level-one-core.html";
			html_file = "spec/level-one-html.html";

			if (testCategory == "HTML") 
			spec_ref = html_file + "#" + specPtr(testInterface);
			else 
			spec_ref = core_file + "#" + specPtr(testInterface);

			infodoc.write("<BODY BGCOLOR=#0000FF LINK=#FFFF00 VLINK=#FFFF00>\n");
			infodoc.write("<CENTER>\n");
			infodoc.write("<P><P>\n");
			infodoc.write("<b><A HREF="+spec_ref+" TARGET=viewer>DOM REC</A></B><BR>\n");
			infodoc.write("<b><A HREF="+sr_file+" TARGET=viewer>SR's</A></b><BR>\n");
			infodoc.write("<b><A HREF="+src_file+" TARGET=viewer>Source</A></b><BR>\n");
			infodoc.write("<b><A HREF="+util_file+" TARGET=viewer>Utility</A></B><BR>\n");
			infodoc.write("</CENTER>\n");
			infodoc.close();
		}

		public void getInterfaces(option) 
		{
		var Cats = new Object();
		Cats["Fundamental"] = ['DOMImplementation','Node', 'NodeList',
		'Document', 'NamedNodeMap', 'CharacterData', 'Attr',
		'Element', 'Text', 'Comment'];

		Cats["Extended"]  = ['CDATASection', 'DocumentType', 'Notation', 'Entity', 
		'ProcessingInstruction' ];

		Cats["HTML"] = [
		'HTMLAnchorElement',
		'HTMLAppletElement',
		'HTMLAreaElement',
		'HTMLBaseElement',
		'HTMLBaseFontElement',
		'HTMLBlockquoteElement',
		'HTMLBodyElement',
		'HTMLBRElement',
		'HTMLButtonElement',
		'HTMLCollection',
		'HTMLDirectoryElement',
		'HTMLDivElement',
		'HTMLDListElement',
		'HTMLDocument',
		'HTMLElement',
		'HTMLFieldSetElement',
		'HTMLFontElement',
		'HTMLFormElement',
		'HTMLFrameElement',
		'HTMLFrameSetElement',
		'HTMLHeadElement',
		'HTMLHeadingElement',
		'HTMLHRElement',
		'HTMLHtmlElement',
		'HTMLIFrameElement',
		'HTMLImageElement',
		'HTMLInputElement',
		'HTMLIsIndexElement',
		'HTMLLabelElement',
		'HTMLLegendElement',
		'HTMLLIElement',
		'HTMLLinkElement',
		'HTMLMapElement',
		'HTMLMenuElement',
		'HTMLMetaElement',
		'HTMLModElement',
		'HTMLObjectElement',
		'HTMLOListElement',
		'HTMLOptGroupElement',
		'HTMLOptionElement',
		'HTMLParagraphElement',
		'HTMLParamElement',
		'HTMLPreElement',
		'HTMLQuoteElement',
		'HTMLScriptElement',
		'HTMLSelectElement',
		'HTMLStyleElement',
		'HTMLTableCaptionElement',
		'HTMLTableCellElement',
		'HTMLTableColElement',
		'HTMLTableElement',
		'HTMLTableRowElement',
		'HTMLTableSectionElement',
		'HTMLTextAreaElement',
		'HTMLTitleElement',
		'HTMLUListElement'];
		return Cats[option];
		}

		public void displayCategories() 
		{
			cdoc.write("<BODY BGCOLOR=\"#0000FF\" TEXT=\"#FFFF00\">\n");
			cdoc.write("<CENTER>\n");
			cdoc.write("<IMG SRC=\"pix/nist.gif\" width=100 height=75>\n");
			cdoc.write("<P>\n");
			cdoc.write("<b>DOM<BR>Categories</b><p>\n");
			cdoc.write("<FORM NAME=selectCategory>\n");
			cdoc.write("<SELECT NAME=category onClick=displayInterfaces(this.form)>\n");
			cdoc.write("<OPTION SELECTED VALUE=Fundamental>Fundamental\n");
			cdoc.write("<OPTION VALUE=Extended>Extended\n");
			cdoc.write("<OPTION VALUE=HTML>HTML\n");
			cdoc.write("</select>\n");
			cdoc.write("</form>\n");
			cdoc.write("</CENTER>\n");
			cdoc.write("</BODY>\n");
			cdoc.close();
		}
		public void displayInterfaces(form) 
		{

			cat = form.category.options[form.category.selectedIndex].Value;
			interfaces = getInterfaces(cat);

			idoc.write("<BODY BGCOLOR=\"#0000FF\" TEXT=\"#FFFF00\">\n");
			idoc.write("<CENTER>\n");
			idoc.write("<P>\n");
			idoc.write("<b>DOM<BR>Interfaces</b><p>\n");
			idoc.write("<FORM NAME=selectInterface>\n");
			idoc.write("<SELECT NAME=interface onClick=parent.navig()>\n");
			for (i = 0; i < interfaces.length; i++)
				idoc.write("<OPTION VALUE="+interfaces[i]+">"+interfaces[i]+"\n");
			idoc.write("</select>\n");
			idoc.write("</form>\n");
			idoc.write("</CENTER>\n");
			idoc.write("</BODY>\n");
			idoc.close();
		}
*/
	
	}//class

}
