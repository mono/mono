//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Text;
    using System.IO;
    using System.Globalization;

    // Describes a "state" in the interpretter. A state is any source location that
    // a breakpoint could be set on or that could be stepped to.
    [DebuggerNonUserCode]
    [Fx.Tag.XamlVisible(false)]
    public class State
    {
        [Fx.Tag.SecurityNote(Critical = "This value is used in IL generation performed under an assert. It gets validated before setting in partial trust.")]
        [SecurityCritical]
        SourceLocation location;
        [Fx.Tag.SecurityNote(Critical = "This value is used in IL generation performed under an assert. It gets validated before setting in partial trust.")]
        [SecurityCritical]
        string name;
        IEnumerable<LocalsItemDescription> earlyLocals;
        int numberOfEarlyLocals;

        // Calling Type.GetMethod() is slow (10,000 calls can take ~1 minute).
        // So we stash extra fields to be able to make the call lazily (as we Enter the state).
        // this.type.GetMethod
        Type type;
        [Fx.Tag.SecurityNote(Critical = "This value is used in IL generation performed under an assert. It gets validated before setting in partial trust.")]
        [SecurityCritical]
        string methodName;

        [Fx.Tag.SecurityNote(Critical = "This value is used in IL generation performed under an assert. Used to determine if we should invoke the generated code for this state.")]
        [SecurityCritical]
        bool debuggingEnabled = true;

        [Fx.Tag.SecurityNote(Critical = "Sets SecurityCritical name member.",
            Safe = "We validate the SourceLocation and name before storing it in the member when running in Partial Trust.")]
        [SecuritySafeCritical]
        internal State(SourceLocation location, string name, IEnumerable<LocalsItemDescription> earlyLocals, int numberOfEarlyLocals)
        {
            // If we are running in Partial Trust, validate the name string. We only do this in partial trust for backward compatability.
            // We are doing the validation because we want to prevent anything passed to us by non-critical code from affecting the generation
            // of the code to the dynamic assembly we are creating.
            if (!PartialTrustHelpers.AppDomainFullyTrusted)
            {
                this.name = ValidateIdentifierString(name);
                this.location = ValidateSourceLocation(location);
            }
            else
            {
                this.location = location;
                this.name = name;
            }

            this.earlyLocals = earlyLocals;
            Fx.Assert(earlyLocals != null || numberOfEarlyLocals == 0,
                "If earlyLocals is null then numberOfEarlyLocals should be 0");
            // Ignore the passed numberOfEarlyLocals if earlyLocal is null.
            this.numberOfEarlyLocals = (earlyLocals == null) ? 0 : numberOfEarlyLocals;
        }

        // Location in source file associated with this state.
        internal SourceLocation Location
        {
            [Fx.Tag.SecurityNote(Critical = "Accesses the SecurityCritical location member. We validated the location when this object was constructed.",
                Safe = "SourceLocation is immutable and we validated it in the constructor.")]
            [SecuritySafeCritical]
            get { return this.location; }
        }


        // Friendly name of the state. May be null if state is not named.
        // States need unique names.
        internal string Name
        {
            [Fx.Tag.SecurityNote(Critical = "Sets SecurityCritical name member.",
                Safe = "We are only reading it, not setting it.")]
            [SecuritySafeCritical]
            get { return this.name; }
        }


        // Type definitions for early bound locals. This list is ordered.
        // Names should be unique.
        internal IEnumerable<LocalsItemDescription> EarlyLocals
        {
            get { return this.earlyLocals; }
        }

        internal int NumberOfEarlyLocals
        {
            get { return this.numberOfEarlyLocals; }
        }

        internal bool DebuggingEnabled
        {
            [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical debuggingEnabled member.",
                Safe = "We don't change anyting. We only return the value.")]
            [SecuritySafeCritical]
            get
            {
                return this.debuggingEnabled;
            }

            [Fx.Tag.SecurityNote(Critical = "Sets SecurityCritical debuggingEnabled member.")]
            [SecuritySafeCritical]
            set
            {
                this.debuggingEnabled = value;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Sets SecurityCritical methodName member.")]
        [SecurityCritical]
        internal void CacheMethodInfo(Type type, string methodName)
        {
            this.type = type;
            this.methodName = methodName;
        }

        // Helper to lazily get the MethodInfo. This is expensive, so caller should cache it.
        [Fx.Tag.SecurityNote(Critical = "Generates and returns a MethodInfo that is used to generate the dynamic module and accesses Critical member methodName.")]
        [SecurityCritical]
        internal MethodInfo GetMethodInfo(bool withPriming)
        {
            MethodInfo methodInfo = this.type.GetMethod(withPriming ? StateManager.MethodWithPrimingPrefix + this.methodName : this.methodName);
            return methodInfo;
        }

        // internal because it is used from StateManager, too for the assembly name, type name, and type name prefix.
        internal static string ValidateIdentifierString(string input)
        {
            string result = input.Normalize(NormalizationForm.FormC);

            if (result.Length > 255)
            {
                result = result.Substring(0, 255);
            }

            // Make the identifier conform to Unicode programming language identifer specification.
            char[] chars = result.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                UnicodeCategory category = char.GetUnicodeCategory(chars[i]);
                // Check for identifier_start
                if ((category == UnicodeCategory.UppercaseLetter) ||
                    (category == UnicodeCategory.LowercaseLetter) ||
                    (category == UnicodeCategory.TitlecaseLetter) ||
                    (category == UnicodeCategory.ModifierLetter) ||
                    (category == UnicodeCategory.OtherLetter) ||
                    (category == UnicodeCategory.LetterNumber))
                {
                    continue;
                }
                // If it's not the first character, also check for identifier_extend
                if ((i != 0) &&
                    ((category == UnicodeCategory.NonSpacingMark) ||
                     (category == UnicodeCategory.SpacingCombiningMark) || 
                     (category == UnicodeCategory.DecimalDigitNumber) ||
                     (category == UnicodeCategory.ConnectorPunctuation) ||
                     (category == UnicodeCategory.Format)))
                {
                    continue;
                }

                // Not valid for identifiers - change it to an underscore.
                chars[i] = '_';
            }

            result = new string(chars);

            return result;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StateManager.DisableCodeGeneration.")]
        [SecurityCritical]
        SourceLocation ValidateSourceLocation(SourceLocation input)
        {
            bool returnNewLocation = false;
            string newFileName = input.FileName;

            if (string.IsNullOrWhiteSpace(newFileName))
            {
                this.DebuggingEnabled = false;
                Trace.WriteLine(SR.DebugInstrumentationFailed(SR.InvalidFileName(this.name)));
                return input;
            }

            // There was some validation of the column and line number already done in the SourceLocation constructor.
            // We are going to limit line and column numbers to Int16.MaxValue
            if ((input.StartLine > Int16.MaxValue) || (input.EndLine > Int16.MaxValue))
            {
                this.DebuggingEnabled = false;
                Trace.WriteLine(SR.DebugInstrumentationFailed(SR.LineNumberTooLarge(this.name)));
                return input;
            }

            if ((input.StartColumn > Int16.MaxValue) || (input.EndColumn > Int16.MaxValue))
            {
                this.DebuggingEnabled = false;
                Trace.WriteLine(SR.DebugInstrumentationFailed(SR.ColumnNumberTooLarge(this.name)));
                return input;
            }

            // Truncate at 255 characters.
            if (newFileName.Length > 255)
            {
                newFileName = newFileName.Substring(0, 255);
                returnNewLocation = true;
            }

            if (ReplaceInvalidCharactersWithUnderscore(ref newFileName, Path.GetInvalidPathChars()))
            {
                returnNewLocation = true;
            }

            string fileNameOnly = Path.GetFileName(newFileName);
            if (ReplaceInvalidCharactersWithUnderscore(ref fileNameOnly, Path.GetInvalidFileNameChars()))
            {
                // The filename portion has been munged. We need to make a new full name.
                string path = Path.GetDirectoryName(newFileName);
                newFileName = path + "\\" + fileNameOnly;
                returnNewLocation = true;
            }

            if (returnNewLocation)
            {
                return new SourceLocation(newFileName, input.StartLine, input.StartColumn, input.EndLine, input.EndColumn);
            }

            return input;
        }

        static bool ReplaceInvalidCharactersWithUnderscore(ref string input, char[] invalidChars)
        {
            bool modified = false;
            int invalidIndex = 0;
            while ((invalidIndex = input.IndexOfAny(invalidChars)) != -1)
            {
                char[] charArray = input.ToCharArray();
                charArray[invalidIndex] = '_';
                input = new string(charArray);
                modified = true;

            }

            return modified;
        }
    }
}
