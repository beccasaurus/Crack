using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Custom List of Command that lets you easily get an Command by name</summary>
	public class CommandList : List<Command>, IList<Command>, IEnumerable<Command> {
		public CommandList() : base(){}
		public CommandList(IEnumerable<Command> apps) : base(apps){}

		/// <summary>Returns a command with this exact name, or null.</summary>
		public virtual Command this[string name] {
			get { return this.FirstOrDefault(app => app.Name == name); }
		}

		/// <summary>Returns all of the commands with names starting with the given string.  Useful for finding and running CLI commands.</summary>
		public virtual CommandList StartingWith(string query) {
			return new CommandList(this.Where(cmd => cmd.Name.StartsWith(query)).OrderBy(cmd => cmd.Name).ToList());
		}
	}
}
