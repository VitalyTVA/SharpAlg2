using System;
using System.IO;
using System.Collections;
using SharpKit.JavaScript;

namespace SharpAlg.Native.Parser {
    [JsType(JsMode.Prototype, Filename = SR.JS_Parser)]
    public class Buffer {
        public const int EOF = char.MaxValue + 1;

        string source;

        public Buffer(string source) {
            this.source = source;
        }

        public virtual int Read() {
            if(Pos < source.Length) {
                Pos++;
                return PlatformHelper.CharToInt(source[Pos - 1]);
            }
            return EOF;
        }
        public int Peek() {
            int curPos = Pos;
            int ch = Read();
            Pos = curPos;
            return ch;
        }

        //// beg .. begin, zero-based, inclusive, in byte
        //// end .. end, zero-based, exclusive, in byte
        //public string GetString(int beg, int end) {
        //}

        int pos;
        public int Pos {
            get { return pos; }
            set {
                if(value < 0 || value > source.Length) {
                    throw new FatalError(SR.STR_Parser_BufferOutOfBoundsAccessPosition + value);
                }
                pos = value;
            }
        }
    }
}
