REM LineNo: 10
REM ExpectedError: BC30503
REM ErrorMessage: Properties in a Module cannot be declared 'Protected'.

Imports system

Module M
	private i as integer

	protected Property p() as Integer
		GET
			return i
		END GET

		SET (val as Integer)
			i = val
		End SET

	End Property


	Sub Main()
	End Sub

End Module
