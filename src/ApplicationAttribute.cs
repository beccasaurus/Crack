using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>The [Application] attribute for your application methods</summary>
	public class ApplicationAttribute : Attribute {
		public ApplicationAttribute(){}
		public ApplicationAttribute(string description) : this() {
			Description = description;
		}
		public ApplicationAttribute(string name, string description) : this(description) {
			Name = name;
		}

		public virtual string Name        { get; set; }
		public virtual string Description { get; set; }
	}
}
