using SciterSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconDrop.Data
{
	public class Source
	{
		public string name;
		public string url;
		public string license;
		public string licenseURL;
		public string designer;
		public string designerURL;
		public List<Icon> icons;

		public SciterValue ToSV()
		{
			SciterValue sv = new SciterValue();
			sv["name"] = new SciterValue(name);
			sv["url"] = new SciterValue(url);
			sv["license"] = new SciterValue(license);
			sv["licenseURL"] = new SciterValue(licenseURL);
			//sv["designer"] = new SciterValue(designer);
			//sv["designerURL"] = new SciterValue(designerURL);
			sv["pack"] = new SciterValue(true);
			return sv;
		}
	}

	public enum EIconKind
	{
		LIBRARY,
		COLLECTION,
		STORE,
	}

	public class Icon
	{
		public Icon() { }

		public EIconKind kind;
		public string hash;
		public string id;// for SVG sprite
		public string path;
		public bool colored;
		public List<string> arr_svgpath = new List<string>();
		public List<string> arr_fill = new List<string>();
		public List<string> arr_tags = new List<string>();
		public BoundsD bounds;
		public SciterValue source;

		public SciterValue ToSV()
		{
			SciterValue sv = new SciterValue();
			sv["kind"] = new SciterValue((int)kind);
			sv["hash"] = new SciterValue(hash);
			sv["arr_tags"] = SciterValue.FromList(arr_tags);
			sv["source"] = source;
			sv["colored"] = new SciterValue(colored);
			if(kind == EIconKind.COLLECTION)
				sv["path"] = new SciterValue(path);
			return sv;
		}

		public bool EnsureIsLoaded()
		{
			switch(kind)
			{
				case EIconKind.LIBRARY:
					return true;
				case EIconKind.COLLECTION:
					return File.Exists(path);
				case EIconKind.STORE:
					if(!File.Exists(path))
					{
						try
						{
							Store.LoadIcon(this).Wait();
							return true;
						}
						catch(Exception)
						{
							return false;
						}
					}
					return true;
				default:
					Debug.Assert(false);
					break;
			}
			return false;
		}
	}
}