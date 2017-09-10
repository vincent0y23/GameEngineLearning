using System;
using System.Runtime.InteropServices;

namespace RunTime.Windows
{
	public struct WNDCLASSEX
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
		public IntPtr hwnd;
		public int message;
		public int wParam;
		public int lParam;
		public int time;
		public POINT pt;
	}
}