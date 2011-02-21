using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

/// <summary>Primary namespace for Console Rack (Crack)</summary>
namespace ConsoleRack {

	/// <summary>Main Crack class for things like running a Crack application, finding Middleware, etc</summary>
	/// <remarks>
	/// This is really just a convenience class with static helper methods that make it easy to run 
	/// Applications and Middleware.
	/// </remarks>
	public class Crack {

		static MiddlewareList  _middlewares;
		static ApplicationList _applications;

		public static MiddlewareList Middlewares {
			get {
				if (_middlewares == null) _middlewares = Middleware.From(Assembly.GetCallingAssembly());
				return _middlewares;
			}
			set { _middlewares = value; }
		}

		public static ApplicationList Applications {
			get {
				if (_applications == null) _applications = Application.AllFromAssembly(Assembly.GetCallingAssembly());
				return _applications;
			}
			set { _applications = value; }
		}

		/// <summary>If only 1 [Application] is found, we run that.</summary>
		public static void Run(string[] args) {
			Middlewares  = Middleware.From(Assembly.GetCallingAssembly());
			Applications = Application.AllFromAssembly(Assembly.GetCallingAssembly());

			if (Crack.Applications.Count == 1)
				Run(Crack.Applications.First(), args);
			else
				throw new Exception("Unless there is exactly 1 [Application] found, you must pass an Application to Run()");
		}

		/// <summary>Runs all Crack.Middleware using the provided arguments</summary>
		/// <remarks>
		/// If Crack.Middleware has not been set, we look for all [Middleware] in the calling assembly.
		/// </remarks>
		public static void Run(Application app, string[] args) {
			app.Invoke(new Request(args), Crack.Middlewares).Execute();
		}

		/// <summary>Returns a list of all public static MethodInfo found in the given assembly that have the given attribute type</summary>
		public static List<MethodInfo> GetMethodInfos<T>(Assembly assembly) {
			var methods  = new List<MethodInfo>();
			var attrType = typeof(T);
			foreach (var type in assembly.GetTypes())
				foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
					if (Attribute.IsDefined(method, attrType))
						methods.Add(method);
			return methods;
		}

		/// <summary>Returns the full name of the Method, eg. "Namespace.MyClass.MyMethod"</summary>
		public static string FullMethodName(MethodInfo method) {
			return (method == null) ? null : method.DeclaringType.FullName + "." + method.Name;
		}
	}
}
