using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ConsoleRack;

/// <summary>Primary namespace for Console Rack (Crack)</summary>
namespace ConsoleRack {
	
	/// <summary>Main Crack class for things like running a Crack application, finding Middleware, etc</summary>
	public class Crack {

		static List<Middleware> _middlewares;

		public static List<Middleware> Middlewares {
			get {
				if (_middlewares == null) _middlewares = Middleware.From(Assembly.GetCallingAssembly());
				return _middlewares;
			}
			set { _middlewares = value; }
		}

		/// <summary>Runs all Crack.Middleware using the provided arguments</summary>
		/// <remarks>
		/// If Crack.Middleware has not been set, we look for all [Middleware] in the calling assembly.
		/// </remarks>
		public static void Run(string[] args) {
			var middleware = Crack.Middlewares.FirstOrDefault();

			if (middleware == null)
				throw new Exception("There are no middleware to invoke");
			else
				middleware.Invoke(new Request(args)).Execute();
		}

		/// <summary>Returns a list of all public, static MethodInfo found in the given assembly that have the given attribute type</summary>
		public static List<MethodInfo> GetMethodInfos<T>(Assembly assembly) {
			var methods  = new List<MethodInfo>();
			var attrType = typeof(T);
			foreach (var type in assembly.GetTypes())
				foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
					if (Attribute.IsDefined(method, attrType))
						methods.Add(method);
			return methods;
		}
	}

	/// <summary>Represents a Console response (has STDOUT, STDERR, and an ExitCode)</summary>
	public class Response {

		public Response() {
			ExitCode        = 0;
			_stdout_builder = new StringBuilder();
			_stderr_builder = new StringBuilder();
			_stdout         = new StringWriter(_stdout_builder);
			_stderr         = new StringWriter(_stderr_builder);
		}
		public Response(string stdout) : this() {
			STDOUT.WriteLine(stdout);
		}

		StringBuilder _stdout_builder, _stderr_builder;
		StringWriter _stdout, _stderr;

		public virtual int ExitCode { get; set; }
		
		public virtual TextWriter STDOUT { get { return _stdout; } }

		public virtual TextWriter STDERR { get { return _stderr; } }

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		public virtual int Execute() {
			return Execute(true);
		}

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		/// <remarks>
		/// You can call response.Execute(false) and we'll return the ExitCode instead of actually exiting the current process.
		/// </remarks>
		public virtual int Execute(bool exit) {
			if (_stdout_builder.Length > 0) Console.Out.Write(_stdout_builder.ToString());
			if (_stderr_builder.Length > 0) Console.Error.Write(_stderr_builder.ToString());
			if (exit) Environment.Exit(ExitCode);
			return ExitCode;
		}
	}

	/// <summary>Represents a Console request (string[] args)</summary>
	/// <remarks>
	/// Originally, we didn't have a Request object, we just passed along string[] args.
	///
	/// The benefit of having a Request object is we can stick arbitrary data into 
	/// request.Data (a Dictionary of strings and objects) and that data will be 
	/// available to other Middleware and, finally, the Application.
	/// </remarks>
	public class Request {

		public Request() {}

		public Request(string[] arguments) : this() {
			Arguments = arguments;
		}

		public virtual string[] Arguments { get; set; }
	}

	/// <summary>Represents a console Application.</summary>
	/// <remarks>
	/// This is just like a Middleware (infact a Middleware *is* an Application) 
	/// but it doesn't have an inner Application that it calls
	/// </remarks>
	public class Application {

		public Application(MethodInfo method) {
			Method = method;
		}

		ApplicationAttribute _attribute;

		/// <summary>The actual MethodInfo implementation that was decorated with [Application]</summary>
		public virtual MethodInfo Method { get; set; }

		/// <summary>Gets the actual ApplicationAttribute instance that decorates this Application's Method</summary>
		public virtual ApplicationAttribute ApplicationAttribute {
			get {
				if (_attribute == null) _attribute = GetCustomAttribute<ApplicationAttribute>(Method);
				return _attribute;
			}
		}

