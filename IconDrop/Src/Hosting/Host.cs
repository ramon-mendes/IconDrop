using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;
using IconDrop.Data;
using IconDrop.Svg;

namespace IconDrop.Hosting
{
	class Host : BaseHost
	{
		public Host(SciterWindow wnd)
		{
			var host = this;
			host.Setup(wnd);
			host.AttachEvh(new HostEvh());
			host.RegisterBehaviorHandler(typeof(IconsSource));
		}

		protected override SciterXDef.LoadResult OnLoadData(SciterXDef.SCN_LOAD_DATA sld)
		{
			if(sld.uri.StartsWith("tmp:tmp.svg"))
			{
				var icn = Joiner._iconsByHash.ToList()[1200].Value;
				var path = SvgParser.FromPath(icn.arr_svgpath[0]);
				//path.NormalizeToSize(100);

				string xml = SvgXML.FromSvgParser(path).ToXML();
				byte[] bytes = Encoding.UTF8.GetBytes(xml);
				_api.SciterDataReady(sld.hwnd, sld.uri, bytes, (uint)bytes.Length);
				return SciterXDef.LoadResult.LOAD_OK;
			}
			else if(sld.uri.StartsWith("svg:"))
			{
				if(sld.uri.Contains("?rnd="))
					sld.uri = sld.uri.Split('?')[0];
				int length = sld.uri.Length - 8;
				string hash = sld.uri.Substring(4, length);
				var icn = Joiner._iconsByHash[hash];
				switch(icn.kind)
				{
					case EIconKind.COLLECTION:
						try
						{
							byte[] bytess = File.ReadAllBytes(icn.path);
							_api.SciterDataReady(sld.hwnd, sld.uri, bytess, (uint)bytess.Length);
						}
						catch(Exception)
						{
						}
						break;

					case EIconKind.LIBRARY:
						string xml = SvgXML.FromIcon(icn).ToXML();
						byte[] bytes = Encoding.UTF8.GetBytes(xml);
						_api.SciterDataReady(sld.hwnd, sld.uri, bytes, (uint)bytes.Length);
						break;

					case EIconKind.STORE:
						if(Store.IsIconLoaded(icn))
						{
							byte[] bytess = File.ReadAllBytes(icn.path);
							_api.SciterDataReady(sld.hwnd, sld.uri, bytess, (uint)bytess.Length);
							return SciterXDef.LoadResult.LOAD_OK;
						}
						else
						{
							Store.LoadIcon(icn).ContinueWith((t) =>
							{
								if(t.Status == TaskStatus.RanToCompletion)
								{
									byte[] bytess = File.ReadAllBytes(icn.path);
									_api.SciterDataReadyAsync(sld.hwnd, sld.uri, bytess, (uint)bytess.Length, sld.requestId);
								}
							});
							return SciterXDef.LoadResult.LOAD_DELAYED;
						}
				}
				return SciterXDef.LoadResult.LOAD_OK;
			}
			return base.OnLoadData(sld);
		}

		/*protected override void OnDataLoaded(SciterXDef.SCN_DATA_LOADED sdl)
		{
			if(sdl.uri.StartsWith(Consts.SERVER_ICONS + "IconPacks/IconFile"))
			{
				byte[] data = new byte[sdl.dataSize];
				Marshal.Copy(sdl.data, data, 0, (int)sdl.dataSize);
				Store.ResolveIconData(sdl.uri, data);
			}
		}*/
	}

	class HostEvh : SciterEventHandler
	{
		private string _tmp_dir = Path.GetTempPath() + "IconDrop" + Path.DirectorySeparatorChar;

		public HostEvh()
		{
			if(Directory.Exists(_tmp_dir))
				Directory.Delete(_tmp_dir, true);
			Directory.CreateDirectory(_tmp_dir);
		}

		public bool Host_InDbg(SciterElement el, SciterValue[] args, out SciterValue result)
		{
#if DEBUG
			result = new SciterValue(true);
#else
			result = new SciterValue(false);
#endif
			return true;
		}

		/*
		public bool Host_Exec(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			Process.Start(args[0].Get(""), args[1].Get(""));
			result = null;
			return true;
		}*/

		public bool Host_RevealFile(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string path = args[0].Get("");
#if OSX
			Process.Start("open", "-R \"" + path + '"');
#else
			Process.Start("explorer", "/select," + path.Replace('/', '\\'));
#endif

			result = null;
			return true;
		}

		public bool Host_RevealDir(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string path = args[0].Get("");
#if OSX
			Process.Start("open", path);
#else
			Process.Start("explorer", path.Replace('/', '\\'));
#endif
			result = null;
			return true;
		}

		public void Host_Quit() => App.Exit();

		public bool Host_IconStoreList(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			result = new SciterValue();
			foreach(var pack in Store._store_packs)
				result.Append(pack.sv);
			return true;
		}

		public void Host_SetupCollections(SciterValue[] args)
		{
			string dir = args[0].Get("");
			bool create_demo = args[1].Get(false);
			var cbk = args[2];
			Collections.Setup(dir, create_demo, cbk);
		}

		public void Host_CopySkiaCode(SciterValue[] args)
		{
			string hash = args[0].Get("");
			Utils.CopyText(SKIconCode.IconToCode(Joiner._iconsByHash[hash]));
		}

