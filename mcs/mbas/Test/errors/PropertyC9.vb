REM LineNo: 10
REM ExpectedError: BC30435
REM ErrorMessage: Members in a Structure cannot be declared 'Protected'.

Imports system

Structure S
	private i as integer

	protected Property p() as Integer
		GET
			return i
		END GET

		SET (val as Integer)
			i = val
		End SET

	End Property
End Structure

Module M
	Sub Main()
	End Sub

End Module
