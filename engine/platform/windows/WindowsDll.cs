using System;
using System.Runtime.InteropServices;

namespace RunTime
{
	public class WindowsDll : IDisposable
	{
		[DllImport("kernel32")]
		public static extern IntPtr LoadLibrary(string fileName);
		[DllImport("kernel32")]
		public static extern IntPtr GetProcAddress(IntPtr module, string procName);
		[DllImport("kernel32")]
		public static extern int FreeLibrary(IntPtr module);

		private IntPtr _handle;
		public IntPtr Handle { get { return _handle; } }

		public WindowsDll(string name)
		{
			LoadDll(name);
		}

		protected IntPtr LoadDll(string name)
		{
			return LoadLibrary(name);
		}

		protected void FreeDll(IntPtr handle)
		{
			FreeLibrary(handle);
		}

		protected IntPtr LoadFunction(IntPtr handle, string functionName)
		{
			return GetProcAddress(handle, functionName);
		}

		public T LoadFunction<T>(string name)
		{
			IntPtr functionPtr = LoadFunction(_handle, name);
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