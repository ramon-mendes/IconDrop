using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SciterSharp;
using Kernys.Bson;
using IconDrop.Hosting;

namespace IconDrop.Data
{
	static class Joiner
	{
		public static Dictionary<string, Icon> _iconsByHash { get; private set; }

		public static void Setup()
		{
			Store.Setup();
			Library.Setup();

			Join();
		}

		public static void Join()
		{
			var dic = new Dictionary<string, Icon>();

			// flat packs
			foreach(var source in Library._lib.sources)
			{
				foreach(var icn in source.icons)
					dic.Add(icn.hash, icn);
			}

			// collections
			foreach(var dir in Collections._collected_dirs)
			{
				foreach(var collected in dir.Value)
					dic.Add(collected.icon.hash, collected.icon);
			}

			// store packs
			foreach(var pack in Store._store_packs)
			{
				foreach(var icn in pack.icons)
					dic.Add(icn.hash, icn);
			}

			_iconsByHash = dic;
		}
	}
}