// CS0030: Cannot convert type `Position' to `Board.Stone'
// Line: 20

using System;
using System.Collections;

public class Position {
}

public class Board {
    public enum Stone : int {
	None = 0,
	    Empty = 1,
	    Black = 2,
	    White = 3
    }

    public Stone Get(Position p)
    {
	return (Stone)p;
    }

    public static void Main() {
    }
    
}






