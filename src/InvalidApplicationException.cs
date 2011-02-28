using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Exception that gets raised if the MethodInfo used to instantiate an Application isn't valid (eg. wrong return type)</summary>
	public class InvalidApplicationException : Exception  {
		public InvalidApplicationException(string message) : base(message) {}
	}
}
