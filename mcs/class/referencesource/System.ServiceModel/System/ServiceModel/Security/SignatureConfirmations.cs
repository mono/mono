//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;

    class SignatureConfirmations
    {
        SignatureConfirmation[] confirmations;
        int length;
        bool encrypted;

        struct SignatureConfirmation
        {
            public byte[] value;

            public SignatureConfirmation(byte[] value)
            {
                this.value = value;
            }
        }

        public SignatureConfirmations()
        {
            confirmations = new SignatureConfirmation[1];
            length = 0;
        }

        public int Count
        {
            get { return length; }
        }

        public void AddConfirmation(byte[] value, bool encrypted)
        {
            if (confirmations.Length == length)
            {
                SignatureConfirmation[] newConfirmations = new SignatureConfirmation[length * 2];
                Array.Copy(confirmations, 0, newConfirmations, 0, length);
                confirmations = newConfirmations;
            }
            confirmations[length] = new SignatureConfirmation(value);
            ++length;
            this.encrypted |= encrypted;
        }

        public void GetConfirmation(int index, out byte[] value, out bool encrypted)
        {
            if (index < 0 || index >= length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeInRange, 0, length)));
            }

            value = confirmations[index].value;
            encrypted = this.encrypted;
        }

        public bool IsMarkedForEncryption
        {
            get { return this.encrypted; }
        }
    }
}
