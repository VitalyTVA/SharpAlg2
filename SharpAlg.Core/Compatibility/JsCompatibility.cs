#if PORTABLE
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpKit.JavaScript {
    public enum JsMode { None, Prototype, Clr, Json }
    public class JsTypeAttribute : Attribute {
        public JsTypeAttribute(JsMode mode) {
        }
        public string Filename { get; set; }
    }
    public class JsMethodAttribute : Attribute {
        public JsMethodAttribute() {
        }
        public string Code { get; set; }
    }
}
#endif