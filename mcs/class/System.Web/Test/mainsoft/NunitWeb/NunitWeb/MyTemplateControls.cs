using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyTemplateControls
{
  public class TestTemplateItemEventArgs : EventArgs
  {
    TestTemplateItem item;

    public TestTemplateItem Item {
      get { return item; }
    }

    public TestTemplateItemEventArgs (TestTemplateItem item)
    {
      this.item = item;
    }
  }

  public delegate void TestTemplateItemCreatedEventHandler (TestTemplateControl sender, TestTemplateItemEventArgs args);
  
  public class TestTemplateControl : WebControl, INamingContainer
  {
    public event TestTemplateItemCreatedEventHandler ItemCreated;

    TestContainerControl container;
    
    public TestContainerControl Container {
      get { return container; }
    }

    public TestTemplateControl ()
    {
      container = new TestContainerControl (this);
    }
#if SYSTEM_WEB_EXTENSIONS
    protected
#else
    protected internal 
#endif
    override void CreateChildControls ()
    {
      Controls.Clear ();
      container.AddItem ();
      Controls.Add (container);
    }
    
    internal void OnItemCreated (TestTemplateItem item)
    {
      TestTemplateItemCreatedEventHandler eh = ItemCreated;

      if (eh != null)
	eh (this, new TestTemplateItemEventArgs (item));
    }
  }

  public class TestContainerControl : WebControl, INamingContainer
  {
    TestTemplateControl owner;
    ITemplate itemTemplate;
    
    [PersistenceMode (PersistenceMode.InnerProperty)]
    [TemplateContainer (typeof (TestTemplateItem))]
    public ITemplate ItemTemplate {
      get { return itemTemplate; }
      set { itemTemplate = value; }
    }
    
    public TestTemplateControl Owner {
      get { return owner; }
    }

    public TestContainerControl (TestTemplateControl owner)
    {
      this.owner = owner;
    }

    public void AddItem ()
    {
      TestTemplateItem item = new TestTemplateItem (this);
      item.SetupItem ();
      Controls.Add (item);
    }
  }
  
  sealed class DefaultTemplate : ITemplate
  {
    void ITemplate.InstantiateIn (Control container)
    {
      Label label = new Label ();
      label.Text = "Default Template Label";

      container.Controls.Add (label);
    }
  }
  
  public class TestTemplateItem : Control, INamingContainer
  {
    TestContainerControl container;

    public TestTemplateItem (TestContainerControl container)
    {
      this.container = container;
    }
    
    public void SetupItem ()
    {
      ITemplate template;

      if (container.ItemTemplate == null)
	template = new DefaultTemplate ();
      else
	template = container.ItemTemplate;

      template.InstantiateIn (this);
      container.Owner.OnItemCreated (this);
    }
  }
}
