<%@ Control Language="C#" %>
<script runat="server">
    [System.ComponentModel.Bindable (true)]
    public readonly bool ReadOnlyField = true;
    [System.ComponentModel.Bindable (true)]
    public bool ReadOnlyProperty
    {
        get { return true; }
    }
</script>
