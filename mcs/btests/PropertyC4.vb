REM LineNo: 10
REM ExpectedError: BC30124
REM ErrorMessage: Property without a 'ReadOnly' or 'WriteOnly' specifier must provide both a 'Get' and a 'Set'.

Imports system

Module M
	private i as integer

	public Property p() as Integer
		'GET
		'	return i
		'END GET

		SET (ByVal val as Integer)
			i = val
		End SET
	End Property

	Sub Main()
	End Sub

End Module
