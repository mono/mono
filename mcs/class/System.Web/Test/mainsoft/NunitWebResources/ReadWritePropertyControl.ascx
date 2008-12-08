<%@ Control Language="C#" %>
<script runat="server">
    [System.ComponentModel.Bindable (true)]
    public bool ReadWriteField = true;
    
    int _readWriteProperty;
    [System.ComponentModel.Bindable (true)]
    public int ReadWriteProperty
    {
        get { return _readWriteProperty; }
        set { _readWriteProperty = value; }
    }
    
    string _readWriteProperty2;
    [System.ComponentModel.Bindable (true)]
    public string ReadWriteProperty2
    {
	get { return _readWriteProperty2; }
        set { _readWriteProperty2 = value; }
    }
</script>
