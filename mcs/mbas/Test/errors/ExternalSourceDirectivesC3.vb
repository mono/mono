REM LineNo: 15
REM ExpectedError: BC30578
REM ErrorMessage: '#End ExternalSource' must be preceded by a matching '#ExternalSource'.



Imports System
Module ExternalSourceDirective
	Sub Main()
			#ExternalSource("/home/a.aspx", 1024)
			Console.WriteLine("In a.aspx")
			#End ExternalSource

			Console.WriteLine(“In test.aspx”)
			#End ExternalSource
	End Sub
End Module
