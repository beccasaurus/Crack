using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Exception that gets raised if the MethodInfo used to instantiate an Command isn't valid (eg. wrong return type)</summary>
	public class InvalidCommandException : Exception  {
		public InvalidCommandException(string message) : base(message) {}
	}
}
