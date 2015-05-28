/*----------------------------------------------------------------------
SharpAlg Parser
-----------------------------------------------------------------------*/

using System;
using System.IO;
using System.Collections;
//
using System.Collections.Generic;

namespace SharpAlg.Native.Parser {

//(JsMode.Clr, Filename = SR.JS_Parser)]
public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 13;
	const int noSym = 13;


	public Buffer buffer; // scanner buffer
	
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int charPos;      // position by unicode characters starting with 0
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Dictionary<int, int> start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Dictionary<int, int>();
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 4;
		start[46] = 2; 
		start[45] = 5; 
		start[43] = 6; 
		start[42] = 7; 
		start[47] = 8; 
		start[94] = 9; 
		start[33] = 10; 
		start[40] = 11; 
		start[41] = 12; 
		start[44] = 13; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (string source) {
		buffer = new Buffer(source);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0; charPos = -1;
		oldEols = 0;
		NextCh();
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			// buffer reads unicode chars, if UTF8 has been detected
			ch = buffer.Read(); col++; charPos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
            //TODO not tested, ugly
			char[] newBuf = new char[2 * tval.Length];
            for(int i = 0; i < tval.Length; i++) {
                newBuf[i] = tval[i];
            }
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = GetCurrentChar();
			NextCh();
		}
	}

    char GetCurrentChar() {
        return PlatformHelper.IntToChar(ch);
    }




	void CheckLiteral() {
		switch (t.val) {
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();

		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
/*old way		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
<--scan3
		}
old way*/
//new way begin
        bool done = false;
        while(!done) {
		switch (state) {
			case -1: { t.kind = eofSym; done = true; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; done = true; break;
			} // NextCh already done
			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); state = 1; break; }
				else {t.kind = 1; done = true; break;}
			case 2:
				if (ch >= '0' && ch <= '9') { AddCh(); state = 3; break; }
				else { state = 0; break; }
			case 3:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') { AddCh(); state = 3; break; }
				else {t.kind = 3; done = true; break;}
			case 4:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') { AddCh(); state = 4; break; }
				else if (ch == '.') { AddCh(); state = 2; break; }
				else {t.kind = 2; done = true; break;}
			case 5:
				{t.kind = 4; done = true; break;}
			case 6:
				{t.kind = 5; done = true; break;}
			case 7:
				{t.kind = 6; done = true; break;}
			case 8:
				{t.kind = 7; done = true; break;}
			case 9:
				{t.kind = 8; done = true; break;}
			case 10:
				{t.kind = 9; done = true; break;}
			case 11:
				{t.kind = 10; done = true; break;}
			case 12:
				{t.kind = 11; done = true; break;}
			case 13:
				{t.kind = 12; done = true; break;}

		}
        }
//new way end
        t.val = string.Empty;//TODO performance!!!
        for(int i = 0; i < tlen; i++) {
            t.val += tval[i];    
        }
		//t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col; charPos = t.charPos;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
			if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}

	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner
}