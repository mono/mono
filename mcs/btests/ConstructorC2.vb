REM LineNo: 13
REM ExpectedError: BC30455
REM ErrorMessage: Argument not specified for parameter 'i' of 'Public Sub New(i As String)'.

Class A
	public Sub New(i as String)
	End Sub
End Class

Class B
	Inherits A
	public Sub New()
		Mybase.new()
	End Sub
End Class

Module M
	Sub Main()
	End Sub
End Module
