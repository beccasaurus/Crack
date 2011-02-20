using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Exception that gets raised if the MethodInfo used to instantiate an Middleware isn't valid (eg. wrong return type)</summary>
	public class InvalidMiddlewareException : Exception  {
		public InvalidMiddlewareException(string message) : base(message) {}
	}

	/// <summary>The [Application] attribute for your application methods</summary>
	public class MiddlewareAttribute : ApplicationAttribute {
		public MiddlewareAttribute() : base(){}
		public MiddlewareAttribute(string description) : base(description) {}
		public MiddlewareAttribute(string name, string description) : base(name, description) {}
	}

	/// <summary>Custom List of Middleware that lets you easily get an Middleware by name</summary>
	public class MiddlewareList : List<Middleware>, IList<Middleware>, IEnumerable<Middleware> {
		public MiddlewareList() : base(){}
		public MiddlewareList(IEnumerable<Middleware> middlewares) : base(middlewares){}

		public virtual Middleware this[string name] {
			get { return this.FirstOrDefault(mw => mw.Name == name); }
		}

		/// <summary>Given this Request and Application, we sort all of our middleware in order and invoke the first one!</summary>
		public virtual Response Invoke(Request request, Application app) {
			if (Count == 0)
				return app.Invoke(request);

			// Put all of our middleware in the right order
			Middleware.Sort(this);

			// Set the Application property on all middlewares, given the order that they're currently in
			Middleware.SetInnerApplications(this);

			this.Last().Application = app;
			return this.First().Invoke(request);
		}

		void OrderMiddlewares() {
		}
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

		/// <summary>Raises an InvalidMiddlewareException if this method doesn't look valid, so we won't be able to Invoke it properly.</summary>
		public override void ValidateMethod(MethodInfo method) {
			var errors = new List<string>();

			if (! method.IsStatic)
				errors.Add("Must be static");

			if (! typeof(Response).IsAssignableFrom(method.ReturnType))
				errors.Add("Must return a Response");

			var parameters = method.GetParameters();
			if (parameters.Length != 2)
				errors.Add("Must take 2 parameters (Request, Application)");
			else {
				if (! parameters.First().ParameterType.IsAssignableFrom(typeof(Request)))
					errors.Add("Parameter 1 must be a Request");
				if (! parameters.Last().ParameterType.IsAssignableFrom(typeof(Application)))
					errors.Add("Parameter 2 must be an Application");
			}

			if (errors.Count > 0) {
				errors.Insert(0, "This method cannot be used as a Middleware");
				throw new InvalidMiddlewareException(string.Join(". ", errors.ToArray()) + ".");
			}
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
