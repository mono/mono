REM LineNo: 16
REM ExpectedError: BC30610
REM ErrorMessage: Class 'C2' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Property p() As Integer.

REM LineNo: 21
REM ExpectedError: BC31404
REM ErrorMessage: 'Public Shadows Property p() As Integer' cannot shadow a method declared 'MustOverride'.

Imports system

MustInherit Class C1
	public MustOverride Property p() as Integer
End Class


Class C2
	Inherits C1

	private i as integer

	public shadows Property p() as Integer
		SET (ByVal val as Integer)
			i = val
		End SET

		GET
			return i
		END GET
	End Property
End Class


Module M
	Sub Main()
	End Sub

End Module
