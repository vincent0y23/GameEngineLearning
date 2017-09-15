using System;
using RunTime.Windows;

namespace RunTime
{
	public class Engine
	{
		public void Run()
		{
			Console.WriteLine("engine run ");
			HelloEngineD3D window = new HelloEngineD3D();
			window.Run();
			Console.WriteLine("end");
		}
	}
}