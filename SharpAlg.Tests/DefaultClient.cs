using SharpKit.JavaScript;
using SharpKit.Html;
using SharpKit.jQuery;
using SharpAlg.Native;
using System;
using SharpAlg.Tests;
using System.Reflection;

namespace SharpAlg {
    [JsType(JsMode.Global, Filename = "res/Default.js")]
    public class DefaultClient {
        static void DefaultClient_Load() {
            SharpKit.JavaScript.Compilation.JsCompiler.Compile();

        }
        static void btnTest_click(DOMEvent e) {
            RunTests();
        }
        static void RunTests() {
            var fixtures = new object[] {
                    new ExprTests(),
                    new DiffTests(),
                    new ParserTests(),
                    new NumberTests(),
                    new FunctionsTests()
                };
            jQuery jQuery = new jQuery(HtmlContext.document.body);
            jQuery.append("<br/>");
            int ok = 0, failed = 0;
            foreach(var fixture in fixtures) {
                MethodInfo[] methods = fixture.GetType().GetMethods();
                foreach(var method in methods) {
                    if(method.Name.EndsWith("Test")) {
                        if(RunTest(jQuery, fixture, method))
                            ok++;
                        else
                            failed++;
                    }
                }
            }
            jQuery.append(string.Format("<br/>TOTAL {0}<br/>PASSED: {1}<br/>FAILED: {2}<br/>", (ok + failed), ok, failed));
        }
        static bool RunTest(jQuery jQuery, object fixture, MethodInfo method) {
            string status = "OK";
            bool success = true;
            try {
                method.Invoke(fixture, null);
            } catch(Exception e) {
                status = "Failure: " + e;
                success = false;
            }
            jQuery.append(fixture.GetType().Name + "." + method.Name + ": " + status + "<br/>");
            return success;
        }
    }
}