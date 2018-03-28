//------------------------------------------------------------------------------
// <copyright file="Purpose.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    // Represents a purpose that can be passed to a cryptographic routine to control key derivation / ciphertext modification.
    // This is hardening the crypto routines to prevent playing ciphertext off of components that didn't generate them.
    //
    // !! IMPORTANT !!
    // The built-in purposes do not contain privileged information and are not meant to be treated as secrets. Any external
    // person can disassemble our code or look directly at our source to see what our Purpose objects are used for.
    //
    // PrimaryPurpose: This is a well-known string that identifies the reason for this Purpose. The pattern we use
    // is that PrimaryPurpose is the name of the consumer, making each consumer's Purpose unique.
    //
    // SpecificPurposes: These are extra optional strings that further differentiate Purpose objects that might have the
    // same PrimaryPurpose. The pattern we use is that if a single consumer has multiple Purposes, he can use
    // SpecificPurposes to uniquely identify them. The information here is generally not secret (we can put the type of
    // the currently executing Page here, for example), but it is valid to seed this property with a secret obtained
    // at runtime (such as a nonce shared between two parties).

    internal sealed class Purpose {

        // predefined purposes
        public static readonly Purpose AnonymousIdentificationModule_Ticket = new Purpose("AnonymousIdentificationModule.Ticket");
        public static readonly Purpose AssemblyResourceLoader_WebResourceUrl = new Purpose("AssemblyResourceLoader.WebResourceUrl");
        public static readonly Purpose FormsAuthentication_Ticket = new Purpose("FormsAuthentication.Ticket");
        public static readonly Purpose WebForms_Page_PreviousPageID = new Purpose("WebForms.Page.PreviousPageID");
        public static readonly Purpose RolePrincipal_Ticket = new Purpose("RolePrincipal.Ticket");
        public static readonly Purpose ScriptResourceHandler_ScriptResourceUrl = new Purpose("ScriptResourceHandler.ScriptResourceUrl");

        // predefined ViewState purposes; they won't be used as-is (they're combined with the page information)
        public static readonly Purpose WebForms_ClientScriptManager_EventValidation = new Purpose("WebForms.ClientScriptManager.EventValidation");
        public static readonly Purpose WebForms_DetailsView_KeyTable = new Purpose("WebForms.DetailsView.KeyTable");
        public static readonly Purpose WebForms_GridView_DataKeys = new Purpose("WebForms.GridView.DataKeys");
        public static readonly Purpose WebForms_GridView_SortExpression = new Purpose("WebForms.GridView.SortExpression");
        public static readonly Purpose WebForms_HiddenFieldPageStatePersister_ClientState = new Purpose("WebForms.HiddenFieldPageStatePersister.ClientState");
        public static readonly Purpose WebForms_ScriptManager_HistoryState = new Purpose("WebForms.ScriptManager.HistoryState");
        public static readonly Purpose WebForms_SessionPageStatePersister_ClientState = new Purpose("WebForms.SessionPageStatePersister.ClientState");

        // predefined miscellaneoous purposes; they won't be used as-is (they're combined with other specificPurposes)
        public static readonly Purpose User_MachineKey_Protect = new Purpose("User.MachineKey.Protect"); // used by the MachineKey static class Protect / Unprotect methods
        public static readonly Purpose User_ObjectStateFormatter_Serialize = new Purpose("User.ObjectStateFormatter.Serialize"); // used by ObjectStateFormatter.Serialize() if called manually

        public readonly string PrimaryPurpose;
        public readonly string[] SpecificPurposes;

        private byte[] _derivedKeyLabel;
        private byte[] _derivedKeyContext;

        public Purpose(string primaryPurpose, params string[] specificPurposes)
            : this(primaryPurpose, specificPurposes, null, null) {
        }

        // ctor for unit testing
        internal Purpose(string primaryPurpose, string[] specificPurposes, CryptographicKey derivedEncryptionKey, CryptographicKey derivedValidationKey) {
            PrimaryPurpose = primaryPurpose;
            SpecificPurposes = specificPurposes ?? new string[0];
            DerivedEncryptionKey = derivedEncryptionKey;
            DerivedValidationKey = derivedValidationKey;
            SaveDerivedKeys = (SpecificPurposes.Length == 0);
        }

        // The cryptographic keys that were derived from this Purpose.
        internal CryptographicKey DerivedEncryptionKey { get; private set; }
        internal CryptographicKey DerivedValidationKey { get; private set; }

        // Whether the derived key should be saved back to this Purpose object by the ICryptoService,
        // e.g. because this Purpose will be used over and over again. We assume that any built-in
        // Purpose object that is passed without any specific purposes is intended for repeated use,
        // hence the ICryptoService will try to cache cryptographic keys as a performance optimization.
        // If specific purposes have been specified, they were likely generated at runtime, hence it
        // is not appropriate for the keys to be cached in this instance.
        internal bool SaveDerivedKeys { get; set; }

        // Returns a new Purpose which is the specified Purpose plus the specified SpecificPurpose.
        // Leaves the original Purpose unmodified.
        internal Purpose AppendSpecificPurpose(string specificPurpose) {
            // Append the specified specificPurpose to the existing list
            string[] newSpecificPurposes = new string[SpecificPurposes.Length + 1];
            Array.Copy(SpecificPurposes, newSpecificPurposes, SpecificPurposes.Length);
            newSpecificPurposes[newSpecificPurposes.Length - 1] = specificPurpose;
            return new Purpose(PrimaryPurpose, newSpecificPurposes);
        }

        // Returns a new Purpose which is the specified Purpose plus the specified SpecificPurposes.
        // Leaves the original Purpose unmodified.
        internal Purpose AppendSpecificPurposes(IList<string> specificPurposes) {
            // No specific purposes to add
            if (specificPurposes == null || specificPurposes.Count == 0) {
                return this;
            }

            // Append the specified specificPurposes to the existing list
            string[] newSpecificPurposes = new string[SpecificPurposes.Length + specificPurposes.Count];
            Array.Copy(SpecificPurposes, newSpecificPurposes, SpecificPurposes.Length);
            specificPurposes.CopyTo(newSpecificPurposes, SpecificPurposes.Length);
            return new Purpose(PrimaryPurpose, newSpecificPurposes);
        }

        public CryptographicKey GetDerivedEncryptionKey(IMasterKeyProvider masterKeyProvider, KeyDerivationFunction keyDerivationFunction) {
            // has a key already been stored?
            CryptographicKey actualDerivedKey = DerivedEncryptionKey;
            if (actualDerivedKey == null) {
                CryptographicKey masterKey = masterKeyProvider.GetEncryptionKey();
                actualDerivedKey = keyDerivationFunction(masterKey, this);

                // only save the key back to storage if this Purpose is configured to do so
                if (SaveDerivedKeys) {
                    DerivedEncryptionKey = actualDerivedKey;
                }
            }

            return actualDerivedKey;
        }

        public CryptographicKey GetDerivedValidationKey(IMasterKeyProvider masterKeyProvider, KeyDerivationFunction keyDerivationFunction) {
            // has a key already been stored?
            CryptographicKey actualDerivedKey = DerivedValidationKey;
            if (actualDerivedKey == null) {
                CryptographicKey masterKey = masterKeyProvider.GetValidationKey();
                actualDerivedKey = keyDerivationFunction(masterKey, this);

                // only save the key back to storage if this Purpose is configured to do so
                if (SaveDerivedKeys) {
                    DerivedValidationKey = actualDerivedKey;
                }
            }

            return actualDerivedKey;
        }

        // Returns a label and context suitable for passing into the SP800-108 KDF.
        internal void GetKeyDerivationParameters(out byte[] label, out byte[] context) {
            // The primary purpose can just be used as the label directly, since ASP.NET
            // is always in full control of the primary purpose (it's never user-specified).
            if (_derivedKeyLabel == null) {
                _derivedKeyLabel = CryptoUtil.SecureUTF8Encoding.GetBytes(PrimaryPurpose);
            }

            // The specific purposes (which can contain nonce, identity, etc.) are concatenated
            // together to form the context. The BinaryWriter class prepends each element with
            // a 7-bit encoded length to guarantee uniqueness.
            if (_derivedKeyContext == null) {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream, CryptoUtil.SecureUTF8Encoding)) {
                    foreach (string specificPurpose in SpecificPurposes) {
                        writer.Write(specificPurpose);
                    }
                    _derivedKeyContext = stream.ToArray();
                }
            }

            label = _derivedKeyLabel;
            context = _derivedKeyContext;
        }

    }
}
