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
</script>
