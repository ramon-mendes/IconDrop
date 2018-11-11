using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using System.Diagnostics;
using System.IO;

namespace IconDrop.Data
{
	public struct BoundsD
	{
		public double l;
		public double t;
		public double w;
		public double h;
	}

	public class Library
	{
		public static Library _lib { get; private set; }

		public static void Setup()
		{
			var data = File.ReadAllBytes(Consts.AppDir_Shared + "id_flaticons.json");
			string json = Encoding.UTF8.GetString(data);
			_lib = FromSV(SciterValue.FromJSONString(json));

#if DEBUG
			var r = _lib.sources.GroupBy(l => l.name);
			Debug.Assert(r.Count() == _lib.sources.Count);
#endif


			int ihash = 100;
			foreach(var source in _lib.sources)
			{
				var source_sv = source.ToSV();
				_lib.sources_sv.Add(source_sv);
				foreach(var icon in source.icons)
				{
					icon.hash = Utils.CalculateMD5Hash(string.Join("", icon.arr_svgpath)) + "-" + (ihash++);
					icon.source = source_sv;
				}
			}
		}


		public List<Source> sources;
		public List<SciterValue> sources_sv;

		public static Library FromSV(SciterValue sv)
		{
			var lib = new Library()
			{
				sources = new List<Source>(),
				sources_sv = new List<SciterValue>()
			};

			foreach(var sv_source in sv["sources"].AsEnumerable())
			{
				Source source = new Source()
				{
					name = sv_source["name"].Get(""),
					url = sv_source["url"].Get(""),
					license = sv_source["license"].Get(""),
					licenseURL = sv_source["licenseURL"].Get(""),
					designer = sv_source["designer"].Get(""),
					designerURL = sv_source["designerURL"].Get(""),
					icons = new List<Icon>()
				};
				lib.sources.Add(source);

				foreach(var sv_icon in sv_source["icons"].AsEnumerable())
				{
					Icon icon = new Icon();
					icon.kind = EIconKind.LIBRARY;
					icon.bounds.l = sv_icon["bounds"]["l"].Get(0.0);
					icon.bounds.t = sv_icon["bounds"]["t"].Get(0.0);
					icon.bounds.w = sv_icon["bounds"]["w"].Get(0.0);
					icon.bounds.h = sv_icon["bounds"]["h"].Get(0.0);
					source.icons.Add(icon);

					foreach(var sv_tag in sv_icon["arr_svgpath"].AsEnumerable())
						icon.arr_svgpath.Add(sv_tag.Get(""));
					foreach(var sv_tag in sv_icon["arr_fill"].AsEnumerable())
						icon.arr_fill.Add(sv_tag.Get(""));
					foreach(var sv_tag in sv_icon["arr_tags"].AsEnumerable())
						icon.arr_tags.Add(sv_tag.Get(""));
				}
			}
			return lib;
		}
	}
}