REM LineNo: 17
REM ExpectedError: BC30579
REM ErrorMessage: '#ExternalSource' statement must end with a matching '#End ExternalSource'.



Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/a.aspx",30)
		Console.WriteLine("In a.aspx")
		#End ExternalSource

		#ExternalSource("/home/index.aspx", 1024)
		Console.WriteLine("In index.aspx")
	End Sub
End Module
