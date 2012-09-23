using System;
using Reign.Core;

namespace Demo_Windows
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			OS.Run(new MainApp(), 0);
		}
	}
}
