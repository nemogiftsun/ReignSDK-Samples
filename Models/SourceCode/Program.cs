using Reign.Core;
using System;

namespace Demo_Windows
{
	static class Program
	{
		#if WINDOWS
		[STAThread]
		#elif METRO
		[MTAThread]
		#endif
		static void Main(string[] args)// NOTE: NaCl requires args
		{
			OS.Run(new MainApp(), 0);
		}
	}
}
