using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconDrop.Data;

namespace IconDrop.Svg
{
	class SvgSpriteXML
	{
		public List<Icon> _icons = new List<Icon>();

		public void AddIcon(Icon icn)
		{
			_icons.Add(icn);
		}

		public string ToXML()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"<svg style=\"position: absolute; width: 0; height: 0; overflow: hidden;\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">");
			sb.AppendLine("\t<defs>");
			foreach(var icon in _icons)
			{
				string innerxml = null;
				if(icon.kind == EIconKind.COLLECTION || icon.kind == EIconKind.STORE)
					innerxml = IconFileXML(icon);

				string bounds = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", icon.bounds.l, icon.bounds.t, icon.bounds.w, icon.bounds.h);
				if(bounds == "0 0 0 0")
					sb.AppendLine($"\t\t<symbol id=\"{icon.id}\">");
				else
					sb.AppendLine($"\t\t<symbol id=\"{icon.id}\" viewBox=\"{bounds}\">");

				if(icon.kind == EIconKind.COLLECTION || icon.kind == EIconKind.STORE)
				{
					sb.AppendLine(innerxml);
				}
				else
				{
					sb.Append(IconLibraryPath(icon, "\t\t\t"));
				}
				sb.AppendLine("\t\t</symbol>");
			}
			sb.AppendLine("\t</defs>");
			sb.AppendLine("</svg>");

			return sb.ToString();
		}

		private static string IconFileXML(Icon icn)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(File.ReadAllText(icn.path));
			var node_svg = doc.LastChild;
			Debug.Assert(node_svg.Name == "svg");

			var viewBox = node_svg.Attributes["viewBox"];
			if(viewBox != null)
			{
				string[] bounds = viewBox.Value.Split(' ');
				if(bounds.Length == 4)
				{
					icn.bounds.l = double.Parse(bounds[0], CultureInfo.InvariantCulture);
					icn.bounds.t = double.Parse(bounds[1], CultureInfo.InvariantCulture);
					icn.bounds.w = double.Parse(bounds[2], CultureInfo.InvariantCulture);
					icn.bounds.h = double.Parse(bounds[3], CultureInfo.InvariantCulture);
				}
			}

			string xml = node_svg.InnerXml;
			xml = xml.Replace("&#xD;", "");
			xml = xml.Replace("&#xA;", "");
			return xml + "\n";
		}

		private static string IconLibraryPath(Icon icn, string indent)
		{
			StringBuilder sb = new StringBuilder();
			var i = 0;
			foreach(var path in icn.arr_svgpath)
			{
				if(icn.arr_fill.Count != 0)
				{
					string clr = icn.arr_fill[i++];
					sb.Append(indent);
					if(clr != "")
						sb.AppendLine($"<path fill=\"{clr}\" style=\"fill: {clr}\" d=\"{path}\"></path>");
					else
						sb.AppendLine($"<path d=\"{path}\"></path>");
				}
				else
					sb.AppendLine($"\t\t\t<path d=\"{path}\"></path>");
			}
			return sb.ToString();
		}

		public static string GetIconSymbolXML(Icon icn, string ID)
		{
			string innerxml;
			if(icn.kind == EIconKind.COLLECTION || icn.kind == EIconKind.STORE)
				innerxml = IconFileXML(icn);
			else
				innerxml = IconLibraryPath(icn, "\t");

			StringBuilder sb = new StringBuilder();
			string bounds = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", icn.bounds.l, icn.bounds.t, icn.bounds.w, icn.bounds.h);
			if(bounds == "0 0 0 0")
				sb.AppendLine($"<symbol id=\"{ID}\">");
			else
				sb.AppendLine($"<symbol id=\"{ID}\" viewBox=\"{bounds}\">");
			sb.Append("\t" + innerxml);
			sb.AppendLine("</symbol>");
			return sb.ToString();
		}
	}
}