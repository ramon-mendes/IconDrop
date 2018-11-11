using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IconDrop
{
	static class Utils
	{
		private static Random rng = new Random();
		private static MD5 md5 = MD5.Create();

		public static void CopyText(string text)
		{
#if WINDOWS
			Debug.Assert(text.Length != 0);
			var dataObject = new System.Windows.Forms.DataObject();
			dataObject.SetText(text);
			try
			{
				System.Windows.Forms.Clipboard.SetDataObject(dataObject, true, 100, 10);
			}
			catch(Exception)
			{
			}
#else
			AppKit.NSPasteboard.GeneralPasteboard.ClearContents();
			AppKit.NSPasteboard.GeneralPasteboard.SetDataForType(Foundation.NSData.FromString(text), AppKit.NSPasteboard.NSStringType);
#endif
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while(n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static DateTime FromUnixTime(this long unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(unixTime);
		}

		public static int ToUnixEpoch(this DateTime dt)
		{
			int unixTimestamp = (int)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
			return unixTimestamp;
		}

		public static string CalculateMD5Hash(string input)
		{
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("X2"));
			return sb.ToString();
		}
	}
}