using System;
using RunTime.Windows;

namespace RunTime
{
	public class Engine
	{
		public void Run()
		{
			Console.WriteLine("engine run ");
			int width = 800, height = 600;
			HelloEngineWin window = new HelloEngineWin();
			window.Show(100, 100, width, height);
			using (var app = new HelloEngineD3D12Tri())
			{
				app.Initialize(window.Window, width, height);
				using (var loop = new RenderLoop(window.Window))
				{
					while(loop.NextFrame())
					{
						app.Render();
					}
				}
			}
			Console.WriteLine("end");
		}
	}
}