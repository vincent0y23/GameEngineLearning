using System;
using RunTime.Windows;

namespace RunTime
{
	public class Engine
	{
		public void Run()
		{
			Console.WriteLine("engine run ");
			HelloEngineWin window = new HelloEngineWin();
			window.Show();
			RenderLoop.Run(window.Window, null);
			Console.WriteLine("end");
		}
	}
}