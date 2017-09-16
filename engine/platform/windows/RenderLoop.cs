using System;
using RunTime.Windows.Win32;

namespace RunTime.Windows
{
	public class RenderLoop : IDisposable
	{
		public delegate void RenderCallback();

		private IntPtr _hWnd;
		private bool _isControlAlive;
		
		public RenderLoop(IntPtr hWnd)
		{
			_hWnd = hWnd;
			_isControlAlive = true;
		}

		public bool NextFrame()
		{
			if (_isControlAlive)
			{
				var localHandle = _hWnd;
				if (localHandle != IntPtr.Zero)
				{
					// Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
					MSG msg = new MSG();
					while (User32.PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0) != 0)
					{
						if (User32.GetMessage(ref msg, IntPtr.Zero, 0, 0) == -1)
						{
							throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
								"An error happened in rendering loop while processing windows messages. Error: {0}",
								User32.GetLastError()));
						}

						// NCDESTROY event?
						if (msg.message == User32.WM_NCDESTROY)
							_isControlAlive = false;

						var message = new MSG() { hwnd = msg.hwnd, lParam = msg.lParam, message = msg.message, wParam = msg.wParam };
						//if (!Application.FilterMessage(ref message))
						{
							User32.TranslateMessage(ref msg);
							User32.DispatchMessage(ref msg);
						}
					}
				}

			}

			return _isControlAlive;
		}

		private void ControlDisposed(object sender, EventArgs e)
		{
			_isControlAlive = false;
		}

		public void Dispose()
		{
			_hWnd = IntPtr.Zero;
		}

		public static void Run(IntPtr hWnd, RenderCallback renderCallback)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException("form");

			using (var renderLoop = new RenderLoop(hWnd))
			{
				while (renderLoop.NextFrame())
				{
					renderCallback?.Invoke();
				}
			}
		}

		public static bool IsIdle
		{
			get
			{
				MSG msg = new MSG();
				return User32.PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0) == 0;
			}
		}
	}
}