REM LineNo: 14
REM ExpectedError: BC31064
REM ErrorMessage: 'Set' parameter must have the same type as the containing property.

Imports system

Module M
	private i as integer

	public Property p() as Integer
		GET
		END GET

		SET (ByVal val as string)
		End SET

	End Property

	Sub Main()
	End Sub

End Module
