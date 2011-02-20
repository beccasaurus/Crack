using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Represents a Console request (string[] args)</summary>
	/// <remarks>
	/// Originally, we didn't have a Request object, we just passed along string[] args.
	///
	/// The benefit of having a Request object is we can stick arbitrary data into 
	/// request.Data (a Dictionary of strings and objects) and that data will be 
	/// available to other Middleware and, finally, the Application.
	/// </remarks>
	public class Request {

		public Request() {
			Data = new Dictionary<string, object>();
		}

		public Request(string[] arguments) : this() {
			Arguments = arguments;
		}

		public virtual string[] Arguments { get; set; }

		public virtual IDictionary<string, object> Data { get; set; }
	}
}
