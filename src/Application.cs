using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Represents a console Application.</summary>
	/// <remarks>
	/// This is just like a Middleware (infact a Middleware *is* an Application) 
	/// but it doesn't have an inner Application that it calls
	/// </remarks>
	public class Application {

		/// <summary>Application constructor.  Making an application requires a MethodInfo.</summary>
		public Application(MethodInfo method) {
			if (method == null)
				throw new ArgumentException("Method cannot be null");

			ValidateMethod(method);
			Method = method;
		}

		ApplicationAttribute _attribute;
		string _name, _description;

		/// <summary>The actual MethodInfo implementation that was decorated with [Application]</summary>
		public virtual MethodInfo Method { get; set; }

		/// <summary>Returns the full name of the Method, eg. "Namespace.MyClass.MyMethod"</summary>
		public virtual string MethodFullName {
			get { return Crack.FullMethodName(Method); }
		}

		/// <summary>This Application's name.  Defaults to the full name of the method by may be overriden.</summary>
		public virtual string Name {
			get { return _name ?? (ApplicationAttribute == null ? null : ApplicationAttribute.Name) ?? MethodFullName; }
			set { _name = value; }
		}

		/// <summary>A description of this Application.  Useful if you have lots of applications you want to list/query.</summary>
		public virtual string Description {
			get { return _description ?? (ApplicationAttribute == null ? null : ApplicationAttribute.Description); }
			set { _description = value; }
		}

		/// <summary>Gets the actual ApplicationAttribute instance that decorates this Application's Method</summary>
		/// <remarks>
		/// It doesn't happen much in practice, but this *can* be null because you can create an Application using 
		/// a regular old MethodInfo, it doesn't *have* to be decorated with [Application]
		/// </remarks>
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

		/// <summary>Invoke an Application manually, just passing along string[] args</summary>
		public virtual Response Invoke(params string[] args) {
			return Invoke(new Request(args));
		}

		/// <summary>Invoke an Application with the given request</summary>
		public virtual Response Invoke(Request request) {
			return Method.Invoke(null, new object[]{ request }) as Response;
		}

		/// <summary>Invoke this application given the provided list of middleware</summary>
		public virtual Response Invoke(Request request, params Middleware[] middlewares) {
			return Invoke(request, new MiddlewareList(middlewares));
		}

		/// <summary>Invoke this application given the provided MiddlewareList of middlewares</summary>
		/// <remarks>
		/// This runs through all of the provided middleware.  We use the MiddlewareList to give 
		/// us middleware that are ordered properly, etc.
		///
		/// This really just invokes MiddlewareList.Invoke(Request, Application)
		/// </remarks>
		public virtual Response Invoke(Request request, MiddlewareList middlewares) {
			return middlewares.Invoke(request, this);
		}

		/// <summary>Raises an InvalidApplicationException if this method doesn't look valid, so we won't be able to Invoke it properly.</summary>
		public virtual void ValidateMethod(MethodInfo method) {
			var errors = new List<string>();

			if (! method.IsStatic)
				errors.Add("Must be static");

			if (! typeof(Response).IsAssignableFrom(method.ReturnType))
				errors.Add("Must return a Response");

			var parameters = method.GetParameters();
			if (parameters.Length != 1)
				errors.Add("Must take 1 parameter (Request)");
			else
				if (! parameters.First().ParameterType.IsAssignableFrom(typeof(Request)))
					errors.Add("Parameter must be a Request");

			if (errors.Count > 0) {
				errors.Insert(0, Crack.FullMethodName(method) + " cannot be used as " + this.GetType().Name);
				ThrowInvalidException(string.Join(". ", errors.ToArray()) + ".");
			}
		}

		/// <summary>Throws an InvalidApplicationException.  Can be overriden to throw InvalidMiddlewareException, etc.</summary>
		public virtual void ThrowInvalidException(string message) {
			throw new InvalidApplicationException(message);
		}

		/// <summary>Returns all of the Application found in the given assemblies (see <c>AllFromAssembly</c></summary>
		public static ApplicationList AllFromAssemblies(params Assembly[] assemblies) {
			var applications = new ApplicationList();
			foreach (var assembly in assemblies) applications.AddRange(AllFromAssembly(assembly));
			return applications;
		}

		/// <summary>Returns all of the Application found in the given Assembly (my looking for public static methods decorated with [Application]</summary>
		public static ApplicationList AllFromAssembly(Assembly assembly) {
			var all = Crack.GetMethodInfos<ApplicationAttribute>(assembly).Select(method => new Application(method)).ToList();
			return new ApplicationList(all);
		}
	}
}
