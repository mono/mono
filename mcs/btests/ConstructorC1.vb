REM LineNo: 6
REM ExpectedError: BC30297
REM ErrorMessage: Constructor 'Public Sub New(a As String)' cannot call itself:

Class A
	public Sub New(a as string)
		Me.New("aaa")
	End Sub
End Class

Module M
	Sub Main()
	End Sub
End Module

