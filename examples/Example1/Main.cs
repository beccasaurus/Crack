using System;
using ConsoleRack;

namespace Example1 {

	public class MainClass {

		public static void Main(string[] args) { Crack.Run(args); }

		[Application]
		public static Response MyApp(Request req) {
			return new Response("Hello from MyApp!  You passed: {0}", string.Join(", ", req.Arguments));
		}

		[Middleware]
		public static Response Version(Request req, Application app) {
			// ofcourse this could use an option parsing library
			if (req.Arguments.Length > 0)
				if (req.Arguments[0] == "-v" || req.Arguments[0] == "--version")
					return new Response("MyApp version 1.0.5.9");

			return app.Invoke(req);
		}

		[Middleware(Last = true)]
		public static Response AddHeaderAndFooter(Request req, Application app) {
			var header = "[My App]\n==========\n";
			var footer = "==========\nCopyright (c) 2010 Some Cool Guys, Inc.\n";
			return app.Invoke(req).Prepend(header).Append(footer);
		}
	}
}

