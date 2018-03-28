//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Abstract class for SecurityKeyIdentifierClause Serializer.
    /// </summary>
    public abstract class SecurityKeyIdentifierClauseSerializer
    {
        /// <summary>
        /// When implmented in the derived class will check the element where the reader is
        /// positioned for a SecurityKeyIdentifierClause type.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a SecurityKeyIdentifierClause element.</param>
        /// <returns>True if the SecurityKeyIdentifierClause can be deserialized.</returns>
        public abstract bool CanReadKeyIdentifierClause( XmlReader reader );

        /// <summary>
        /// When implemented in the derived class, the method checks if the given
        /// SecurityKeyIdentifierClause can be serialized.
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        /// <returns>True if the SecurityKeyIdentifierClause can be serialized.</returns>
        public abstract bool CanWriteKeyIdentifierClause( SecurityKeyIdentifierClause securityKeyIdentifierClause );

        /// <summary>
        /// When implemented in the dervice class will deserialize a SecurityKeyIdentifierClause
        /// from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a SecurityKeyIdentifierClause.</param>
        /// <returns>Deserialized SecurityKeyIdentifierClause</returns>
        public abstract SecurityKeyIdentifierClause ReadKeyIdentifierClause( XmlReader reader );

        /// <summary>
        /// When implemented in the derived class will serialize the given SecurityKeyIdentifierClause
        /// to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SecurityKeyIdenfierClause.</param>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        public abstract void WriteKeyIdentifierClause( XmlWriter writer, SecurityKeyIdentifierClause securityKeyIdentifierClause );
    }
}
