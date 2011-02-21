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
			ExitCode = 0;
			STDOUT   = new StringBuilder();
			STDERR   = new StringBuilder();
		}
		public Response(string stdout) : this() {
			Out.WriteLine(stdout);
		}
		public Response(string stdout, params object[] objects) : this() {
			Out.WriteLine(stdout, objects);
		}

		public virtual int ExitCode { get; set; }
		
		/// <summary>The StringBuilder that we use to persist this Response's STDOUT (See Out for a TextWriter)</summary>
		public virtual StringBuilder STDOUT { get; set; }

		/// <summary>The StringBuilder that we use to persist this Response's STDERR (See Error for a TextWriter)</summary>
		public virtual StringBuilder STDERR { get; set; }

		/// <summary>A TextWriter for our STDOUT, just like Console.Out so you can easily call response.Out.WriteLine()</summary>
		public virtual TextWriter Out { get { return new StringWriter(STDOUT); } }

		/// <summary>A TextWriter for our STDERR, just like Console.Error so you can easily call response.Out.WriteLine()</summary>
		public virtual TextWriter Error { get { return new StringWriter(STDERR); } }

		/// <summary>Returns all of the text that has been written to STDOUT</summary>
		public virtual string OutputText {
			get { return STDOUT.ToString(); }
			set { STDOUT = new StringBuilder(value); }
		}

		/// <summary>Returns all of the text that has been written to STDERR</summary>
		public virtual string ErrorText {
			get { return STDERR.ToString(); }
			set { STDERR = new StringBuilder(value); }
		}

		/// <summary>Shortcut to OutputText</summary>
		public virtual string Text {
			get { return OutputText;  }
			set { OutputText = value; }
		}

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		public virtual int Execute() {
			return Execute(true);
		}

		/// <summary>Actually executes this Response, writing to Console.Out, Console.Error, and exiting the process using the ExitCode</summary>
		/// <remarks>
		/// You can call response.Execute(false) and we'll return the ExitCode instead of actually exiting the current process.
		/// </remarks>
		public virtual int Execute(bool exit) {
			if (STDOUT.Length > 0) Console.Out.Write(STDOUT.ToString());
			if (STDERR.Length > 0) Console.Error.Write(STDERR.ToString());
			if (exit) Environment.Exit(ExitCode);
			return ExitCode;
		}

		#region Helper methods for modifying the STDOUT and STDERR ... these should be Fluent
		public virtual Response Append(string str, params object[] objects)  { return AppendToOutput(str, objects); }
		public virtual Response Prepend(string str, params object[] objects) { return PrependToOutput(str, objects); }

		public virtual Response AppendToOutput(string str, params object[] objects) {
			STDOUT.Append(string.Format(str, objects));
			return this;
		}
		public virtual Response PrependToOutput(string str, params object[] objects) {
			STDOUT.Insert(0, string.Format(str, objects));
			return this;
		}

		public virtual Response AppendToError(string str, params object[] objects) {
			STDERR.Append(string.Format(str, objects));
			return this;
		}
		public virtual Response PrependToError(string str, params object[] objects) {
			STDERR.Insert(0, string.Format(str, objects));
			return this;
		}
		#endregion
	}
}
