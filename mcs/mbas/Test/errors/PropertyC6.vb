REM LineNo: 15
REM ExpectedError: BC30022
REM ErrorMessage: Properties declared 'ReadOnly' cannot have a 'Set'.

Imports system

Module M
	private i as integer

	public ReadOnly Property p() as Integer
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
