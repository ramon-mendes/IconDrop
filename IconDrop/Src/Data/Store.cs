using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SciterSharp;
using System.Diagnostics;

namespace IconDrop.Data
{
	static class Store
	{
		public static readonly List<StorePack> _store_packs = new List<StorePack>();

		public class StorePack
		{
			public string id;
			public string name;
			public string author;
			public string url;
			// string[] files
			public List<Icon> icons;
			public SciterValue sv;
			public bool colored;

			public SciterValue ToSV()
			{
				SciterValue sv = new SciterValue();
				sv["id"] = new SciterValue(id);
				sv["pack_name"] = new SciterValue(name);
				sv["author_name"] = new SciterValue(author);
				sv["author_link"] = new SciterValue(url);
				return sv;
			}
		}
		
		public static void Setup()
		{
			var data = File.ReadAllBytes(Consts.AppDir_Shared + "id_store.json");
			string json = Encoding.UTF8.GetString(data);

			SciterValue sv = SciterValue.FromJSONString(json);
			foreach(var sv_pack in sv.AsEnumerable())
			{
				var pack = new StorePack()
				{
					id = sv_pack["id"].Get(""),
					name = sv_pack["name"].Get(""),
					author = sv_pack["author"].Get(""),
					url = sv_pack["url"].Get(""),
					icons = new List<Icon>(),
					colored = sv_pack["colored"].Get(false)
				};
				Debug.Assert(pack.name != "" && pack.author != "" && pack.url != "");
				pack.sv = pack.ToSV();

				foreach(var sv_file in sv_pack["files"].AsEnumerable())
				{
					string filename = sv_file.Get("");
					string hash = Utils.CalculateMD5Hash($"Store-{pack.id}-{filename}");
					pack.icons.Add(new Icon()
					{
						kind = EIconKind.STORE,
						hash = hash,
						path = Consts.DirUserData_Store + pack.id + "/" + filename,
						source = pack.sv,
						colored = pack.colored,
						arr_tags = new List<string>() { filename }
					});
				}

				_store_packs.Add(pack);
			}

            Utils.Shuffle(_store_packs);
        }

        public static void LoadStorePack(string id, SciterValue cbk)
		{
			Task.Run(() =>
			{
				List<Task> tasks = new List<Task>();
				var pack = _store_packs.Single(p => p.id == id);
				foreach(var icn in pack.icons)
				{
					if(!IsIconLoaded(icn))
						tasks.Add(LoadIcon(icn));
				}

				bool success = true;
				try
				{
					Task.WaitAll(tasks.ToArray());
				}
				catch(Exception)
				{
					success = false;
				}
				cbk.Call(new SciterValue(success));
			});
		}

		public static Task LoadIcon(Icon icn)
		{
			return Task.Run(() =>
			{
				using(WebClient wb = new WebClient())
				{
					var url = Consts.SERVER_ICONS + "IconPacks/IconFile?id=" + icn.source["id"].Get("") + "&file=" + Uri.EscapeDataString(Path.GetFileName(icn.path));
					Debug.WriteLine("Downloading " + url);
					var data = wb.DownloadData(url);
					Directory.CreateDirectory(Path.GetDirectoryName(icn.path));
					File.WriteAllBytes(icn.path, data);
					Debug.WriteLine("DONE Downloading " + url);
				}
			});
		}

		public static bool IsIconLoaded(Icon icn)
		{
			return File.Exists(icn.path) && new FileInfo(icn.path).Length > 0;
		}
	}
}