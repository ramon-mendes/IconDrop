using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SciterSharp;
using IconDrop.Data;

namespace IconDrop.Hosting
{
	class IconsSource : SciterEventHandler
	{
		#region TIScript Interface
		private IReadOnlyList<Icon> _iconList;
		private int _bulk_pos = 0;
		private Random _rnd = new Random();

		public bool EnsureStoreIsLoaded(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			Store.LoadStorePack(args[0].Get(""), args[1]);
			result = null;
			return true;
		}

		public bool IconHashExists(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string hash = args[0].Get("");
			bool exists = Joiner._iconsByHash.ContainsKey(hash);
			result = new SciterValue(exists);
			return true;
		}

		public bool IconTranslateURL(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string hash = args[0].Get("");
			string url = "svg:" + hash + ".svg";
			if(Joiner._iconsByHash[hash].kind == EIconKind.STORE)
				url += "?rnd=" + _rnd.Next(9999999);
			result = new SciterValue(url);
			return true;
		}

		public bool GetSources(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			result = new SciterValue(Library._lib.sources_sv);
			return true;
		}

		private void SetIconList(List<Icon> list, bool overflows = true)
		{
			_iconList = list;
			_bulk_pos = 0;
		}

		public bool ResetByProj(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			args[0].Isolate();
			var hashes = args[0].Keys.Select(k => k.Get("")).ToList();

			List<Icon> list = new List<Icon>();
			foreach(var hash in hashes)
				list.Add(Joiner._iconsByHash[hash]);
			SetIconList(list);

			result = null;
			return true;
		}

		public bool ResetBySource(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			int isource = args[0].Get(-1);

			List<Icon> icons = new List<Icon>();
			Source source = Library._lib.sources[isource];
			foreach(var icon in source.icons)
				icons.Add(icon);

			SetIconList(icons, false);

			result = null;
			return true;
		}

		public bool ResetByStore(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			var pack = Store._store_packs.Single(p => p.id == args[0].Get(""));
			SetIconList(pack.icons, false);
			result = null;
			return true;
		}

		public bool ResetByNeedle(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string needle = args[0].Get("");

			List<Icon> icons = new List<Icon>();
			foreach(var icon in Joiner._iconsByHash.Values)
			{
				foreach(var tag in icon.arr_tags)
				{
					if(tag.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						icons.Add(icon);
						break;
					}
				}
			}

			SetIconList(icons);

			_bulk_pos = 0;

			result = null;
			return true;
		}

		public bool ResetByCollection(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string dir = args[0].Get("");
			var collected = Collections._collected_dirs[dir];
			SetIconList(collected.Select(c => c.icon).ToList());

			result = null;
			return true;
		}

		public bool ResetPosOnly(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			_bulk_pos = 0;
			result = null;
			return true;
		}

		public bool LoadBulk(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			var f_CreateItem = args[0];

			foreach(Icon icon in _iconList.Skip(_bulk_pos))
			{
				if(icon.kind == EIconKind.COLLECTION && !File.Exists(icon.path))
					continue;

				bool consumed = f_CreateItem.Call(icon.ToSV()).Get(true);
				if(!consumed)
					break;
				else
					_bulk_pos++;
			}

			result = null;
			return true;
		}
		#endregion
	}
}