using System;
using System.Runtime.InteropServices;

namespace RunTime.Windows.Win32
{
	public abstract class WindowsDll : IDisposable
	{
		private IntPtr _IntPtr;
		public IntPtr IntPtr { get { return _IntPtr; } }

		public WindowsDll(string name)
		{
			_IntPtr = LoadDll(name);
		}

		private IntPtr LoadDll(string name)
		{
			return Kernel32.LoadLibrary(name);
		}

		public void Dispose()
		{
			if (_IntPtr != IntPtr.Zero)
				Kernel32.FreeLibrary(_IntPtr);
		}

		protected T LoadFunction<T>(string name)
		{
			IntPtr functionPtr = Kernel32.GetProcAddress(_IntPtr, name);
			if (functionPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException(string.Format("No function was found with the name {0}.", name));
			}

			return Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
		}
	}
}