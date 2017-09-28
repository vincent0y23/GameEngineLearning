using System;
using System.Threading;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using RunTime.Windows.Win32;

using Device = SharpDX.Direct3D11.Device;

namespace RunTime.Windows
{
	public abstract class D3DApp : IDisposable
	{
		private IntPtr _hInstance;
		private WindowsWindow _window;
		private int _width = 800;
		private int _height = 600;

		private bool _isPaused = false;
		private bool _isResizing = false;
		private bool _isMinimized = false;
		private bool _isMaximized = false;
		private Timer _timer = new Timer();
		private int _frameCount;
		private float _timeElapsed = 0f;

		// d3d setting
		private bool _enable4xMsaa = true;
		private int _massQuality;

		// d3d com object
		private Device _device;
		private DeviceContext _deviceContext;
		private SwapChain _swapChain;
		private RenderTargetView _renderTargetView;
		private DepthStencilView _depthStencilView;
		private Texture2D _depthStencilBuffer;

		public D3DApp()
		{
			_hInstance = System.Diagnostics.Process.GetCurrentProcess().Handle;
		}

		public void Dispose()
		{
			ReleaseComObj(_renderTargetView);
			ReleaseComObj(_depthStencilView);
			ReleaseComObj(_swapChain);
			ReleaseComObj(_depthStencilBuffer);

			if (_deviceContext != null)
				_deviceContext.ClearState();

			ReleaseComObj(_deviceContext);
			ReleaseComObj(_device);
		}
		public int Run()
		{
			MSG msg = new MSG();
			_timer.Reset();
			while (msg.message != User32.WM_QUIT)
			{
				if (User32.PeekMessage(ref msg, IntPtr.Zero, 0, 0, User32.PM_REMOVE) != 0)
				{
					User32.TranslateMessage(ref msg);
					User32.DispatchMessage(ref msg);
				}
				else
				{
					_timer.Tick();
					if (!_isPaused)
					{
						CalculateFrameStats();
						UpdateScence(_timer.DelataTime);
						DrawScene();
					}
					else
					{
						Thread.Sleep(100);
					}
				}
			}

			return msg.wParam;
		}

