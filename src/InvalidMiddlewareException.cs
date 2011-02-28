using System;

namespace ConsoleRack {

	/// <summary>Exception that gets raised if the MethodInfo used to instantiate an Middleware isn't valid (eg. wrong return type)</summary>
	public class InvalidMiddlewareException : Exception  {
		public InvalidMiddlewareException(string message) : base(message) {}
	}
}
