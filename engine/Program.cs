using System;
using RunTime.Windows;

namespace RunTime
{
	public class Engine
	{
		public void Run()
		{
			Console.WriteLine("engine run ");
			WindowD2D window = new WindowD2D();
			window.Run();
			Console.WriteLine("end");
		}
	}
}