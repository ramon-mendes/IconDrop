using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using IconDrop.Data;
using IconDrop.Svg;

namespace IconDrop.Hosting
{
	/*public class SKIcon
	{
		public SKRect bounds;
		public string[] paths;
		public string[] fills;
	}*/

	public class SKIconCode
	{
		public static BoundsD NormalizeIcon(Icon icn, int size)
		{
			double max = icn.bounds.w > icn.bounds.h ? icn.bounds.w : icn.bounds.h;
			float factor = size / (float)max;
			BoundsD bounds = new BoundsD()
			{
				l = double.MaxValue,
				t = double.MaxValue
			};
			foreach(var path in icn.arr_svgpath)
			{
				SvgParser parser = SvgParser.FromPath(path);
				parser._scaler.OffsetXY((float)-icn.bounds.l, (float)-icn.bounds.t);
				parser._scaler.Scale(factor);

				if(parser._bounds.w > bounds.w)
					bounds.w = parser._bounds.w;
				if(parser._bounds.h > bounds.h)
					bounds.h = parser._bounds.h;
				if(parser._bounds.l < bounds.l)
					bounds.l = parser._bounds.l;
				if(parser._bounds.t < bounds.t)
					bounds.t = parser._bounds.t;
			}
			return bounds;
		}

		public static string IconToCode(Icon icn)
		{
			StringBuilder sb = new StringBuilder();
			string name = ToCamelCaseName("icon " + icn.arr_tags[0]);
			sb.AppendLine($"public static SKIcon {name} = new SKIcon()");
			sb.AppendLine("{");

			// .tbounds
			string bounds = string.Format(CultureInfo.InvariantCulture, "{0}f, {1}f, {2}f, {3}f",
				icn.bounds.l,
				icn.bounds.t,
				icn.bounds.w + icn.bounds.l,// SKRect constructor takes width and height
				icn.bounds.h + icn.bounds.t);
			sb.AppendLine($"\tbounds = new SKRect({bounds}),");

			// .paths
			sb.Append("\tpaths = new string[] { ");
			foreach(var svgpath in icn.arr_svgpath)
				sb.Append($"\"{svgpath}\", ");
			sb.AppendLine("},");

			// .fills
			sb.Append("\tfills = new string[] { ");
			foreach(var fill in icn.arr_fill)
				sb.Append($"\"{fill}\", ");
			sb.AppendLine("},");

			sb.AppendLine("};");

			return sb.ToString();
		}

		private static string ToCamelCaseName(string name)
		{
			TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
			name = txtInfo.ToTitleCase(name);
			name = name
			              .Replace("-", string.Empty)
			              .Replace("_", string.Empty)
			              .Replace(" ", string.Empty);
			name = $"{name.First().ToString().ToLowerInvariant()}{name.Substring(1)}";
			return name;
		}
	}
}