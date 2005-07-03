using System;
using System.Collections;
using System.Windows;
using System.Windows.Serialization;

namespace Xaml.TestVocab.Console {
	public class ConsoleApp : IAddChild {
		private ArrayList actions = new ArrayList();
		public void AddText(string Text)
		{
			actions.Add(new ConsoleWriter(Text));
		}

		public void AddChild(object Value)
		{
			if (Value is IConsoleAction)
				actions.Add(Value);
			else
				throw new Exception(Value.ToString() + " is not a console action");
		}

		public void Run()
		{
			foreach (IConsoleAction action in actions) {
				int reps = GetRepetitions((DependencyObject)action);
				for (int i = 0; i < reps; i++)
					action.Run();
			}
		}


		public static readonly DependencyProperty RepetitionsProperty = DependencyProperty.RegisterAttached("Repetitions", typeof(int), typeof(ConsoleApp));
		public static void SetRepetitions(DependencyObject obj, int number)
		{
			obj.SetValue(RepetitionsProperty, number);
		}
		public static int GetRepetitions(DependencyObject d)
		{
			object v = d.GetValue(RepetitionsProperty);
			return (v == null ? 1 : (int)v);
		}
	}
}
