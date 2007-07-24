using System;
using System.Web.Script.Serialization;

public class Group
{
    // The JavaScriptSerializer ignores this field.
    [ScriptIgnore]
    public string Comment;

    // The JavaScriptSerializer serializes this field.
    public string GroupName;
}
