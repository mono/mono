/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Dynamic.Utils;

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif
    /// <summary>
    /// Stores information needed to emit debugging symbol information for a
    /// source file, in particular the file name and unique language identifier.
    /// </summary>
    public class SymbolDocumentInfo {
        private readonly string _fileName;

        internal SymbolDocumentInfo(string fileName) {
            ContractUtils.RequiresNotNull(fileName, "fileName");
            _fileName = fileName;
        }

        /// <summary>
        /// The source file name.
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        /// <summary>
        /// Returns the language's unique identifier, if any.
        /// </summary>
        public virtual Guid Language {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Returns the language vendor's unique identifier, if any.
        /// </summary>
        public virtual Guid LanguageVendor {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Returns the document type's unique identifier, if any.
        /// Defaults to the guid for a text file.
        /// </summary>
        public virtual Guid DocumentType {
            get { return Compiler.SymbolGuids.DocumentType_Text; }
        }
    }

    internal sealed class SymbolDocumentWithGuids : SymbolDocumentInfo {
        private readonly Guid _language;
        private readonly Guid _vendor;
        private readonly Guid _documentType;

        internal SymbolDocumentWithGuids(string fileName, ref Guid language)
            : base(fileName) {
            _language = language;
            _documentType = Compiler.SymbolGuids.DocumentType_Text;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor)
            : base(fileName) {
            _language = language;
            _vendor = vendor;
            _documentType = Compiler.SymbolGuids.DocumentType_Text;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor, ref Guid documentType)
            : base(fileName) {
            _language = language;
            _vendor = vendor;
            _documentType = documentType;
        }

        public override Guid Language {
            get { return _language; }
        }

        public override Guid LanguageVendor {
            get { return _vendor; }
        }

        public override Guid DocumentType {
            get { return _documentType; }
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates an instance of <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" />.
        /// </summary>
        /// <param name="fileName">A <see cref="T:System.String" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> equal to.</param>
        /// <returns>A <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that has the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> property set to the specified value.</returns>
        public static SymbolDocumentInfo SymbolDocument(string fileName) {
            return new SymbolDocumentInfo(fileName);
        }

        /// <summary>
        /// Creates an instance of <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" />.
        /// </summary>
        /// <param name="fileName">A <see cref="T:System.String" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> equal to.</param>
        /// <param name="language">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> equal to.</param>
        /// <returns>A <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that has the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> properties set to the specified value.</returns>
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language) {
            return new SymbolDocumentWithGuids(fileName, ref language);
        }

        /// <summary>
        /// Creates an instance of <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" />.
        /// </summary>
        /// <param name="fileName">A <see cref="T:System.String" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> equal to.</param>
        /// <param name="language">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> equal to.</param>
        /// <param name="languageVendor">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.LanguageVendor" /> equal to.</param>
        /// <returns>A <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that has the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.LanguageVendor" /> properties set to the specified value.</returns>
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor) {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor);
        }

        /// <summary>
        /// Creates an instance of <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" />.
        /// </summary>
        /// <param name="fileName">A <see cref="T:System.String" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> equal to.</param>
        /// <param name="language">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> equal to.</param>
        /// <param name="languageVendor">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.LanguageVendor" /> equal to.</param>
        /// <param name="documentType">A <see cref="T:System.Guid" /> to set the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.DocumentType" /> equal to.</param>
        /// <returns>A <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that has the <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.FileName" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.Language" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.LanguageVendor" /> 
        /// and <see cref="P:System.Linq.Expressions.SymbolDocumentInfo.DocumentType" /> properties set to the specified value.</returns>
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor, Guid documentType) {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor, ref documentType);
        }
    }
}
