using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>The [Command] attribute for your command methods</summary>
	public class CommandAttribute : ApplicationAttribute {
		public CommandAttribute() : base() {}
		public CommandAttribute(string description) : base(description) {}
		public CommandAttribute(string name, string description) : base(name, description) {}
	}
}
