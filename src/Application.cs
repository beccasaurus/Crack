using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	public class ApplicationAttribute : Attribute {
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
			Console.WriteLine("Application.Invoke({0})", request);
			return Method.Invoke(null, new object[]{ request }) as Response;
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
}
