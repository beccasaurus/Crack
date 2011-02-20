using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace NUnit.Framework {
	public static class ToDictionaryExtension {
        public static IDictionary<string, object> ToDictionary(this object anonymousType) {
            var attr = BindingFlags.Public | BindingFlags.Instance;
            var dict = new Dictionary<string, object>();
            foreach (var property in anonymousType.GetType().GetProperties(attr))
                if (property.CanRead)
                    dict.Add(property.Name, property.GetValue(anonymousType, null));
            return dict;
        } 
	}
}
