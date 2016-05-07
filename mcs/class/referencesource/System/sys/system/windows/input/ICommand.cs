//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
//---------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Markup;

namespace System.Windows.Input
{
    class BuildInfo{
        public const string WCP_VERSION="4.0.0.0";
        public const string WCP_PUBLIC_KEY_TOKEN="31bf3856ad364e35";

    }
    ///<summary>
    ///     An interface that allows an application author to define a method to be invoked.
    ///</summary>
    [TypeForwardedFrom("PresentationCore, Version=" + BuildInfo.WCP_VERSION + ", Culture=neutral, PublicKeyToken=" + BuildInfo.WCP_PUBLIC_KEY_TOKEN)]
    [TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=" + BuildInfo.WCP_VERSION + ", Culture=neutral, PublicKeyToken=" + BuildInfo.WCP_PUBLIC_KEY_TOKEN + ", Custom=null")]
    [ValueSerializer("System.Windows.Input.CommandValueSerializer, PresentationFramework, Version=" + BuildInfo.WCP_VERSION + ", Culture=neutral, PublicKeyToken=" + BuildInfo.WCP_PUBLIC_KEY_TOKEN + ", Custom=null")]
    public interface ICommand
    {
        /// <summary>
        ///     Raised when the ability of the command to execute has changed.
        /// </summary>
        event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Returns whether the command can be executed.
        /// </summary>
        /// <param name="parameter">A parameter that may be used in executing the command. This parameter may be ignored by some implementations.</param>
        /// <returns>true if the command can be executed with the given parameter and current state. false otherwise.</returns>
        bool CanExecute(object parameter);

        /// <summary>
        ///     Defines the method that should be executed when the command is executed.
        /// </summary>
        /// <param name="parameter">A parameter that may be used in executing the command. This parameter may be ignored by some implementations.</param>
        void Execute(object parameter);
    }
}
