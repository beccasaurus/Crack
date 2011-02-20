using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	public class MiddlewareAttribute : Attribute {
	}

	/// <summary>Represents a Middleware(or Application).</summary>
	/// <remarks>
	/// MiddlewareAttribute is used to find and create these objects.
	/// </remarks>
	public class Middleware : Application {

		public Middleware(MethodInfo method) : base(method) {}

		MiddlewareAttribute _attribute;

		/// <summary>This Middleware's inner application that it gets called with and can Invoke()</summary>
		public virtual Application Application { get; set; }

		/// <summary>Gets the actual MiddlewareAttribute instance that decorates this Middleware's Method</summary>
		public virtual MiddlewareAttribute MiddlewareAttribute {
			get {
				if (_attribute == null) _attribute = GetCustomAttribute<MiddlewareAttribute>(Method);
				return _attribute;
			}
		}

		public override Response Invoke(Request request) {
			return Method.Invoke(null, new object[]{ request, Application }) as Response;
		}

		/// <summary>Returns all of the Middleware for the given assemblies (sorted properly!)</summary>
		public static List<Middleware> From(params Assembly[] assemblies) {
			return SetInnerApplications(Sort(AllFromAssemblies(assemblies)));
		}

		/// <summary>Returns a new List of Middleware sorted based on any sorting rules specified in the [Middleware] attribute</summary>
		/// <remarks>
		/// If you specify a Before or After on your [Middleware] and we can't find the middleware you specified, 
		/// we DO NOT include your Middleware in the stack.  This method will REMOVE it!  You've been warned.
		/// </remarks>
		public static List<Middleware> Sort(List<Middleware> middleware) {
			return middleware; // TODO actually sort ...
		}

		/// <summary>Given a list of middleware, this goes through and sets all of their Application properties</summary>
		/// <remarks>
		/// Once you get this list back, you should Invoke the *first* middleware, if any.
		///
		/// You should also set the Application of the last Middleware (which will be null) 
		/// to the actual application that you want to run.
		///
		/// NOTE: This works with the List that its given and modifies it.
		/// </remarks>
		public static List<Middleware> SetInnerApplications(List<Middleware> middleware) {
			for (var i = 0; i < middleware.Count - 1; i++)
				middleware[i].Application = middleware[i + 1];
			return middleware;
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
}
