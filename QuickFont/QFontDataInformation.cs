using System.Collections.Generic;

namespace QuickFont
{
	public class QFontDataInformation
	{
		public QFontDataInformation (int count, char[] charSet)
		{
			PageCount = count;
			CharSet = charSet;
		}

		public int PageCount {get; private set;}
		public char[] CharSet {get; private set;}

		public List<string> GenerateBitmapPageNames(string filePath)
		{
			string namePrefix = filePath.Replace (".qfont", "").Replace (" ", "");
			var result = new List<string> ();
			if (PageCount == 1)
			{
				result.Add (namePrefix + ".png");
			}
			else
			{
				for (int i = 0; i < PageCount; i++)
				{
					result.Add (namePrefix + "_sheet_" + i);
				}
			}
			return result;
		}
	}
}

