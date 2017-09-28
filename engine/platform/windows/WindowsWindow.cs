using System;
using RunTime.Windows.Win32;

namespace RunTime.Windows
{
	public class WindowsWindow
	{
		public delegate IntPtr HandleWndMsgProc(IntPtr IntPtr, int msg, int wParam, int lParam);

		private IntPtr _hInstance;
		private IntPtr _window;
		private HandleWndMsgProc _msgProc;
		private string _className = "WindowsWndClassName";

		public IntPtr Window { get { return _window; } }

		public WindowsWindow(IntPtr hInstance, HandleWndMsgProc msgProcFunc)
		{
			_hInstance = hInstance;
			_msgProc = msgProcFunc;
		}

		public bool Show(string caption, int x, int y, int width, int height)
		{
			WNDCLASSEX windClass = new WNDCLASSEX
			{
				cbSize = User32.SizeOf(typeof(WNDCLASSEX)),
				style = User32.CS_HREDRAW | User32.CS_VREDRAW,
				hbrBackground = (IntPtr)User32.COLOR_WINDOW,
				cbClsExtra = 0,
				cbWndExtra = 0,
				hInstance = _hInstance,
				hIcon = User32.LoadIcon(IntPtr.Zero, User32.IDI_APPLICATION),
				hCursor = User32.LoadCursor(IntPtr.Zero, User32.IDC_ARROW),
				lpszMenuName = null,
				lpszClassName = _className,
				lpfnWndProc = User32.GetFunctionPointerForDelegate(new User32.HandleDefWindowProc(WndProc)),
			};

			var atom = User32.RegisterClassEx(ref windClass);
			if (atom == 0)
			{
				int errMsg = User32.GetLastError();
				User32.MessageBox(IntPtr.Zero, string.Format("register window class err\n msg {0}", errMsg), "", 0);
				return false;
			}

			_window = User32.CreateWindowEx(0, _className, caption, User32.WS_OVERLAPPEDWINDOW, x, y, width, height,
				IntPtr.Zero, IntPtr.Zero, windClass.hInstance, null);
			if (_window == IntPtr.Zero)
			{
				int errMsg = User32.GetLastError();
				User32.MessageBox(IntPtr.Zero, string.Format("create window err\n msg {0}", errMsg), "", 0);
				return false;
			}

			User32.ShowWindow(_window);
			User32.UpdateWindow(_window);

			return true;
		}

		private IntPtr WndProc(IntPtr IntPtr, int msg, int wParam, int lParam)
		{
			if (_msgProc != null)
				return _msgProc(IntPtr, msg, wParam, lParam);
			else 
				return User32.DefWindowProc(IntPtr, msg, wParam, lParam);
		}
	}
}