using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>The [Middleware] attribute for your middleware methods</summary>
	public class MiddlewareAttribute : Attribute {
		public MiddlewareAttribute(){
			First = false;
			Last  = false;
		}
		public MiddlewareAttribute(string description) : this() {
			Description = description;
		}
		public MiddlewareAttribute(string name, string description) : this(description) {
			Name = name;
		}

		public virtual string Name        { get; set; }
		public virtual string Description { get; set; }
		public virtual string Before      { get; set; }
		public virtual string After       { get; set; }
		public virtual bool   First       { get; set; }
		public virtual bool   Last        { get; set; }
	}
}
