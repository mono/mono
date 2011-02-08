using System;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.CodeDom.Compiler;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class Bug666333Test
	{
		[Test]
		public void Bug666333 ()
		{
			// xml : original xml in the test
			// xml2 : when it is *appropriately* serialized
			// xml3 : mixed, d4p1:activeuser comes first
			// xml4 : mixed, d4p1:activeuser comes second
			// (Note that d4p1:activeuser is the actual element to be deserialized which takes precedence over urn:foo activeuser.)
		
			string xml = @"
				<CheckLoginResponse xmlns='http://tempuri.org/'>
					<playeractiveuser>
						<activeuser>
							<id>id</id>
							<hkey>hkey</hkey>
							<email>FOO@BAR.com</email>
							<lastcheck>2011-01-21T22:50:52.02</lastcheck>
						</activeuser>
						<response>
							<responsemessage>Acceso correcto, creado nuevo hkey!</responsemessage>
							<responsecode>1</responsecode>
							<langId>6</langId>
						</response>
					</playeractiveuser>
				</CheckLoginResponse>
				";

			string xml2 = @"
<CheckLoginResponse xmlns='http://tempuri.org/'>
  <playeractiveuser xmlns:d4p1='http://schemas.datacontract.org/2004/07/' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
    <d4p1:activeuser>
      <d4p1:email i:nil='true' />
      <d4p1:hkey i:nil='true' />
      <d4p1:id>idd</d4p1:id>
      <d4p1:lastcheck i:nil='true' />
    </d4p1:activeuser>
    <d4p1:response>
      <d4p1:langId i:nil='true' />
      <d4p1:responsecode>100</d4p1:responsecode>
      <d4p1:responsemessage i:nil='true' />
    </d4p1:response>
  </playeractiveuser>
</CheckLoginResponse>";

			string xml3 = @"
<CheckLoginResponse xmlns='http://tempuri.org/'>
  <playeractiveuser xmlns:d4p1='http://schemas.datacontract.org/2004/07/' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
    <d4p1:activeuser>
      <d4p1:email i:nil='true' />
      <d4p1:hkey i:nil='true' />
      <d4p1:id>iddd</d4p1:id>
      <d4p1:lastcheck i:nil='true' />
    </d4p1:activeuser>
    <activeuser xmlns='urn:foo'>
      <email i:nil='true' />
      <hkey i:nil='true' />
      <id>idd</id>
      <lastcheck i:nil='true' />
    </activeuser>
    <response xmlns='urn:foo'>
      <langId i:nil='true' />
      <responsecode>200</responsecode>
      <responsemessage i:nil='true' />
    </response>
  </playeractiveuser>
</CheckLoginResponse>";

			string xml4 = @"
<CheckLoginResponse xmlns='http://tempuri.org/'>
  <playeractiveuser xmlns:d4p1='http://schemas.datacontract.org/2004/07/' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
    <activeuser xmlns='urn:foo'>
      <email i:nil='true' />
      <hkey i:nil='true' />
      <id>idd</id>
      <lastcheck i:nil='true' />
    </activeuser>
    <d4p1:activeuser>
      <d4p1:email i:nil='true' />
      <d4p1:hkey i:nil='true' />
      <d4p1:id>iddd</d4p1:id>
      <d4p1:lastcheck i:nil='true' />
    </d4p1:activeuser>
    <response xmlns='urn:foo'>
      <langId i:nil='true' />
      <responsecode>200</responsecode>
      <responsemessage i:nil='true' />
    </response>
  </playeractiveuser>
</CheckLoginResponse>";
			
			var tm = TypedMessageConverter.Create (typeof (CheckLoginResponse), "urn:foo");
			var m = Message.CreateMessage (MessageVersion.Default, "urn:foo", XmlReader.Create (new StringReader (xml)));
			m = Message.CreateMessage (MessageVersion.Default, "urn:foo", XmlReader.Create (new StringReader (xml2)));
			m = Message.CreateMessage (MessageVersion.Default, "urn:foo", XmlReader.Create (new StringReader (xml3)));
			var clr = (CheckLoginResponse) tm.FromMessage (m);
			Assert.IsNotNull (clr.playeractiveuser, "#1");
			Assert.IsNotNull (clr.playeractiveuser.activeuser, "#2");
			Assert.AreEqual ("iddd", clr.playeractiveuser.activeuser.id, "#3");

			m = Message.CreateMessage (MessageVersion.Default, "urn:foo", XmlReader.Create (new StringReader (xml4)));
			Assert.AreEqual ("iddd", clr.playeractiveuser.activeuser.id, "#4");
	}

	}
}

