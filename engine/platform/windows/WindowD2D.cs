using System;
using System.Threading;

namespace RunTime.Windows
{
	public class WindowD2D
	{
		private User32Window _window;
		public void Run()
		{
			_window = User32.CreateWindowEx(0, "WINDOW", "test", 0, 100, 100, 800, 600, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, null);
			if (_window == IntPtr.Zero)
			{
				uint errMsg = WindowsDll.GetLastError();
				Console.WriteLine(string.Format("create window err.msg {0}", errMsg));
			}
			else
			{
				while (true)
				{
					Thread.Sleep(10);
				}
			}
		}
	}
}