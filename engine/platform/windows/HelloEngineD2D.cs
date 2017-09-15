using System;
using System.Diagnostics;
using SharpDX.Direct2D1;
using RunTime.Windows.Win32;

namespace RunTime.Windows
{
	public class HelloEngineD2D
	{
		private IntPtr _window;
		private MSG _tempMsg;
		private string _className = "myclass";

		//SharpDX.DXGI.Surface _surface;
		Factory _factory;
		RenderTarget _renderTarget;
		Brush _lightSlateGrayBrush;
		Brush _cornflowerBlueBrush;

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

			while (User32.GetMessage(ref _tempMsg, IntPtr.Zero, 0, 0) != 0)
			{
				User32.TranslateMessage(ref _tempMsg);
				User32.DispatchMessage(ref _tempMsg);
			}
		}

		private IntPtr myWndProc(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			switch (msg)
			{
				case User32.WM_CREATE:
					_factory = new Factory(FactoryType.SingleThreaded);
					break;
				case User32.WM_PAINT:
					{
						RECT rc = new RECT();
						User32.GetClientRect(hWnd, ref rc);
						CreateGraphicsResources(hWnd, rc.right - rc.left, rc.bottom - rc.top);
						_renderTarget.BeginDraw();
						_renderTarget.Clear(SharpDX.Color.White);

						SharpDX.Size2F rtSize = _renderTarget.Size;
						for (int x = 0; x < rtSize.Width; x += 10)
						{
							_renderTarget.DrawLine(new SharpDX.Vector2(x, 0f), new SharpDX.Vector2(x, rtSize.Height), _lightSlateGrayBrush, 0.5f);
						}

						for (int y = 0; y < rtSize.Height; y += 10)
						{
							_renderTarget.DrawLine(new SharpDX.Vector2(0f, y), new SharpDX.Vector2(rtSize.Width, y), _cornflowerBlueBrush, 0.5f);
						}

						SharpDX.RectangleF rect0 = new SharpDX.RectangleF(rtSize.Width / 2f - 50f, rtSize.Height / 2f - 50f, 100f, 100f);
						SharpDX.RectangleF rect1 = new SharpDX.RectangleF(rtSize.Width / 2f - 100f, rtSize.Height / 2f - 100f, 200f, 200f);
						_renderTarget.FillRectangle(rect0, _lightSlateGrayBrush);
						_renderTarget.DrawRectangle(rect1, _cornflowerBlueBrush);

						_renderTarget.EndDraw();
					}
					break;
				case User32.WM_SIZE:
					{
						RECT rc = new RECT();
						User32.GetClientRect(hWnd, ref rc);
						DestoryResources();
						CreateGraphicsResources(hWnd, rc.right - rc.left, rc.bottom - rc.top);
					}
					break;
				//case User32.WM_DISPLAYCHANGE:
				//	{
				//		RECT rc = new RECT();
				//		User32.InvalidateRect(hWnd, ref rc, 0);
				//	}
				//	break;
				case User32.WM_DESTROY:
					DestoryResources();
					User32.PostQuitMessage(0);
					break;
				default:
					break;
			}
			return User32.DefWindowProc(hWnd, msg, wParam, lParam);
		}

		private void CreateGraphicsResources(IntPtr hWnd, int width, int height)
		{
			if (_renderTarget == null)
			{
				PixelFormat pixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore);

				HwndRenderTargetProperties hwndRenderTargetProperties = new HwndRenderTargetProperties();
				hwndRenderTargetProperties.Hwnd = hWnd;
				hwndRenderTargetProperties.PixelSize = new SharpDX.Size2(width, height);
				hwndRenderTargetProperties.PresentOptions = PresentOptions.None;

				RenderTargetProperties renderTargetProperties = new RenderTargetProperties(
					RenderTargetType.Hardware,
					pixelFormat, 0, 0, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);

				_renderTarget = new WindowRenderTarget(_factory,
					renderTargetProperties,
					hwndRenderTargetProperties);

				_lightSlateGrayBrush = new SolidColorBrush(_renderTarget, SharpDX.Color.LightSlateGray);
				_cornflowerBlueBrush = new SolidColorBrush(_renderTarget, SharpDX.Color.CornflowerBlue);
			}
		}

		private void DestoryResources()
		{
			if (_renderTarget != null)
				_renderTarget.Dispose();
			if (_lightSlateGrayBrush != null)
				_lightSlateGrayBrush.Dispose();
			if (_cornflowerBlueBrush != null)
				_cornflowerBlueBrush.Dispose();
			_renderTarget = null;
			_lightSlateGrayBrush = null;
			_cornflowerBlueBrush = null;
		}
	}
}