using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace System.Web.Script.Serialization.TypeResolver.CS
{
    public class CustomTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            return Type.GetType(id);
        }

        public override string ResolveTypeId(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.Name;
        }
    }

    public class ColorType
    {
        public string[] rgb =
            new string[] { "00", "00", "FF" };
        public FavoriteColors defaultColor = FavoriteColors.Blue;
    }

    public enum FavoriteColors
    {
        Black,
        White,
        Blue,
        Red
    }
}
