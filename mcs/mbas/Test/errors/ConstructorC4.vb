REM LineNo: 8
REM ExpectedError: BC30480
REM ErrorMessage: Shared 'Sub New' cannot be declared 'Public'.

Imports System

Class A
	Public Shared Sub New()
		Console.WriteLine("Shared ctor")
	End Sub
End Class

Module M
	Sub Main()
	End Sub
End Module
