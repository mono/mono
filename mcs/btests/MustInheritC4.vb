Imports System

'Testing a private mustoverride method
MustInherit Class C1
    Private MustOverride Function F2() As Integer
End Class

Class C2
    Inherits C1
	Private Overrides Function F() As Integer
	End Function
End Class

'Testing a shared mustoverride method 
MustInherit Class C3
              Public Shared MustOverride Sub F()
End Class

Module MustInheritTest
	Sub Main()
		
	End Sub
End Module
