using System;
using System.IO;
using System.Text;
using System.Reflection;
using NUnit.Framework;

namespace ConsoleRack.Specs {

	[TestFixture]
	public class ResponseSpec : Spec {

		[Test]
		public void defaults() {
			var resp = new Response();
			resp.ExitCode.ShouldEqual(0);
			resp.STDOUT.ToString().ShouldEqual("");
			resp.STDERR.ToString().ShouldEqual("");
		}

		[Test]
		public void can_instantiate_with_STDOUT() {
			new Response("Foo Bar").STDOUT.ToString().ShouldEqual("Foo Bar\n");
			new Response("{0} there {1}", "hi", 15).STDOUT.ToString().ShouldEqual("hi there 15\n");
		}

		[Test]
		public void can_get_StringBuilder_for_STDOUT_and_STDERR() {
			var response = new Response();
			response.Out.Should(Be.InstanceOf(typeof(TextWriter)));
			response.Error.Should(Be.InstanceOf(typeof(TextWriter)));

			response.STDOUT.Append("hello");

			response.OutputText.ShouldEqual("hello");
			response.ErrorText.ShouldEqual("");

			response.STDERR.Append("boom!");

			response.OutputText.ShouldEqual("hello");
			response.ErrorText.ShouldEqual("boom!");
		}

		[Test]
		public void can_TextWriter_for_STDOUT_and_STDERR() {
			var response = new Response();
			response.STDOUT.Should(Be.InstanceOf(typeof(StringBuilder)));
			response.STDERR.Should(Be.InstanceOf(typeof(StringBuilder)));

			response.Out.Write("hello");

			response.OutputText.ShouldEqual("hello");
			response.ErrorText.ShouldEqual("");

			response.Error.Write("boom!");

			response.OutputText.ShouldEqual("hello");
			response.ErrorText.ShouldEqual("boom!");
		}

		[Test]
		public void can_get_string_Text_for_STDOUT_and_STDERR() {
			var response = new Response("w00t");

			response.Text.ShouldEqual("w00t\n");
			response.OutputText.ShouldEqual("w00t\n");
			response.ErrorText.ShouldEqual("");

			response.OutputText += "hi";

			response.Text.ShouldEqual("w00t\nhi");
			response.OutputText.ShouldEqual("w00t\nhi");
			response.ErrorText.ShouldEqual("");

			response.ErrorText = "boom!";

			response.Text.ShouldEqual("w00t\nhi");
			response.OutputText.ShouldEqual("w00t\nhi");
			response.ErrorText.ShouldEqual("boom!");
		}
	}
}
