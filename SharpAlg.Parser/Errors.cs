//
using System;
using System.Text;

namespace SharpAlg.Native.Parser {
    //(JsMode.Prototype, Filename = SR.JS_Parser)]
    public abstract class ErrorsBase {
        const string errMsgFormat = "Error at line {0} column {1}: {2}";
        public static string GetErrorText(int line, int column, string errorText) {
            return string.Format(errMsgFormat, line, column, errorText);
        }

        StringBuilder errorsBuilder = new StringBuilder();
        public int Count { get; private set; }
        public string Errors { get { return errorsBuilder.ToString(); } }
        

        public void SynErr(int line, int column, int parserErrorCode) {
            AppendLine(string.Format(errMsgFormat, line, column, GetErrorByCode(parserErrorCode)));
            Count++;
        }

        public void SemErr(int line, int column, string errorText) {
            AppendLine(GetErrorText(line, column, errorText));
            Count++;
        }

        public void SemErr(string s) {
            AppendLine(s);
            Count++;
        }

        public void Warning(int line, int column, string errorText) {
            AppendLine(string.Format(errMsgFormat, line, column, errorText));
        }

        public void Warning(string warningText) {
            AppendLine(warningText);
        }
        void AppendLine(string s) {
            errorsBuilder.Append(s);
            errorsBuilder.Append("\r\n");
        }
        protected abstract string GetErrorByCode(int n);
    }
    public class FatalError : Exception {
        public FatalError(string m) : base(m) { }
    }
}