		public void Host_CopySVGIconUse(SciterValue[] args)
		{
			string name = args[0].Get("");
			string svguse = $"<svg class=\"icon icon-{name}\"><use xlink:href=\"#{name}\"></use></svg>";
			Utils.CopyText(svguse);
		}

		public bool Host_CopySVGIconSymbol(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string hash = args[0].Get("");
			string ID = args.Length == 2 ? args[1].Get("") : "SOME-NAME-HERE";
			var icn = Joiner._iconsByHash[hash];
			Utils.CopyText(SvgSpriteXML.GetIconSymbolXML(icn, ID));
			result = null;
			return true;
		}

		/*public bool Host_IsUpdateAvailable(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			var version = UpdateControl.IsUpdateAvailable();
			if(version != null)
				args[0].Call(new SciterValue(version));
			result = null;
			return true;
		}*/

		public bool Host_SaveTempSVG(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string iconhash = args[0].Get("");
			bool white = args[1].Get(false);
			Icon icn = Joiner._iconsByHash[iconhash];

			string filepath;
			if(icn.kind == EIconKind.LIBRARY)
			{
				var svg = SvgXML.FromIcon(icn);
                var factor = 75f / (float)icn.bounds.w;
                svg.Scale(factor);
				var xml = svg.ToXML(white);

                string fname = icn.arr_tags[0].Replace("/", "").Replace("\\", "");
                filepath = _tmp_dir + fname + ".svg";
				File.WriteAllText(filepath, xml);
			} else {
				filepath = icn.path;
			}

			Debug.Assert(File.Exists(filepath));
			result = new SciterValue(filepath);
			return true;
		}

		public bool Host_SavePNG(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			string name = args[0].Get("");
			string filepath = _tmp_dir + name + ".png";
			var bytes = args[1].GetBytes();
			File.WriteAllBytes(filepath, bytes);

			result = new SciterValue(filepath);
			return true;
		}

		public void Host_GenerateSVGSprite(SciterValue[] args)
		{
			string sv_outputpath = args[0].Get("");
			if(!Directory.Exists(Path.GetDirectoryName(sv_outputpath)))
				return;

			var sv_icons = args[1];
			sv_icons.Isolate();

			var xml = new SvgSpriteXML();
			foreach(var item in sv_icons.Keys)
			{
				string hash = item.Get("");
				if(!Joiner._iconsByHash.ContainsKey(hash))
					continue;
				var icon = Joiner._iconsByHash[hash];
				icon.id = sv_icons[hash].Get("");
				xml.AddIcon(icon);
			}
			File.WriteAllText(sv_outputpath, xml.ToXML());
		}

#if OSX
		public void Host_StartDnD(SciterValue[] args)
		{
			string file = args[0].Get("");
			int xView = args[1].Get(-1);
			int yView = args[2].Get(-1);

			//var img = args[3].GetBytes();
			//Debug.Assert(img.Length != 0);
			//File.WriteAllBytes(_tmp_dragimg, img);

			_tmp_dragimg = Consts.AppDir_Resources + "cursor.png";
			
			new DnDOSX().StartDnD(file, _tmp_dragimg, xView, yView, () =>
			{
				args[4].Call();
			});
		}
#endif
	}

	class BaseHost : SciterHost
	{
		protected static SciterX.ISciterAPI _api = SciterX.API;
		protected static SciterArchive _archive = new SciterArchive();
		protected SciterWindow _wnd;
		private static string _rescwd;

		static BaseHost()
		{
#if !DEBUG
			_archive.Open(SciterAppResource.ArchiveResource.resources);
#endif

#if DEBUG
			_rescwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/');
#if OSX
			_rescwd += "/../../../../../res/";
#else
			_rescwd += "/../../res/";
#endif
			_rescwd = Path.GetFullPath(_rescwd).Replace('\\', '/');
			Debug.Assert(Directory.Exists(_rescwd));
#endif
		}

		public void Setup(SciterWindow wnd)
		{
			_wnd = wnd;
			SetupWindow(wnd);
		}

		public void SetupPage(string page_from_res_folder)
		{
			string path = _rescwd + page_from_res_folder;
			Debug.Assert(File.Exists(path));

#if DEBUG
			string url = "file://" + path;
#else
			string url = "archive://app/" + page_from_res_folder;
#endif

			bool res = _wnd.LoadPage(url);
			Debug.Assert(res);
		}

		protected override SciterXDef.LoadResult OnLoadData(SciterXDef.SCN_LOAD_DATA sld)
		{
#if DEBUG
			if(sld.uri.StartsWith("file://"))
			{
				Debug.Assert(File.Exists(sld.uri.Substring(7)));
			}
#endif
			if(sld.uri.StartsWith("archive://app/"))
			{
				// load resource from SciterArchive
				string path = sld.uri.Substring(14);
				byte[] data = _archive.Get(path);
				if(data!=null)
					_api.SciterDataReady(sld.hwnd, sld.uri, data, (uint) data.Length);
			}

			// call base to ensure LibConsole is loaded
			return base.OnLoadData(sld);
		}

		public static byte[] LoadResource(string path)
		{
#if DEBUG
			path = _rescwd + path;
			Debug.Assert(File.Exists(path));
			return File.ReadAllBytes(path);
#else
			return _archive.Get(path);
#endif
		}
	}
}