using System;
using Reign.Core;

namespace Demo
{
	class Program
	{
		#if WIN32
		[STAThread]
		#endif
		#if WINRT
		[MTAThread]
		#endif
		static void Main(string[] args)
		{
			 OS.Run(new MainApp(), 60);
		}
	}
}
