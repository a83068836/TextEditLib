using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AvalonEditB.Document;
using AvalonEditB.Folding;
using AvalonEditB.Highlighting;
using TextEditLib.Foldings;

namespace TextEditLib
{
	public abstract class AbstractFoldingStrategy
	{
		public void UpdateFoldings(FoldingManager manager, TextDocument document)
		{
			int firstErrorOffset;
			IEnumerable<NewFolding> foldings = CreateNewFoldings(document, out firstErrorOffset);
			manager.UpdateFoldings(foldings, firstErrorOffset);
		}
		public abstract IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset);

		
	}
    public class legendFoldingStrategy : AbstractFoldingStrategy
	{
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			List<TextLine> lines = GetLines(document);
			List<NewFolding> folds = new List<NewFolding>();

			NewFolding usingFold = UsingFolding(lines);
			if (usingFold != null) folds.Add(usingFold);

			folds.AddRange(BraceFoldings(document));
			folds.AddRange(RegionFoldings(lines));
			folds.AddRange(NoteFoldings(lines));
			folds.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
			return folds;
		}

		private NewFolding UsingFolding(List<TextLine> lines)
		{
			int start = -1;
			for (int i = 0; i < lines.Count; i++) {
				TextLine line = lines[i];
				if (Regex.Match(line.Text, "using").Success) {
					if (start < 0) start = line.Offset;
				} else {
					if (start >= 0) {
						NewFolding fold = new NewFolding(start, line.Offset);// + line.Length + 1
						fold.Name = "using";
						fold.DefaultClosed = false;
						return fold;
					}
				}
			}
			return null;
		}

		private List<NewFolding> BraceFoldings(ITextSource doc)
		{
			List<NewFolding> folds = new List<NewFolding>();
			Stack<int> starts = new Stack<int>();
			int lastNewLineOffset = 0;
			char open = '{';
			char close = '}';
			if (doc != null)
			{
                for (int i = 0; i < doc.TextLength; i++)
                {
                    char c = doc.GetCharAt(i);
                    if (c == open)
                    {
                        starts.Push(i);
                    }
                    else if (c == close && starts.Count > 0)
                    {
                        int start = starts.Pop();
                        if (start < lastNewLineOffset)
                        {
                            folds.Add(new NewFolding(start, i + 1));
                        }
                    }
                    else if (c == '\n' || c == '\r')
                    {
                        lastNewLineOffset = i + 1;
                    }
                }
            }
			return folds;
		}

		private List<NewFolding> RegionFoldings(List<TextLine> lines)
		{
			List<NewFolding> folds = new List<NewFolding>();
			Stack<int> starts = new Stack<int>();
			Stack<string> names = new Stack<string>();
			for (int i = 0; i < lines.Count; i++) {
				TextLine line = lines[i];
				Match m = Regex.Match(line.Text, @"\[@(.*?)\]", RegexOptions.Multiline);
				if (m.Success) {
					starts.Push(line.Offset + m.Index);
					names.Push(m.Groups[1].Value);
				}//^\\s*
				if (i < lines.Count - 1) {
					TextLine line1 = lines[i + 1];
					Match m1 = Regex.Match(line1.Text, @"\[@(.*?)\]");
					if (m1.Value != m.Value) {
						if (m1.Success && starts.Count > 0) {
							int start = starts.Pop();
							string trim = Regex.Replace(lines[i].Text, @"\s", "");
							int a = 0;
							int b = line.Offset + m.Index + m.Length;
							while (trim == "") {
								a++;
								trim = Regex.Replace(lines[i - a].Text, @"\s", "");
								b = lines[i - a + 1].Offset;
							}
							NewFolding fold = new NewFolding(start, b - 2);
							fold.Name = names.Pop();
							fold.DefaultClosed = false;
							folds.Add(fold);
						}
					}
				} else if (i == lines.Count - 1) {
					if (names.Count>0) {
						int start = starts.Pop();

						string trim = Regex.Replace(lines[i].Text, @"\s", "");
						int a = 0;
						int b = line.Offset + line.Text.Length;
						while (trim == "") {
							a++;
							trim = Regex.Replace(lines[i - a].Text, @"\s", "");
							b = lines[i - a + 1].Offset-2;
						}

						NewFolding fold = new NewFolding(start, b);
						fold.Name = names.Pop();
						fold.DefaultClosed = false;
						folds.Add(fold);
					}
				}
				
			}
			return folds;
		}

		private List<NewFolding> NoteFoldings(List<TextLine> lines)
		{
			List<NewFolding> folds = new List<NewFolding>();
			Stack<int> starts = new Stack<int>();
			Stack<string> names = new Stack<string>();
			int startLine = -1;
			for (int i = 0; i < lines.Count; i++) {
				TextLine line = lines[i];
				Match m = Regex.Match(line.Text, "/{2,}(\\s.+)$", RegexOptions.Multiline);
				if (m.Success) {
					if (startLine < 0) {
						starts.Push(line.Offset + m.Index);
						names.Push(m.Groups[1].Value);
						startLine = i;
					}
				} else if (starts.Count > 0) {
					int start = starts.Pop();
					string name = names.Pop();
					if (i - startLine > 1) {
						m = Regex.Match(line.Text, "^\\s*", RegexOptions.Multiline);
						NewFolding fold = new NewFolding(start, line.Offset + (m.Success ? m.Length : 4));
						fold.Name = "…";
						fold.DefaultClosed = false;
						folds.Add(fold);
						startLine = -1;
					}
				}
			}

			return folds;
		}

		List<TextLine> GetLines(ITextSource doc)
		{
			List<TextLine> lines = new List<TextLine>();
			int offset = 0;
			int end = 0;
			if (doc != null)
			{
                for (int i = 0; i < doc.TextLength; i++)
                {
                    char c = doc.GetCharAt(i);
                    if (c == '\r')
                    {
                        end = i + 1;
                        lines.Add(new TextLine(offset, end - offset, doc.GetText(offset, end - offset)));
                        offset = end + 1;
                    }
                    if (i == doc.TextLength - 1)
                    {
                        end = i + 1;
                        lines.Add(new TextLine(offset, end - offset, doc.GetText(offset, end - offset)));
                        offset = end;
                    }
                }
            }
			return lines;
		}
	}

	public class TextLine
	{
		public int Offset { get; set; }
		public int Length { get; set; }
		public string Text { get; set; }

		public TextLine()
		{

		}

		public TextLine(int start, int length, string t)
		{
			this.Offset = start;
			this.Length = length;
			this.Text = t;
		}
	}
}




