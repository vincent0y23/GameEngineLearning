using System;
using System.IO;
using System.Diagnostics;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using RunTime.Windows.Win32;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace RunTime.Windows
{
	public class HelloEngineD3D
	{
		private IntPtr _window;
		private MSG _tempMsg;
		private string _className = "myclass";

		private string _shaderFilePath;
		private Device _device;
		private DeviceContext _deviceContext;
		private SwapChain _swapChain;
		private RenderTargetView _renderTargetView;
		private VertexShader _vertexShader;
		private PixelShader _pixelShader;
		private InputLayout _layout;
		private Buffer _vertexBuffer;

		public void Run()
		{
			if (!CreateShaderPath())
				return;

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
					break;
				case User32.WM_PAINT:
					RECT rc = new RECT();
					User32.GetClientRect(hWnd, ref rc);
					CreateGraphicsResources(hWnd, rc.right - rc.left, rc.bottom - rc.top);
					// clear the back buffer to a deep black
					_deviceContext.ClearRenderTargetView(_renderTargetView, Color.Black);
					// do 3D rendering on the back buffer here
					{
						// select which vertex buffer to display
						//UINT stride = sizeof(VERTEX);
						//UINT offset = 0;
						//g_pDevcon->IASetVertexBuffers(0, 1, &g_pVBuffer, &stride, &offset);

						// select which primtive type we are using
						//g_pDevcon->IASetPrimitiveTopology(D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

						// draw the vertex buffer to the back buffer
						_deviceContext.Draw(3, 0);
					}
					// swap the back buffer and the front buffer
					_swapChain.Present(0, PresentFlags.None);
					break;
				case User32.WM_SIZE:
					if (_swapChain != null)
					{
						DestoryResources();
					}
					break;
				//case User32.WM_DISPLAYCHANGE:
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

		private bool CreateShaderPath()
		{
			string dir = AppDomain.CurrentDomain.BaseDirectory;
			_shaderFilePath = Path.Combine(dir, "MiniTri.fx");
			return File.Exists(_shaderFilePath);
		}

		private void CreateGraphicsResources(IntPtr hWnd, int width, int height)
		{
			if (_swapChain == null)
			{
				SwapChainDescription swapChainDesc = new SwapChainDescription()
				{
					BufferCount = 1,
					Usage = Usage.RenderTargetOutput,
					OutputHandle = hWnd,
					IsWindowed = true,
					Flags = SwapChainFlags.AllowModeSwitch
				};
				swapChainDesc.ModeDescription = new ModeDescription()
				{
					Width = width,
					Height = height,
					Format = Format.R8G8B8A8_UNorm,
					RefreshRate = new Rational()
					{
						Numerator = 60,
						Denominator = 1,
					},
				};
				swapChainDesc.SampleDescription = new SampleDescription()
				{
					Count = 4,
				};

				Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out _device, out _swapChain);
				_deviceContext = _device.ImmediateContext;

				// CreateRenderTarget
				var texture2D = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(_swapChain, 0);
				_renderTargetView = new RenderTargetView(_device, texture2D);
				_deviceContext.OutputMerger.SetTargets(_renderTargetView);
				texture2D.Dispose();

				// SetViewPort
				Viewport viewPort = new Viewport(0, 0, width, height, 0f, 1f);
				_deviceContext.Rasterizer.SetViewport(viewPort);

				// InitPipeline  loads and prepares the shaders
				// load and compile the two shaders
				var vertexShaderByteCode = ShaderBytecode.CompileFromFile(_shaderFilePath, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
				_vertexShader = new VertexShader(_device, vertexShaderByteCode);
				var pixelShaderByteCode = ShaderBytecode.CompileFromFile(_shaderFilePath, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
				_pixelShader = new PixelShader(_device, pixelShaderByteCode);
				// set the shader objects
				_deviceContext.VertexShader.Set(_vertexShader);
				_deviceContext.PixelShader.Set(_pixelShader);
				// Layout from VertexShader input signature
				InputElement[] idDesc = new InputElement[] {
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
					new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
				};
				_layout = new InputLayout(_device, new ShaderSignature(vertexShaderByteCode), idDesc);
				_deviceContext.InputAssembler.InputLayout = _layout;
				_deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

				// InitGraphics  creates the shape to render
				Vector4[] vertices = new[]
								  {
									  new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
									  new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
									  new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
								  };
				BufferDescription bufferDesc = new BufferDescription()
				{
					SizeInBytes = Vector4.SizeInBytes * vertices.Length,
					Usage = ResourceUsage.Dynamic,
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write
				};
				_vertexBuffer = Buffer.Create(_device, vertices, bufferDesc);
				//var _vertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);
				_deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, 32, 0));
				//_deviceContext.MapSubresource()

				vertexShaderByteCode.Dispose();
				pixelShaderByteCode.Dispose();
			}
		}

		private void DestoryResources()
		{
			_layout.Dispose();
			_vertexShader.Dispose();
			_pixelShader.Dispose();
			_vertexBuffer.Dispose();
			_swapChain.Dispose();
			_renderTargetView.Dispose();
			_device.Dispose();
			_deviceContext.Dispose();

			_layout = null;
			_vertexShader = null;
			_pixelShader = null;
			_vertexBuffer = null;
			_swapChain = null;
			_renderTargetView = null;
			_device = null;
			_deviceContext = null;
		}
	}
}