Imports System

Class A
	Shared Sub New()
	End Sub
End Class

Class B
	Inherits A

	Shared Sub New()
		Me.New()
	End Sub
End Class


Module M
	Sub Main()
	End Sub
End Module
