using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Custom List of Application that lets you easily get an Application by name</summary>
	public class ApplicationList : List<Application>, IList<Application>, IEnumerable<Application> {
		public ApplicationList() : base(){}
		public ApplicationList(IEnumerable<Application> apps) : base(apps){}

		public virtual Application this[string name] {
			get { return this.FirstOrDefault(app => app.Name == name); }
		}
	}
}
