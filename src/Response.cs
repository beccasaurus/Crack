using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Represents a Console response (has STDOUT, STDERR, and an ExitCode)</summary>
	public class Response {

		public Response() {
			ExitCode        = 0;
			_stdout_builder = new StringBuilder();
			_stderr_builder = new StringBuilder();
			_stdout         = new StringWriter(_stdout_builder);
			_stderr         = new StringWriter(_stderr_builder);
		}
		public Response(string stdout) : this() {
			STDOUT.WriteLine(stdout);
		}

		StringBuilder _stdout_builder, _stderr_builder;
		StringWriter _stdout, _stderr;

		public virtual int ExitCode { get; set; }
		
		public virtual TextWriter STDOUT { get { return _stdout; } }

		public virtual TextWriter STDERR { get { return _stderr; } }

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		public virtual int Execute() {
			return Execute(true);
		}

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		/// <remarks>
		/// You can call response.Execute(false) and we'll return the ExitCode instead of actually exiting the current process.
		/// </remarks>
		public virtual int Execute(bool exit) {
			if (_stdout_builder.Length > 0) Console.Out.Write(_stdout_builder.ToString());
			if (_stderr_builder.Length > 0) Console.Error.Write(_stderr_builder.ToString());
			if (exit) Environment.Exit(ExitCode);
			return ExitCode;
		}
	}
}
