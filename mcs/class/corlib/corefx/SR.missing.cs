partial class SR
{
	public const string Arg_InvalidComObjectException = "Attempt has been made to use a COM object that does not have a backing class factory.";
	public const string Arg_InvalidOleVariantTypeException = "Specified OLE variant was invalid.";
	public const string Arg_SafeArrayRankMismatchException = "Specified array was not of the expected rank.";
	public const string Arg_SafeArrayTypeMismatchException = "Specified array was not of the expected type.";
    public const string TypeNotDelegate = "'Type '{0}' is not a delegate type.  EventTokenTable may only be used with delegate types.'";
    public const string InvalidOperationException_ActorGraphCircular = "An Actor must not create a circular reference between itself (or one of its child Actors) and one of its parents.";
    public const string InvalidOperation_ClaimCannotBeRemoved = "The Claim '{0}' was not able to be removed.  It is either not part of this Identity or it is a Claim that is owned by the Principal that contains this Identity. For example, the Principal will own the Claim when creating a GenericPrincipal with roles. The roles will be exposed through the Identity that is passed in the constructor, but not actually owned by the Identity.  Similar logic exists for a RolePrincipal.";
    public const string PlatformNotSupported_Serialization = "This instance contains state that cannot be serialized and deserialized on this platform.";
    public const string PrivilegeNotHeld_Default = "The process does not possess some privilege required for this operation.";
    public const string PrivilegeNotHeld_Named = "The process does not possess the '{0}' privilege which is required for this operation.";
}