using AvalonEditB.Document;
using AvalonEditB.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextEditLib.Foldings
{
    public class IniFoldingStrategy : AbstractFoldingStrategy
    {
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            List<TextLine> lines = GetLines(document);
            List<NewFolding> folds = new List<NewFolding>();

            foreach (var fold in SectionFoldings(lines))
            {
                folds.Add(fold);
            }

            folds.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return folds;
        }

        private List<NewFolding> SectionFoldings(List<TextLine> lines)
        {
            List<NewFolding> folds = new List<NewFolding>();
            Stack<int> sectionStarts = new Stack<int>();
            Stack<string> sectionNames = new Stack<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                TextLine line = lines[i];
                Match m = Regex.Match(line.Text, @"\[(.*)\]", RegexOptions.Multiline);
                if (m.Success)
                {
                    // Start of a section
                    sectionStarts.Push(line.Offset);
                    sectionNames.Push(m.Groups[1].Value.Trim());
                }
                else if (sectionStarts.Any() && IsEndOfSection(i, lines))
                {
                    // End of the previous section
                    int start = sectionStarts.Pop();
                    string name = sectionNames.Pop();
                    int end = AdjustEndOffset(i, lines);
                    NewFolding fold = new NewFolding(start, end);
                    fold.Name = $"[{name}]";
                    fold.DefaultClosed = false;
                    folds.Add(fold);
                }
            }

            // Check if there's an unclosed section at the end
            if (sectionStarts.Any())
            {
                int start = sectionStarts.Pop();
                string name = sectionNames.Pop();
                int end = AdjustEndOffset(lines.Count - 1, lines);
                NewFolding fold = new NewFolding(start, end);
                fold.Name = $"[{name}]";
                fold.DefaultClosed = false;
                folds.Add(fold);
            }

            return folds;
        }

        private int AdjustEndOffset(int index, List<TextLine> lines)
        {
            // Find the last non-blank line before the section end
            for (int i = index; i >= 0; i--)
            {
                string trimmedLine = lines[i].Text.Trim();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    // Return the end offset of this line without including the newline character(s)
                    var a = lines[i].Length;
                    var b = lines[i].Text.TrimEnd().Length;
                    //return lines[i].Offset + lines[i].Length - lines[i].Text.TrimEnd().Length;
                    return lines[i].Offset+ lines[i].Text.TrimEnd().Length;
                }
            }

            // If no non-blank line was found, use the current line's offset
            return lines[index].Offset;
        }

        private bool IsEndOfSection(int index, List<TextLine> lines)
        {
            // Check if we're at the end of the file or the next line is another section
            if (index == lines.Count - 1 || Regex.Match(lines[index + 1].Text, @"\[(.*)\]", RegexOptions.Multiline).Success)
            {
                return true;
            }

            // Check if the next non-blank/empty line is another section
            for (int i = index + 1; i < lines.Count; i++)
            {
                string trimmedLine = lines[i].Text.Trim();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    if (Regex.Match(trimmedLine, @"\[(.*)\]", RegexOptions.Multiline).Success)
                    {
                        return true;
                    }
                    break;
                }
            }

            return false;
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
}