		/// <summary>Returns the custom attribute of the given type that's decorated on the given method (if any)</summary>
		public virtual T GetCustomAttribute<T>(MethodInfo method) where T : Attribute {
			var attributes = Method.GetCustomAttributes(typeof(T), true);
			return (attributes.Length > 0) ? attributes[0] as T : null;
		}

		public virtual Response Invoke(Request request) {
			return null;
		}

		/// <summary>Returns all of the Application found in the given assemblies (see <c>AllFromAssembly</c></summary>
		public static List<Application> AllFromAssemblies(params Assembly[] assemblies) {
			var applications = new List<Application>();
			foreach (var assembly in assemblies) applications.AddRange(AllFromAssembly(assembly));
			return applications;
		}

		/// <summary>Returns all of the Application found in the given Assembly (my looking for public static methods decorated with [Application]</summary>
		public static List<Application> AllFromAssembly(Assembly assembly) {
			return Crack.GetMethodInfos<ApplicationAttribute>(assembly).Select(method => new Application(method)).ToList();
		}
	}

	/// <summary>Represents a Middleware(or Application).</summary>
	/// <remarks>
	/// MiddlewareAttribute is used to find and create these objects.
	/// </remarks>
	public class Middleware : Application {

		public Middleware(MethodInfo method) : base(method) {}

		MiddlewareAttribute _attribute;

		/// <summary>Gets the actual MiddlewareAttribute instance that decorates this Middleware's Method</summary>
		public virtual MiddlewareAttribute MiddlewareAttribute {
			get {
				if (_attribute == null) _attribute = GetCustomAttribute<MiddlewareAttribute>(Method);
				return _attribute;
			}
		}

		public override Response Invoke(Request request) {
			return null;
		}

		/// <summary>Returns all of the Middleware for the given assemblies (sorted properly!)</summary>
		public static List<Middleware> From(params Assembly[] assemblies) {
			return Sort(AllFromAssemblies(assemblies));
		}

		/// <summary>Returns a new List of Middleware sorted based on any sorting rules specified in the [Middleware] attribute</summary>
		/// <remarks>
		/// If you specify a Before or After on your [Middleware] and we can't find the middleware you specified, 
		/// we DO NOT include your Middleware in the stack.  This method will REMOVE it!  You've been warned.
		/// </remarks>
		public static List<Middleware> Sort(List<Middleware> middleware) {
			return middleware; // TODO actually sort ...
		}

		/// <summary>Returns all of the Middleware found in the given assemblies (see <c>AllFromAssembly</c></summary>
		public static new List<Middleware> AllFromAssemblies(params Assembly[] assemblies) {
			var middlewares = new List<Middleware>();
			foreach (var assembly in assemblies)
				middlewares.AddRange(AllFromAssembly(assembly));
			return middlewares;
		}

		/// <summary>Returns all of the Middleware found in the given Assembly (my looking for public static methods decorated with [Middleware]</summary>
		public static new List<Middleware> AllFromAssembly(Assembly assembly) {
			return Crack.GetMethodInfos<MiddlewareAttribute>(assembly).Select(method => new Middleware(method)).ToList();
		}
	}

	public class ApplicationAttribute : Attribute {

	}

	public class MiddlewareAttribute : Attribute {

	}
}

namespace MyApp {
	public class Program {
		public static void Main(string[] args) {
			Crack.Run(args);
		}
	}

	public class SomeMiddleware {

	}

	public class MyApp {

		[Middleware]
		public static Response Invoke(Request request, Application app) {
			Console.WriteLine("Hello from middleware");
			return app.Invoke(request);
		}

		[Application]
		public static Response Invoke(Request request) {
			Console.WriteLine("Hello from application");
			return new Response("This is my STDOUT text");
		}
	}
}
