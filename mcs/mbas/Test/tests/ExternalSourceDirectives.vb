Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/main.aspx",30)
		Console.WriteLine("In main.aspx")
		#End ExternalSource
	End Sub


	Sub A()
		#ExternalSource("/home/a.aspx",23)
		Console.WriteLine("In a.aspx")
		#End ExternalSource		
	End Sub
End Module
