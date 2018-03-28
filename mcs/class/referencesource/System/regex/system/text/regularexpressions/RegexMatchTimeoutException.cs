///------------------------------------------------------------------------------
/// <copyright file="RegexMatchTimeoutException.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
/// <owner>gpaperin</owner>
///------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;


namespace System.Text.RegularExpressions {

/// <summary>
/// This is the exception that is thrown when a RegEx matching timeout occurs.
/// </summary>

#if SILVERLIGHT
#if FEATURE_NETCORE
public
#else
internal 
#endif
class RegexMatchTimeoutException : TimeoutException {
#else
[Serializable]
public class RegexMatchTimeoutException : TimeoutException, ISerializable {
#endif


    private string regexInput = null;

    private string regexPattern = null;

    private TimeSpan matchTimeout = TimeSpan.FromTicks(-1);


    /// <summary>
    /// This is the preferred constructor to use.
    /// The other constructors are provided for compliance to Fx design guidelines.
    /// </summary>
    /// <param name="regexInput">Matching timeout occured during mathing within the specified input.</param>
    /// <param name="regexPattern">Matching timeout occured during mathing to the specified pattern.</param>
    /// <param name="matchTimeout">Matching timeout occured becasue matching took longer than the specified timeout.</param>
    public RegexMatchTimeoutException(string regexInput, string regexPattern, TimeSpan matchTimeout) :
        base(SR.GetString(SR.RegexMatchTimeoutException_Occurred)) {
        Init(regexInput, regexPattern, matchTimeout);
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>    
    public RegexMatchTimeoutException()
        : base() {
        Init();
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RegexMatchTimeoutException(string message)
        : base(message) {
        Init();
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a <code>null</code>.</param>
    public RegexMatchTimeoutException(string message, Exception inner)
        : base(message, inner) {
        Init();
    }


    #if !SILVERLIGHT
    /// <summary>
    /// Initializes a new RegexMatchTimeoutException with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo  that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext  that contains contextual information about the source or destination.</param>
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    protected RegexMatchTimeoutException(SerializationInfo info, StreamingContext context) :
        base(info, context) {

        string input = info.GetString("regexInput");
        string pattern = info.GetString("regexPattern");
        TimeSpan timeout = TimeSpan.FromTicks(info.GetInt64("timeoutTicks"));
        Init(input, pattern, timeout);
    }
        
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context) {
        base.GetObjectData(si, context);
        si.AddValue("regexInput", this.regexInput);
        si.AddValue("regexPattern", this.regexPattern);
        si.AddValue("timeoutTicks", this.matchTimeout.Ticks);
    }
    #endif // !SILVERLIGHT

    private void Init() {
        Init("", "", TimeSpan.FromTicks(-1));
    }

    private void Init(string input, string pattern, TimeSpan timeout) {
        this.regexInput = input;
        this.regexPattern = pattern;
        this.matchTimeout = timeout;
    }

    #if SILVERLIGHT && !FEATURE_NETCORE
    internal string Pattern {
    #else
    public string Pattern {
    #endif
    #if SILVERLIGHT
        [SecurityCritical]
    #else  // SILVERLIGHT
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    #endif  // SILVERLIGHT
        get { return regexPattern; }
    }

    #if SILVERLIGHT && !FEATURE_NETCORE
    internal string Input {
    #else
    public string Input {
    #endif
    #if SILVERLIGHT
        [SecurityCritical]
    #else  // SILVERLIGHT
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    #endif  // SILVERLIGHT
        get { return regexInput; }
    }

    #if SILVERLIGHT && !FEATURE_NETCORE
    internal TimeSpan MatchTimeout {
    #else
    public TimeSpan MatchTimeout {
    #endif
    #if SILVERLIGHT
        [SecurityCritical]
    #else  // SILVERLIGHT
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    #endif  // SILVERLIGHT
        get { return matchTimeout; }
    }
} // public class RegexMatchTimeoutException


} // namespace System.Text.RegularExpressions

// file RegexMatchTimeoutException.cs
