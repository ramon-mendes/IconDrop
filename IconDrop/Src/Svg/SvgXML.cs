using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconDrop.Data;

namespace IconDrop.Svg
{
	public class SvgXML
	{
		public List<string> _paths = new List<string>();
		public List<string> _fills = new List<string>();
		private BoundsD _bounds;

		private SvgXML() {}
		
		public static SvgXML FromIcon(Icon icon)
		{
			var svg = new SvgXML();

			int ipath = 0;
			foreach(var svgpath in icon.arr_svgpath)
			{
				svg._paths.Add(svgpath);
				if(icon.arr_fill.Count != 0)
					svg._fills.Add(icon.arr_fill[ipath++]);
			}

			svg._bounds = icon.bounds;
			return svg;
		}

		public static SvgXML FromSvgParser(SvgParser parser)
		{
			var svg = new SvgXML();
			svg._paths.Add(parser._scaler.ToPath());
			svg._bounds = parser._bounds;
			return svg;
		}
		public void Scale(float factor)
		{
			for(int i = 0; i < _paths.Count; i++)
			{
				var sp = SvgParser.FromPath(_paths[i]);
				sp._scaler.Scale(factor);
				_paths[i] = sp._scaler.ToPath();
			}
			_bounds.l *= factor;
			_bounds.t *= factor;
			_bounds.w *= factor;
			_bounds.h *= factor;
		}

		public string ToXML(bool white = false)
		{
			string bounds = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", _bounds.l, _bounds.t, _bounds.w, _bounds.h);

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			if(white)
				sb.AppendLine($"<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"{bounds}\" fill=\"white\">");
			else
				sb.AppendLine($"<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"{bounds}\">");
			if(_fills.Count != 0)
			{
				int ipath = 0;
				foreach(var path in _paths)
					sb.AppendLine($"<path d=\"{path}\" fill=\"{_fills[ipath++]}\" />");
			}
			else
			{
				foreach(var path in _paths)
					sb.AppendLine($"<path d=\"{path}\" />");//fill=\"#000000\" style=\"fill: #000000;\" 
			}
			
			sb.AppendLine("</svg>");
			return sb.ToString();
		}
	}
}