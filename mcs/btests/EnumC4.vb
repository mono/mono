Imports System

Module M
	Public Enum E1 As Long
		A = B
		B
	End Enum

   Sub Main()
    End Sub
End Module

'C:\btests\EnumC4.vb(6) : error BC30500: Constant 'A' cannot depend on its own value.- B