// Generated code


[GeneratedCode("System.ServiceModel", "4.0.0.0"), DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Advanced), MessageContract(WrapperName="CheckLoginResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
public class CheckLoginResponse
{
    // Fields
    [MessageBodyMember(Namespace="http://tempuri.org/", Order=0), XmlElement(IsNullable=true)]
    public PlayerActiveUser playeractiveuser;

    // Methods
    public CheckLoginResponse()
    {
    }

    public CheckLoginResponse(PlayerActiveUser playeractiveuser)
    {
        this.playeractiveuser = playeractiveuser;
    }
}


[GeneratedCode("System.Xml", "4.0.30319.1"), DebuggerStepThrough, XmlType(Namespace="http://tempuri.org/")]
public class PlayerActiveUser : INotifyPropertyChanged
{
    // Fields
    private ActiveUserReference activeuserField;
    //private PropertyChangedEventHandler PropertyChanged;
    private Response responseField;

    // Events
    public event PropertyChangedEventHandler PropertyChanged;

    // Methods
    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged != null)
        {
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Properties
    [XmlElement(Order=0)]
    public ActiveUserReference activeuser
    {
        get
        {
            return this.activeuserField;
        }
        set
        {
            this.activeuserField = value;
            this.RaisePropertyChanged("activeuser");
        }
    }

    [XmlElement(Order=1)]
    public Response response
    {

        get
        {
            return this.responseField;
        }
        set
        {
            this.responseField = value;
            this.RaisePropertyChanged("response");
        }
    }
}


[XmlType(Namespace="http://tempuri.org/"), GeneratedCode("System.Xml", "4.0.30319.1"), DebuggerStepThrough]
public class Response : INotifyPropertyChanged
{
    // Fields
    private int? langIdField;
    //private PropertyChangedEventHandler PropertyChanged;
    private int? responsecodeField;
    private string responsemessageField;

    // Events
    public event PropertyChangedEventHandler PropertyChanged;

    // Methods
    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged != null)
        {
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Properties
    [XmlElement(IsNullable=true, Order=2)]
    public int? langId
    {
        get
        {
            return this.langIdField;
        }
        set
        {
            this.langIdField = value;
            this.RaisePropertyChanged("langId");
        }
    }

    [XmlElement(IsNullable=true, Order=1)]
    public int? responsecode
    {
        get
        {
            return this.responsecodeField;
        }
        set
        {
            this.responsecodeField = value;
            this.RaisePropertyChanged("responsecode");
        }
    }

    [XmlElement(Order=0)]
    public string responsemessage
    {
        get
        {
            return this.responsemessageField;
        }
        set
        {
            this.responsemessageField = value;
            this.RaisePropertyChanged("responsemessage");
        }
    }
}


[XmlType(Namespace="http://tempuri.org/"), DebuggerStepThrough, GeneratedCode("System.Xml", "4.0.30319.1")]
public class ActiveUserReference : ESObject
{
    // Fields
    private string emailField;
    private string hkeyField;
    private string idField;
    private DateTime? lastcheckField;

    // Properties
    [XmlElement(Order=2)]
    public string email
    {
        get
        {
            return this.emailField;
        }
        set
        {
            this.emailField = value;
            base.RaisePropertyChanged("email");
        }
    }

    [XmlElement(Order=1)]
    public string hkey
    {
        get
        {
            return this.hkeyField;
        }
        set
        {
            this.hkeyField = value;
            base.RaisePropertyChanged("hkey");
        }
    }

    [XmlElement(Order=0)]
    public string id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
            base.RaisePropertyChanged("id");
        }
    }

    [XmlElement(IsNullable=true, Order=3)]
    public DateTime? lastcheck
    {
        get
        {
            return this.lastcheckField;
        }
        set
        {
            this.lastcheckField = value;
            base.RaisePropertyChanged("lastcheck");
        }
    }
}


[XmlType(Namespace="http://tempuri.org/"), GeneratedCode("System.Xml", "4.0.30319.1"), XmlInclude(typeof(ActiveUserReference)), DebuggerStepThrough]
public abstract class ESObject : INotifyPropertyChanged
{
    // Fields
    //private PropertyChangedEventHandler PropertyChanged;

    // Events
    public event PropertyChangedEventHandler PropertyChanged;

    // Methods
    protected ESObject()
    {
    }

    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged != null)
        {
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

