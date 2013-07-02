
// Uber-test for object and collection initialization
using System;
using System.Collections.Generic;

public class Test
{
	private class Point
	{
		public int X;
		public int Y;
	}
	private class Line
	{
		public Point P1 = new Point ();
		public Point P2 = new Point ();
	}
	private class Rectangle
	{
		public Line Top = new Line ();
		public Line Right = new Line ();
		public Line Left = new Line ();
		public Line Bottom = new Line ();
	}
	private class Library
	{
		public string Name;
		public string PhoneNumber;
		public List<string>  Books;
		public Library ()
		{
			Books = new List<string> { "Tale of Two Cities", "Catcher in the Rye", "Great Gatsby" };
		}
	}
	private class Thing
	{
		public int Number;
		public string Name;
	}
	private class Box
	{
		public Thing Thing1;
		public Thing Thing2;
	}
	public static int Main ()
	{
		Thing thing1 = new Thing() { Number = 1, Name = "Bob" };
		
		Line line = new Line { P1 = { X = 1, Y = 5 }, P2 = { X = 3, Y = 6 } };
		if (line.P1.X != 1 || line.P1.Y != 5 || line.P2.X != 3 || line.P2.Y != 6)
			return 1;

		Rectangle rectangle = new Rectangle () {
			Top = {
				P1 = { X = 0, Y = 5 },
				P2 = { X = 5, Y = 5 } },
			Bottom = {
				P1 = { X = 0, Y = 0, },
				P2 = { X = 5, Y = 0, }, },
			Right = {
				P1 = { X = 5, Y = 5 },
				P2 = { X = 5, Y = 0 } },
			Left = {
				P1 = { X = 0, Y = 0, },
				P2 = { X = 0, Y = 5 } } };
		if (rectangle.Top.P1.X != 0 || rectangle.Bottom.P2.X != 5 || rectangle.Right.P2.Y != 0 || rectangle.Left.P1.Y != 0)
			return 2;

		List<string> list = new List<string> (3) { "Foo", "Bar", "Baz" };
		if (list[0] != "Foo" || list[1] != "Bar" || list[2] != "Baz")
			return 3;

		Library library = new Library {
			Name = "New York Public Library",
			Books = { "Grapes of Wrath", "Dracula", },
			PhoneNumber = "212-621-0626" };
		if (library.Name != "New York Public Library" || library.PhoneNumber != "212-621-0626" ||
			library.Books[0] != "Tale of Two Cities" ||
			library.Books[1] != "Catcher in the Rye" ||
			library.Books[2] != "Great Gatsby" ||
			library.Books[3] != "Grapes of Wrath" ||
			library.Books[4] != "Dracula")
			return 4;

		Box box = new Box {
			Thing1 = new Thing { Number = 1, Name = "Wilber" },
			Thing2 = new Thing() { Number = 2, Name = "Chris" } };
		if (box.Thing1.Number != 1 || box.Thing1.Name != "Wilber" || box.Thing2.Number != 2 || box.Thing2.Name != "Chris")
			return 5;
		
		Library library2 = new Library { Books = new List<string> { "The Hound of Baskerville", "Flatland", "The Origin of Species" } };
		if (library2.Books[0] != "The Hound of Baskerville" ||
			library2.Books[1] != "Flatland" ||
			library2.Books[2] != "The Origin of Species")
			return 6;

		return 0;
	}
}
