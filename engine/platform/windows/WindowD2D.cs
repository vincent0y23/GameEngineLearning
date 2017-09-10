using System;
using System.Diagnostics;
using RunTime.Windows.Win32;

namespace RunTime.Windows
{
	public class WindowD2D
	{
		private IntPtr _window;
		private MSG _tempMsg;
		private string _className = "myclass";

		public void Run()
		{
			WNDCLASSEX windClass = new WNDCLASSEX
			{
				cbSize = User32.SizeOf(typeof(WNDCLASSEX)),
				style = User32.CS_HREDRAW | User32.CS_VREDRAW,
				hbrBackground = (IntPtr)User32.COLOR_WINDOW,
				cbClsExtra = 0,
				cbWndExtra = 0,
				hInstance = Process.GetCurrentProcess().Handle,
				hIcon = IntPtr.Zero,
				hCursor = User32.LoadCursor(IntPtr.Zero, User32.IDC_ARROW),
				lpszMenuName = null,
				lpszClassName = _className,
				lpfnWndProc = User32.GetFunctionPointerForDelegate(new User32.HandleDefWindowProc(myWndProc)),
				hIconSm = IntPtr.Zero,
			};

			var atom = User32.RegisterClassEx(ref windClass);
			if (atom == 0)
			{
				int errMsg = User32.GetLastError();
				Console.WriteLine(string.Format("register class err.msg {0}", errMsg));
				return;
			}

			_window = User32.CreateWindowEx(0, _className, "test", User32.WS_OVERLAPPEDWINDOW, 100, 100, 800, 600, 
				IntPtr.Zero, IntPtr.Zero, windClass.hInstance, null);
			if (_window == IntPtr.Zero)
			{
				int errMsg = User32.GetLastError();
				Console.WriteLine(string.Format("create window err.msg {0}", errMsg));
				return;
			}

			User32.ShowWindow(_window);

			while (User32.GetMessage(ref _tempMsg, IntPtr.Zero, 0, 0) > 0)
			{
				User32.TranslateMessage(ref _tempMsg);
				User32.DispatchMessage(ref _tempMsg);
			}
		}

		private IntPtr myWndProc(IntPtr IntPtr, int msg, int wParam, int lParam)
		{
			switch (msg)
			{
				// All GUI painting must be done here
				case User32.WM_PAINT:
					break;

				case User32.WM_LBUTTONDBLCLK:
					break;

				case User32.WM_DESTROY:

					//If you want to shutdown the application, call the next function instead of DestroyWindow
					//PostQuitMessage(0);
					break;

				default:
					break;
			}
			return User32.DefWindowProc(IntPtr, msg, wParam, lParam);
		}
	}
}