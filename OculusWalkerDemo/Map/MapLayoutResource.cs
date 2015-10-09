using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using SharpDX;

namespace Oggy
{
	public class MapLayoutResource : IDisposable// : ResourceBase
	{
		#region properties

		private List<Entry> m_entryList;
		public ReadOnlyCollection<Entry> Entries
		{
			get
			{
				return m_entryList.AsReadOnly();
			}
		}

		#endregion // properties

		#region public types

		public struct Entry
		{
			public int ModelId;
			public Matrix Layout;
		}

		#endregion // public types

		public MapLayoutResource(String uid)
			//: base(uid)
		{
			m_entryList = new List<Entry>(10);
		}

		public static MapLayoutResource FromScene(String uid, BlenderScene scene)
		{
			var res = new MapLayoutResource(uid);

			foreach (var n in scene.LinkList)
			{
				var match = _ModelIdRegex.Match(n.TargetFileName);
				int modelId = int.Parse(match.Groups[1].Value);
				res.m_entryList.Add(new Entry() { ModelId = modelId, Layout = n.Layout });
			}

			return res;
		}

		public void Dispose()
		{
			m_entryList.Clear();
		}

		private static Regex _ModelIdRegex = new Regex(@"p(\d+)\.blend");

		#region private members

		

		#endregion // private members
	}
}
