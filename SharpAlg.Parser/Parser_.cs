/*----------------------------------------------------------------------
SharpAlg Parser
-----------------------------------------------------------------------*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
//
using SharpAlg.Native.Builder;

namespace SharpAlg.Native.Parser {


//(JsMode.Clr, Filename = SR.JS_Parser)]
class ArgsList : List<Expr> { }
//(JsMode.Prototype, Filename = SR.JS_Parser)]
public class Parser {
	public const int _EOF = 0;
	public const int _identifier = 1;
	public const int _number = 2;
	public const int _floatNumber = 3;
	public const int maxT = 13;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;


	public Expr Expr { get; private set; }
	readonly ExprBuilder builder;
	public Parser(Scanner scanner, ExprBuilder builder) {
		this.scanner = scanner;
		errors = new Errors();
		this.builder = builder;
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s][la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol][kind] || set[repFol][kind] || set[0][kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void SharpAlg() {
		Expr expr; 
		AdditiveExpression(out expr);
		this.Expr = expr; 
	}

	void AdditiveExpression(out Expr expr) {
		bool leftMinus = false, rightMinus; Expr rightExpr = null; 
		if (la.kind == 4) {
			Get();
			leftMinus = true; 
		}
		MultiplicativeExpression(out expr);
		expr = leftMinus ? builder.Minus(expr) : expr; 
		while (la.kind == 4 || la.kind == 5) {
			AdditiveOperation(out rightMinus);
			MultiplicativeExpression(out rightExpr);
			expr = builder.Add(expr, (rightMinus ? builder.Minus(rightExpr) : rightExpr)); 
		}
	}

	void MultiplicativeExpression(out Expr expr) {
		bool divide; Expr rightExpr; 
		PowerExpression(out expr);
		while (la.kind == 6 || la.kind == 7) {
			MultiplicativeOperation(out divide);
			PowerExpression(out rightExpr);
			expr = builder.Multiply(expr, (divide ? builder.Inverse(rightExpr) : rightExpr)); 
		}
	}

	void AdditiveOperation(out bool minus) {
		minus = false; 
		if (la.kind == 5) {
			Get();
		} else if (la.kind == 4) {
			Get();
			minus = true; 
		} else SynErr(14);
	}

	void PowerExpression(out Expr expr) {
		Expr rightExpr; 
		FactorialExpression(out expr);
		while (la.kind == 8) {
			Get();
			FactorialExpression(out rightExpr);
			expr = builder.Power(expr, rightExpr); 
		}
	}

	void MultiplicativeOperation(out bool divide) {
		divide = false; 
		if (la.kind == 6) {
			Get();
		} else if (la.kind == 7) {
			Get();
			divide = true; 
		} else SynErr(15);
	}

	void FactorialExpression(out Expr expr) {
		Terminal(out expr);
		while (la.kind == 9) {
			Get();
			expr = builder.Function("factorial", expr.AsEnumerable()); 
		}
	}

	void Terminal(out Expr expr) {
		expr = null; 
		if (la.kind == 2) {
			Get();
			expr = Expr.Constant(NumberFactory.FromIntString(t.val)); 
		} else if (la.kind == 3) {
			Get();
			expr = Expr.Constant(NumberFactory.FromString(t.val)); 
		} else if (la.kind == 10) {
			Get();
			AdditiveExpression(out expr);
			Expect(11);
		} else if (la.kind == 1) {
			FunctionCall(out expr);
		} else SynErr(16);
	}

	void FunctionCall(out Expr expr) {
		string name; ArgsList args = null; 
		Expect(1);
		name = t.val; 
		while (la.kind == 10) {
			ArgumentList(ref args);
		}
		expr = args != null ? (Expr)builder.Function(name, args) : builder.Parameter(name); 
	}

	void ArgumentList(ref ArgsList args) {
		args = new ArgsList(); 
		Expect(10);
		while (StartOf(1)) {
			List(args);
		}
		Expect(11);
	}

	void List(ArgsList args) {
		Expr first; 
		AdditiveExpression(out first);
		args.Add(first); 
		while (la.kind == 12) {
			Get();
			List(args);
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		try {
		SharpAlg();
		Expect(0);

		} catch(Exception e) {
			errors.SemErr(e.GetMessage());
		}
	}
/*original set	
	static readonly bool[,] set = {
<--initialization
	};
*/
//parser set patch begin
	static readonly bool[][] set = {
		new bool[] {T,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		new bool[] {x,T,T,T, T,x,x,x, x,x,T,x, x,x,x}

	};
//parser set patch end
} // end Parser

//(JsMode.Prototype, Filename = SR.JS_Parser)]
public class Errors : ErrorsBase {
    protected override string GetErrorByCode(int n) {
        string s;
        switch(n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "identifier expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "floatNumber expected"; break;
			case 4: s = "\"-\" expected"; break;
			case 5: s = "\"+\" expected"; break;
			case 6: s = "\"*\" expected"; break;
			case 7: s = "\"/\" expected"; break;
			case 8: s = "\"^\" expected"; break;
			case 9: s = "\"!\" expected"; break;
			case 10: s = "\"(\" expected"; break;
			case 11: s = "\")\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "??? expected"; break;
			case 14: s = "invalid AdditiveOperation"; break;
			case 15: s = "invalid MultiplicativeOperation"; break;
			case 16: s = "invalid Terminal"; break;

            default: s = "error " + n; break;
        }
        return s;
    }
}}