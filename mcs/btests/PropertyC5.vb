REM LineNo: 11
REM ExpectedError: BC30023
REM ErrorMessage: Properties declared 'WriteOnly' cannot have a 'Get'.

Imports system

Module M
	private i as integer

	public WriteOnly Property p() as Integer
		GET
			return i
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET
	End Property

	Sub Main()
	End Sub

End Module