		public virtual bool Init()
		{
			if (!InitMainWindow())
				return false;
			if (!InitDirect3D())
				return false;
			return true;
		}
		public virtual void OnResize()
		{
			ReleaseComObj(_renderTargetView);
			ReleaseComObj(_depthStencilView);
			ReleaseComObj(_depthStencilBuffer);

			_swapChain.ResizeBuffers(1, _width, _height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

			// create renderTargetView
			using (Texture2D texture2D = _swapChain.GetBackBuffer<Texture2D>(0))
			{
				_renderTargetView = new RenderTargetView(_device, texture2D);
			}

			// create depth/stencil buffer
			Texture2DDescription texture2DDesc = new Texture2DDescription()
			{
				Width = _width,
				Height = _height,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.D24_UNorm_S8_UInt,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
			};

			if (_enable4xMsaa)
			{
				texture2DDesc.SampleDescription = new SampleDescription()
				{
					Count = 4,
					Quality = _massQuality - 1
				};
			}
			else
			{
				texture2DDesc.SampleDescription = new SampleDescription()
				{
					Count = 1,
					Quality = 0
				};
			}

			_depthStencilBuffer = new Texture2D(_device, texture2DDesc);
			_depthStencilView = new DepthStencilView(_device, _depthStencilBuffer);

			// bind the views to the output merger stage
			_deviceContext.OutputMerger.SetRenderTargets(_depthStencilView, _renderTargetView);

			// set the viewport
			Viewport viewPort = new Viewport()
			{
				X = 0,
				Y = 0,
				Width = _width,
				Height = _height,
				MinDepth = 0f,
				MaxDepth = 1f
			};
			_deviceContext.Rasterizer.SetViewport(viewPort);
		}
		public abstract void UpdateScence(float deltaTime);
		public abstract void DrawScene();

		protected virtual IntPtr WndProc(IntPtr IntPtr, int msg, int wParam, int lParam)
		{
			switch (msg)
			{
				case User32.WM_ACTIVATE:
					if (User32.LOWORD(wParam) == User32.WA_INACTIVE)
					{
						_isPaused = true;
						_timer.Stop();
					}
					else
					{
						_isPaused = false;
						_timer.Start();
					}
					return IntPtr.Zero;
				case User32.WM_SIZE:
					_width = User32.LOWORD(lParam);
					_height = User32.HIWORD(lParam);
					if (_device != null)
					{
						if (wParam == User32.SIZE_MINIMIZED)
						{
							_isPaused = true;
							_isMinimized = true;
							_isMaximized = false;
						}
						else if (wParam == User32.SIZE_MAXIMIZED)
						{
							_isPaused = false;
							_isMinimized = false;
							_isMaximized = true;
							OnResize();
						}
						else if (wParam == User32.SIZE_RESTORED)
						{
							if (_isMinimized)
							{
								_isPaused = false;
								_isMinimized = false;
								OnResize();
							}
							else if (_isMaximized)
							{
								_isPaused = false;
								_isMaximized = false;
								OnResize();
							}
							else if (_isResizing)
							{ }
							else
							{
								OnResize();
							}
						}
					}
					return IntPtr.Zero;
				case User32.WM_ENTERSIZEMOVE:
					_isPaused = true;
					_isResizing = true;
					_timer.Stop();
					return IntPtr.Zero;
				case User32.WM_EXITSIZEMOVE:
					_isPaused = false;
					_isResizing = false;
					_timer.Start();
					OnResize();
					return IntPtr.Zero;
				case User32.WM_DESTROY:
					User32.PostQuitMessage(0);
					return IntPtr.Zero;
				case User32.WM_MENUCHAR:
					return User32.MakeLResult(0, 1);
				//case User32.WM_GETMINMAXINFO:
				case User32.WM_LBUTTONDOWN:
				case User32.WM_MBUTTONDOWN:
				case User32.WM_RBUTTONDOWN:
					OnMouseDown(wParam, User32.GET_X_LPARAM(lParam), User32.GET_Y_LPARAM(lParam));
					return IntPtr.Zero;
				case User32.WM_LBUTTONUP:
				case User32.WM_MBUTTONUP:
				case User32.WM_RBUTTONUP:
					OnMouseUp(wParam, User32.GET_X_LPARAM(lParam), User32.GET_Y_LPARAM(lParam));
					return IntPtr.Zero;
				case User32.WM_MOUSEMOVE:
					OnMouseMove(wParam, User32.GET_X_LPARAM(lParam), User32.GET_Y_LPARAM(lParam));
					return IntPtr.Zero;
			}

			return User32.DefWindowProc(IntPtr, msg, wParam, lParam);
		}
		protected virtual void OnMouseDown(int btnState, int x, int y) { }
		protected virtual void OnMouseUp(int btnState, int x, int y) { }
		protected virtual void OnMouseMove(int btnState, int x, int y) { }

		protected bool InitMainWindow()
		{
			_window = new WindowsWindow(_hInstance, WndProc);
			return _window.Show("D3DApp", 100, 100, _width, _height);
		}
		protected bool InitDirect3D()
		{
			DeviceCreationFlags createDeviceFlags = DeviceCreationFlags.None;
#if DEBUG
			createDeviceFlags |= DeviceCreationFlags.Debug;
#endif

			// create device
			_device = new Device(null, createDeviceFlags);
			if (_device.NativePointer == IntPtr.Zero)
			{
				User32.MessageBox(IntPtr.Zero, "Create d3dDevice failed.", "", 0);
				return false;
			}
			if (_device.FeatureLevel != FeatureLevel.Level_11_0)
			{
				User32.MessageBox(IntPtr.Zero, "Direct3D Feature Level 11 unsupported.", "", 0);
				return false;
			}

			_deviceContext = _device.ImmediateContext;

			_massQuality = _device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4);
			if (_massQuality <= 0)
				return false;

			// create swapchain
			SwapChainDescription swapChainDesc = new SwapChainDescription()
			{
				BufferCount = 1,
				Usage = Usage.RenderTargetOutput,
				OutputHandle = _window.Window,
				IsWindowed = true,
				Flags = SwapChainFlags.None,
				SwapEffect = SwapEffect.Discard
			};

			swapChainDesc.ModeDescription = new ModeDescription()
			{
				Width = _width,
				Height = _height,
				Format = Format.R8G8B8A8_UNorm,
				ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
				Scaling = DisplayModeScaling.Unspecified,
				RefreshRate = new Rational()
				{
					Numerator = 60,
					Denominator = 1,
				},
			};

			if (_enable4xMsaa)
			{
				swapChainDesc.SampleDescription = new SampleDescription()
				{
					Count = 4,
					Quality = _massQuality - 1,
				};
			}
			else
			{
				swapChainDesc.SampleDescription = new SampleDescription()
				{
					Count = 1,
					Quality = 0,
				};
			}

			IntPtr dxgiDeviceNativePtr;
			_device.QueryInterface(typeof(SharpDX.DXGI.Device).GUID, out dxgiDeviceNativePtr);
			using (SharpDX.DXGI.Device dxgiDevice = new SharpDX.DXGI.Device(dxgiDeviceNativePtr))
			{
				IntPtr dxgiAdapterNativePtr;
				dxgiDevice.GetParent(typeof(SharpDX.DXGI.Adapter).GUID, out dxgiAdapterNativePtr);
				using (SharpDX.DXGI.Adapter dxgiAdapter = new Adapter(dxgiAdapterNativePtr))
				{
					IntPtr dxgiFactoryNativePtr;
					dxgiAdapter.GetParent(typeof(SharpDX.DXGI.Factory).GUID, out dxgiFactoryNativePtr);
					using (SharpDX.DXGI.Factory dxgiFactory = new Factory(dxgiFactoryNativePtr))
					{
						_swapChain = new SwapChain(dxgiFactory, _device, swapChainDesc);
					}
				}
			}

			OnResize();
			return true;
		}
		protected void CalculateFrameStats()
		{
			_frameCount++;
			if ((_timer.TotalTime() - _timeElapsed) >= 1f)
			{
				string str = string.Format("    FPS: {0}    Frame Time: {1} (ms)", _frameCount, 1000f / _frameCount);
				User32.SetWindowText(_window.Window, str);

				_frameCount = 0;
				_timeElapsed += 1f;
			}
		}
		protected void ReleaseComObj<T>(T obj) where T : ComObject
		{
			if (obj != null)
				obj.Dispose();
			obj = null;
		}
	}
}
