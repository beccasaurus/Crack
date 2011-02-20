using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace MyApp {
	public class Program {
		public static void Main(string[] args) {
			Crack.Run(args);
		}

		[Middleware]
		public static Response Middleware0(Request request, Application app) {
			Console.WriteLine("Hello from middleware - FIRST!");
			return app.Invoke(request);
		}

		[Middleware]
		public static Response Middleware1(Request request, Application app) {
			Console.WriteLine("Hello from middleware");
			return app.Invoke(request);
		}

		[Middleware]
		public static Response Middleware2(Request request, Application app) {
			Console.WriteLine("Hello from a different middleware");
			return app.Invoke(request);
		}

		[Application]
		public static Response InvokeMyApp(Request request) {
			Console.WriteLine("Hello from application");
			return new Response("This is my STDOUT text");
		}
	}
}
