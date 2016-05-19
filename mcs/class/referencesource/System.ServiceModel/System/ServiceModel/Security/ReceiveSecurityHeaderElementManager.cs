//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using ISignatureReaderProvider = System.IdentityModel.ISignatureReaderProvider;
    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;
    using SignedXml = System.IdentityModel.SignedXml;
    using System.Collections.Generic;

    sealed class ReceiveSecurityHeaderElementManager : ISignatureReaderProvider
    {
        const int InitialCapacity = 8;
        readonly ReceiveSecurityHeader securityHeader;
        ReceiveSecurityHeaderEntry[] elements;
        int count;
        readonly string[] headerIds;
        string[] predecryptionHeaderIds;
        string bodyId;
        string bodyContentId;
        bool isPrimaryTokenSigned = false;

        public ReceiveSecurityHeaderElementManager(ReceiveSecurityHeader securityHeader)
        {
            this.securityHeader = securityHeader;
            this.elements = new ReceiveSecurityHeaderEntry[InitialCapacity];
            if (securityHeader.RequireMessageProtection)
            {
                this.headerIds = new string[securityHeader.ProcessedMessage.Headers.Count];
            }
        }

        public int Count
        {
            get { return this.count; }
        }

        public bool IsPrimaryTokenSigned
        {
            get { return this.isPrimaryTokenSigned; }
            set { this.isPrimaryTokenSigned = value; }
        }

        public void AppendElement(
            ReceiveSecurityHeaderElementCategory elementCategory, object element,
            ReceiveSecurityHeaderBindingModes bindingMode, string id, TokenTracker supportingTokenTracker)
        {
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
            }
            EnsureCapacityToAdd();
            this.elements[this.count++].SetElement(elementCategory, element, bindingMode, id, false, null, supportingTokenTracker);
        }

        public void AppendSignature(SignedXml signedXml)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.Signature, signedXml,
                ReceiveSecurityHeaderBindingModes.Unknown, signedXml.Id, null);
        }

        public void AppendReferenceList(ReferenceList referenceList)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.ReferenceList, referenceList,
                ReceiveSecurityHeaderBindingModes.Unknown, null, null);
        }

        public void AppendEncryptedData(EncryptedData encryptedData)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.EncryptedData, encryptedData,
                ReceiveSecurityHeaderBindingModes.Unknown, encryptedData.Id, null);
        }

        public void AppendSignatureConfirmation(ISignatureValueSecurityElement signatureConfirmationElement)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.SignatureConfirmation, signatureConfirmationElement,
                ReceiveSecurityHeaderBindingModes.Unknown, signatureConfirmationElement.Id, null);
        }

        public void AppendTimestamp(SecurityTimestamp timestamp)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.Timestamp, timestamp,
                ReceiveSecurityHeaderBindingModes.Unknown, timestamp.Id, null);
        }

        public void AppendSecurityTokenReference(SecurityKeyIdentifierClause strClause, string strId)
        {
            if (!String.IsNullOrEmpty(strId))
            {
                VerifyIdUniquenessInSecurityHeader(strId);
                AppendElement(ReceiveSecurityHeaderElementCategory.SecurityTokenReference, strClause, ReceiveSecurityHeaderBindingModes.Unknown, strId, null);
            }
        }

        public void AppendToken(SecurityToken token, ReceiveSecurityHeaderBindingModes mode, TokenTracker supportingTokenTracker)
        {
            AppendElement(ReceiveSecurityHeaderElementCategory.Token, token,
                mode, token.Id, supportingTokenTracker);
        }

        public void EnsureAllRequiredSecurityHeaderTargetsWereProtected()
        {
            Fx.Assert(this.securityHeader.RequireMessageProtection, "security header protection checks should only be done for message security");
            ReceiveSecurityHeaderEntry entry;
            for (int i = 0; i < this.count; i++)
            {
                GetElementEntry(i, out entry);
                if (!entry.signed)
                {
                    switch (entry.elementCategory)
                    {
                        case ReceiveSecurityHeaderElementCategory.Timestamp:
                        case ReceiveSecurityHeaderElementCategory.SignatureConfirmation:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new MessageSecurityException(SR.GetString(SR.RequiredSecurityHeaderElementNotSigned, entry.elementCategory, entry.id)));
                        case ReceiveSecurityHeaderElementCategory.Token:
                            switch (entry.bindingMode)
                            {
                                case ReceiveSecurityHeaderBindingModes.Signed:
                                case ReceiveSecurityHeaderBindingModes.SignedEndorsing:
                                case ReceiveSecurityHeaderBindingModes.Basic:
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new MessageSecurityException(SR.GetString(SR.RequiredSecurityTokenNotSigned, entry.element, entry.bindingMode)));
                            }
                            break;
                    }
                }
                
                if (!entry.encrypted)
                {
                    if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.Token &&
                        entry.bindingMode == ReceiveSecurityHeaderBindingModes.Basic)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new MessageSecurityException(SR.GetString(SR.RequiredSecurityTokenNotEncrypted, entry.element, entry.bindingMode)));
                    }
                }
            }
        }

        void EnsureCapacityToAdd()
        {
            if (this.count == this.elements.Length)
            {
                ReceiveSecurityHeaderEntry[] newElements = new ReceiveSecurityHeaderEntry[this.elements.Length * 2];
                Array.Copy(this.elements, 0, newElements, 0, this.count);
                this.elements = newElements;
            }
        }

        public object GetElement(int index)
        {
            Fx.Assert(0 <= index && index < this.count, "");
            return this.elements[index].element;
        }

        public T GetElement<T>(int index) where T : class
        {
            Fx.Assert(0 <= index && index < this.count, "");
            return (T) this.elements[index].element;
        }

        public void GetElementEntry(int index, out ReceiveSecurityHeaderEntry element)
        {
            Fx.Assert(0 <= index && index < this.count, "index out of range");
            element = this.elements[index];
        }

        public ReceiveSecurityHeaderElementCategory GetElementCategory(int index)
        {
            Fx.Assert(0 <= index && index < this.count, "index out of range");
            return this.elements[index].elementCategory;
        }

        public void GetPrimarySignature(out XmlDictionaryReader reader, out string id)
        {
            ReceiveSecurityHeaderEntry entry;
            for (int i = 0; i < this.count; i++)
            {
                GetElementEntry(i, out entry);
                if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.Signature &&
                    entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary)
                {
                    reader = GetReader(i, false);
                    id = entry.id;
                    return;
                }
            }
            reader = null;
            id = null;
            return;
        }

        internal XmlDictionaryReader GetReader(int index, bool requiresEncryptedFormReader)
        {
            Fx.Assert(0 <= index && index < this.count, "index out of range");
            if (!requiresEncryptedFormReader)
            {
                byte[] decryptedBuffer = this.elements[index].decryptedBuffer;
                if (decryptedBuffer != null)
                {
                    return this.securityHeader.CreateDecryptedReader(decryptedBuffer);
                }
            }
            XmlDictionaryReader securityHeaderReader = this.securityHeader.CreateSecurityHeaderReader();
            securityHeaderReader.ReadStartElement();
            for (int i = 0; securityHeaderReader.IsStartElement() && i < index; i++)
            {
                securityHeaderReader.Skip();
            }
            return securityHeaderReader;
        }

        public XmlDictionaryReader GetSignatureVerificationReader(string id, bool requiresEncryptedFormReaderIfDecrypted)
        {
            ReceiveSecurityHeaderEntry entry;
            for (int i = 0; i < this.count; i++)
            {
                GetElementEntry(i, out entry);
                bool encryptedForm = entry.encrypted && requiresEncryptedFormReaderIfDecrypted;
                bool isSignedToken = (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Signed) || (entry.bindingMode == ReceiveSecurityHeaderBindingModes.SignedEndorsing);
                if (entry.MatchesId(id, encryptedForm))
                {
                    SetSigned(i);
                    if (!this.IsPrimaryTokenSigned)
                    {
                        this.IsPrimaryTokenSigned = entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary && entry.elementCategory == ReceiveSecurityHeaderElementCategory.Token;
                    }
                    return GetReader(i, encryptedForm);
                }                
                else if (entry.MatchesId(id, isSignedToken))
                {
                    SetSigned(i);
                    if (!this.IsPrimaryTokenSigned)
                    {
                        this.IsPrimaryTokenSigned = entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary && entry.elementCategory == ReceiveSecurityHeaderElementCategory.Token;
                    }
                    return GetReader(i, isSignedToken);
                }
            }
            return null;
        }

        void OnDuplicateId(string id)
        {
            throw TraceUtility.ThrowHelperError(
                new MessageSecurityException(SR.GetString(SR.DuplicateIdInMessageToBeVerified, id)), this.securityHeader.SecurityVerifiedMessage);
        }

        public void SetBindingMode(int index, ReceiveSecurityHeaderBindingModes bindingMode)
        {
            Fx.Assert(0 <= index && index < this.count, "index out of range");
            this.elements[index].bindingMode = bindingMode;
        }

        public void SetElement(int index, object element)
        {
            Fx.Assert(0 <= index && index < this.count, "");
            this.elements[index].element = element;
        }

        public void ReplaceHeaderEntry(int index, ReceiveSecurityHeaderEntry element)
        {
            Fx.Assert(0 <= index && index < this.count, "");
            this.elements[index] = element;
        }

        public void SetElementAfterDecryption(
            int index,
            ReceiveSecurityHeaderElementCategory elementCategory, object element,
            ReceiveSecurityHeaderBindingModes bindingMode, string id, byte[] decryptedBuffer, TokenTracker supportingTokenTracker)
        {
            Fx.Assert(0 <= index && index < this.count, "index out of range");
            Fx.Assert(this.elements[index].elementCategory == ReceiveSecurityHeaderElementCategory.EncryptedData, "Replaced item must be EncryptedData");
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
            }
            this.elements[index].PreserveIdBeforeDecryption();
            this.elements[index].SetElement(elementCategory, element, bindingMode, id, true, decryptedBuffer, supportingTokenTracker);
        }

        public void SetSignatureAfterDecryption(int index, SignedXml signedXml, byte[] decryptedBuffer)
        {
            SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.Signature,
                                      signedXml, ReceiveSecurityHeaderBindingModes.Unknown, signedXml.Id, decryptedBuffer, null);
        }

        public void SetSignatureConfirmationAfterDecryption(int index, ISignatureValueSecurityElement signatureConfirmationElement, byte[] decryptedBuffer)
        {
            SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.SignatureConfirmation,
                                      signatureConfirmationElement, ReceiveSecurityHeaderBindingModes.Unknown, signatureConfirmationElement.Id, decryptedBuffer, null);
        }

        internal void SetSigned(int index)
        {
            Fx.Assert(0 <= index && index < this.count, "");
            this.elements[index].signed = true;
            if (this.elements[index].supportingTokenTracker != null)
            {
                this.elements[index].supportingTokenTracker.IsSigned = true;
            }
        }

        public void SetTimestampSigned(string id)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.elements[i].elementCategory == ReceiveSecurityHeaderElementCategory.Timestamp &&
                    this.elements[i].id == id)
                {
                    SetSigned(i);
                }
            }
        }

        public void SetTokenAfterDecryption(int index, SecurityToken token, ReceiveSecurityHeaderBindingModes mode, byte[] decryptedBuffer, TokenTracker supportingTokenTracker)
        {
            SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.Token, token, mode, token.Id, decryptedBuffer, supportingTokenTracker);
        }

        internal bool TryGetTokenElementIndexFromStrId(string strId, out int index)
        {
            index = -1;
            SecurityKeyIdentifierClause strClause = null;
            for (int position = 0; position < this.Count; position++)
            {
                if (this.GetElementCategory(position) == ReceiveSecurityHeaderElementCategory.SecurityTokenReference)
                {
                    strClause = this.GetElement(position) as SecurityKeyIdentifierClause;
                    if (strClause.Id == strId)
                        break;
                }
            }

            if (strClause == null)
                return false;

            for (int position = 0; position < this.Count; position++)
            {
                if (this.GetElementCategory(position) == ReceiveSecurityHeaderElementCategory.Token)
                {
                    SecurityToken token = this.GetElement(position) as SecurityToken;
                    if (token.MatchesKeyIdentifierClause(strClause))
                    {
                        index = position;
                        return true;
                    }
                }
            }

            return false;
        }

        public void VerifyUniquenessAndSetBodyId(string id)
        {
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
                VerifyIdUniquenessInMessageHeadersAndBody(id, this.headerIds.Length);
                this.bodyId = id;
            }
        }

        public void VerifyUniquenessAndSetBodyContentId(string id)
        {
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
                VerifyIdUniquenessInMessageHeadersAndBody(id, this.headerIds.Length);
                this.bodyContentId = id;
            }
        }

        public void VerifyUniquenessAndSetDecryptedHeaderId(string id, int headerIndex)
        {
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
                VerifyIdUniquenessInMessageHeadersAndBody(id, headerIndex);
                if (this.predecryptionHeaderIds == null)
                {
                    this.predecryptionHeaderIds = new string[headerIds.Length];
                }
                this.predecryptionHeaderIds[headerIndex] = this.headerIds[headerIndex];
                this.headerIds[headerIndex] = id;
            }
        }

        public void VerifyUniquenessAndSetHeaderId(string id, int headerIndex)
        {
            if (id != null)
            {
                VerifyIdUniquenessInSecurityHeader(id);
                VerifyIdUniquenessInMessageHeadersAndBody(id, headerIndex);
                this.headerIds[headerIndex] = id;
            }
        }

        void VerifyIdUniquenessInHeaderIdTable(string id, int headerCount, string[] headerIdTable)
        {
            for (int i = 0; i < headerCount; i++)
            {
                if (headerIdTable[i] == id)
                {
                    OnDuplicateId(id);
                }
            }
        }

        void VerifyIdUniquenessInSecurityHeader(string id)
        {
            Fx.Assert(id != null, "Uniqueness should only be tested for non-empty ids");
            for (int i = 0; i < this.count; i++)
            {
                if (this.elements[i].id == id || this.elements[i].encryptedFormId == id)
                {
                    OnDuplicateId(id);
                }
            }
        }

        void VerifyIdUniquenessInMessageHeadersAndBody(string id, int headerCount)
        {
            Fx.Assert(id != null, "Uniqueness should only be tested for non-empty ids");
            VerifyIdUniquenessInHeaderIdTable(id, headerCount, this.headerIds);
            if (this.predecryptionHeaderIds != null)
            {
                VerifyIdUniquenessInHeaderIdTable(id, headerCount, this.predecryptionHeaderIds);
            }
            if (this.bodyId == id || this.bodyContentId == id)
            {
                OnDuplicateId(id);
            }
        }

        XmlDictionaryReader ISignatureReaderProvider.GetReader(object callbackContext)
        {
            int index = (int)callbackContext;
            Fx.Assert(index < this.Count, "Invalid Context provided.");
            return GetReader(index, false);
        }

        public void VerifySignatureConfirmationWasFound()
        {
            ReceiveSecurityHeaderEntry entry;
            for (int i = 0; i < this.count; i++)
            {
                GetElementEntry(i, out entry);
                if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.SignatureConfirmation)
                {
                    return;
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SignatureConfirmationWasExpected)));
        }

    }
}
