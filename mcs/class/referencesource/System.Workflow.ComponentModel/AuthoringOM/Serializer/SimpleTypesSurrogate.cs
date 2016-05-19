namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Collections;
    using System.Collections.Generic;

    #region SimpleTypesSurrogate
    //This class is currently used only for Guids. The size diff is 93 bytes per guid over binary formatter
    //Will add support for other types as well, eventually.
    internal sealed class SimpleTypesSurrogate : ISerializationSurrogate
    {
        enum TypeID : byte
        {
            Guid = 1,
            Null,
        }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj.GetType() == typeof(Guid))
            {
                Guid guid = (Guid)obj;
                info.AddValue("typeID", TypeID.Guid);
                info.AddValue("bits", guid.ToByteArray());
            }
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            TypeID typeID = (TypeID)info.GetValue("typeID", typeof(TypeID));

            if (typeID == TypeID.Guid)
                return new Guid(info.GetValue("bits", typeof(byte[])) as byte[]);

            return null;
        }
    }
    #endregion
}
