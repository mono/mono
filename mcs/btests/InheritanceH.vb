Imports System

'Testing a class that inherits from an inner class of another class that inherits from the first class
Class C1
    Inherits C2.C3
End Class

Class C2
    Inherits C1
        Class C3
        End Class
End Class

'Testing a class that inherits from its inner class
                                                                                                                             
Class C4
    Inherits C4.C5
        Class C5
        End Class
End Class


Module Inheritance
	Sub Main()
	End Sub
End Module
