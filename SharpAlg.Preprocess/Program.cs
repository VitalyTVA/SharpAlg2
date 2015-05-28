using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.IO;

namespace SharpAlg.Preprocess {
    public class Program {
        const string path = @"..\..";
        static void Main(string[] args) {
            PatchScanner();

            PatchParser();
        }
        static void PatchParser() {
            string parserFileName = Path.Combine(path, @"Parser.cs");
            string parser = File.ReadAllText(parserFileName);
            int startIndex = parser.IndexOf("//parser set patch begin");
            int endIndex = parser.IndexOf("//parser set patch end");
            string patch = parser.Substring(startIndex, endIndex - startIndex);
            string patch2 = PatchParser(patch);
            parser = parser.Replace(patch, patch2);

            RewriteFile(parserFileName, parser);
        }
        static void PatchScanner() {
            
            string scannerFileName = Path.Combine(path, @"Scanner.cs");
            string scanner = File.ReadAllText(scannerFileName);
            scanner = scanner.Replace("tval[tlen++] = (char) ch;", "tval[tlen++] = GetCurrentChar();");

            int startIndex = scanner.IndexOf("//new way begin");
            int endIndex = scanner.IndexOf("//new way end");
            string patch = scanner.Substring(startIndex, endIndex - startIndex);
            string patch2 = PatchScanner(patch);
            scanner = scanner.Replace(patch, patch2);

            RewriteFile(scannerFileName, scanner);
        }
        static void RewriteFile(string fileName, string text) {
            string finalFileName = fileName.Replace(".cs", "_.cs");
            string oldText = null;
            if(File.Exists(finalFileName))
                oldText = File.ReadAllText(finalFileName);
            if(oldText != text)
                File.WriteAllText(finalFileName, text);
            File.Delete(fileName);
        }
        public static string PatchScanner(string s) {
            string regex = @"{AddCh\(\); goto case (\d+);}";
            MatchCollection matches = Regex.Matches(s, regex);
            string result = s.Replace("break;", "done = true; break;");
            result = Regex.Replace(result, regex, "{ AddCh(); state = $1; break; }");

            regex = @"{goto case (\d+);}";
            matches = Regex.Matches(s, regex);
            result = Regex.Replace(result, regex, "{ state = $1; break; }");
            return result;
        }
        public static string PatchParser(string s) {
            return s.Replace("		{", "		new bool[] {");
        }

    }
    [TestFixture]
    public class Tests {
        [Test]
        public void PatchParser() {
            string s = @"
static readonly bool[][] set = {
		{T,x,x,x, x,x,x,x, x},
		{x,x,x,T, T,T,T,x, x}

	};
";
            string expected = @"
static readonly bool[][] set = {
		new bool[] {T,x,x,x, x,x,x,x, x},
		new bool[] {x,x,x,T, T,T,T,x, x}

	};
";
            string result = Program.PatchParser(s);
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void PatchScanner() {
            string s = @"
switch(state) {
case -1: { t.kind = eofSym; break; } // NextCh already done
case 0: {
if(recKind != noSym) {
tlen = recEnd - t.pos;
SetScannerBehindT();
}
t.kind = recKind; break;
} // NextCh already done
case 1:
recEnd = pos; recKind = 1;
if(ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
else { t.kind = 1; break; }
case 2:
recEnd = pos; recKind = 2;
if(ch >= '0' && ch <= '9') {goto case 2;}
else { t.kind = 2; break; }
case 3:
{ t.kind = 3; break; }
}
";
        string expected = @"
switch(state) {
case -1: { t.kind = eofSym; done = true; break; } // NextCh already done
case 0: {
if(recKind != noSym) {
tlen = recEnd - t.pos;
SetScannerBehindT();
}
t.kind = recKind; done = true; break;
} // NextCh already done
case 1:
recEnd = pos; recKind = 1;
if(ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); state = 1; break; }
else { t.kind = 1; done = true; break; }
case 2:
recEnd = pos; recKind = 2;
if(ch >= '0' && ch <= '9') { state = 2; break; }
else { t.kind = 2; done = true; break; }
case 3:
{ t.kind = 3; done = true; break; }
}
";
        string result = Program.PatchScanner(s);
        Assert.AreEqual(expected, result);
        }
    }
}
