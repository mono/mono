REM LineNo: 4
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'Delegate_dll'.

Imports System

NameSpace NSDelegate
	Public Class C
		public Delegate Sub SD()

		Public Sub S
			'Console.WriteLine("S - got called")
		End Sub

		Public Sub callSD(d as SD)
			'Console.WriteLine("SD - got called")
			d.Invoke()
		End Sub
	End Class
End NameSpace
