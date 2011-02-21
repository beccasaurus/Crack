using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Represents a Middleware(or Application).</summary>
	/// <remarks>
	/// MiddlewareAttribute is used to find and create these objects.
	/// </remarks>
	public class Middleware : Application {

		public Middleware(MethodInfo method) : base(method) {}

		MiddlewareAttribute _attribute;
		string _name, _description, _before, _after;
		bool? _first, _last;

		/// <summary>This Middleware's inner application that it gets called with and can Invoke()</summary>
		public virtual Application Application { get; set; }

		/// <summary>Gets the actual MiddlewareAttribute instance that decorates this Middleware's Method</summary>
		public virtual MiddlewareAttribute MiddlewareAttribute {
			get {
				if (_attribute == null) _attribute = GetCustomAttribute<MiddlewareAttribute>(Method);
				return _attribute;
			}
		}

		/// <summary>This Middleware's name.  Defaults to the full name of the method by may be overriden.</summary>
		public virtual string Name {
			get { return _name ?? ((MiddlewareAttribute == null ? null : MiddlewareAttribute.Name)) ?? MethodFullName; }
			set { _name = value; }
		}

		/// <summary>A description of this Middleware.  Useful if you have lots of middlewares you want to list/query.</summary>
		public virtual string Description {
			get { return _description ?? (MiddlewareAttribute == null ? null : MiddlewareAttribute.Description); }
			set { _description = value; }
		}

		public virtual string Before {
			get { return _before ?? ((MiddlewareAttribute != null) ? MiddlewareAttribute.Before : null); }
			set { _before = value; }
		}

		public virtual string After {
			get { return _after ?? ((MiddlewareAttribute != null) ? MiddlewareAttribute.After : null); }
			set { _after = value; }
		}

		public virtual bool First {
			get { return (bool) (_first ?? ((MiddlewareAttribute != null) ? MiddlewareAttribute.First : false)); }
			set { _first = value; }
		}

		public virtual bool Last {
			get { return (bool) (_last ?? ((MiddlewareAttribute != null) ? MiddlewareAttribute.Last : false)); }
			set { _last = value; }
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
				errors.Insert(0, Crack.FullMethodName(method) + " cannot be used as a Middleware");
				throw new InvalidMiddlewareException(string.Join(". ", errors.ToArray()) + ".");
			}
		}

		/// <summary>Returns all of the Middleware for the given assemblies (sorted properly!)</summary>
		public static MiddlewareList From(params Assembly[] assemblies) {
			return SetInnerApplications(Sort(AllFromAssemblies(assemblies)));
		}

		/// <summary>Returns a new List of Middleware sorted based on any sorting rules specified in the [Middleware] attribute</summary>
		/// <remarks>
		/// If you specify a Before or After on your [Middleware] and we can't find the middleware you specified, 
		/// we DO NOT include your Middleware in the stack.  This method will REMOVE it!  You've been warned.
		/// </remarks>
		public static MiddlewareList Sort(MiddlewareList middlewares) {
			var copy = middlewares.ToArray();
			
			// Process Middleware marked with First or Last
			foreach (var mw in copy)
				if (mw.First)
					middlewares.MoveToTop(mw);
				else if (mw.Last)
					middlewares.MoveToBottom(mw);

			// Process Middleware marked with Before or After
			foreach (var mw in copy) {
				if (mw.First || mw.Last) continue; // you can't be First|Last and use Before|After

				if (! string.IsNullOrEmpty(mw.Before))
					middlewares.MoveToBefore(mw, mw.Before);
				else if (! string.IsNullOrEmpty(mw.After))
					middlewares.MoveToAfter(mw, mw.After);
			}

			return middlewares;
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
		public static MiddlewareList SetInnerApplications(MiddlewareList middleware) {
			for (var i = 0; i < middleware.Count - 1; i++)
				middleware[i].Application = middleware[i + 1];
			return middleware;
		}

		/// <summary>Returns all of the Middleware found in the given assemblies (see <c>AllFromAssembly</c></summary>
		public static new MiddlewareList AllFromAssemblies(params Assembly[] assemblies) {
			var middlewares = new MiddlewareList();
			foreach (var assembly in assemblies)
				middlewares.AddRange(AllFromAssembly(assembly));
			return middlewares;
		}

		/// <summary>Returns all of the Middleware found in the given Assembly (my looking for public static methods decorated with [Middleware]</summary>
		public static new MiddlewareList AllFromAssembly(Assembly assembly) {
			var middlewares = Crack.GetMethodInfos<MiddlewareAttribute>(assembly).Select(method => new Middleware(method)).ToList();
			return new MiddlewareList(middlewares);
		}

		/// <summary>Shortcut to get all Middleware from the calling assembly.</summary>
		public static new MiddlewareList AllFromAssembly() {
			return AllFromAssembly(Assembly.GetCallingAssembly());
		}
	}
}
