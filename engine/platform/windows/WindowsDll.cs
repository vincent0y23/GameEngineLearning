using System;
using System.Runtime.InteropServices;

namespace RunTime.Windows
{
	public abstract class WindowsDll : IDisposable
	{
		[DllImport("kernel32")]
		public static extern IntPtr LoadLibrary(string fileName);
		[DllImport("kernel32")]
		public static extern IntPtr GetProcAddress(IntPtr module, string procName);
		[DllImport("kernel32")]
		public static extern int FreeLibrary(IntPtr module);
		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		private IntPtr _handle;
		public IntPtr Handle { get { return _handle; } }

		public WindowsDll(string name)
		{
			_handle = LoadDll(name);
		}

		private IntPtr LoadDll(string name)
		{
			return LoadLibrary(name);
		}

		private void FreeDll(IntPtr handle)
		{
			FreeLibrary(handle);
		}

		protected IntPtr LoadFunction(string name)
		{
			IntPtr functionPtr = GetProcAddress(_handle, name);
			if (functionPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException(string.Format("No function was found with the name {0}.", name));
			}
			return functionPtr;
		}

		protected T LoadFunction<T>(string name)
		{
			IntPtr functionPtr = GetProcAddress(_handle, name);
			if (functionPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException(string.Format("No function was found with the name {0}.", name));
			}

			return Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
		}

		public void Dispose()
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}
			FreeDll(_handle);
		}
	}
}