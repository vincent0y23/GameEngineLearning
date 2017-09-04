using System;
using System.Runtime.InteropServices;

using HANDLE = System.IntPtr;
using HWND = System.IntPtr;
using HDC = System.IntPtr;

namespace RunTime.Windows
{
	public struct User32Window
	{
		public readonly IntPtr Handle;

		public User32Window(IntPtr pointer)
		{
			Handle = pointer;
		}

		public static implicit operator IntPtr(User32Window window) { return window.Handle; }
		public static implicit operator User32Window(IntPtr pointer) { return new User32Window(pointer); }
	}

	struct WNDCLASSEX
	{
		[MarshalAs(UnmanagedType.U4)]
		public int cbSize;
		[MarshalAs(UnmanagedType.U4)]
		public int style;
		public IntPtr lpfnWndProc; // not WndProc
		public int cbClsExtra;
		public int cbWndExtra;
		public IntPtr hInstance;
		public IntPtr hIcon;
		public IntPtr hCursor;
		public IntPtr hbrBackground;
		public string lpszMenuName;
		public string lpszClassName;
		public IntPtr hIconSm;
	}

	public struct POINT 
	{
		public int x;
		public int y;
	}

	public struct MSG 
	{
		public HWND hwnd;
		public int message;
		public int wParam;
		public int lParam;
		public int time;
		public POINT pt;
	}
}