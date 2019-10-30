using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.AppUtils
{

    public class Expression
    {
        Dictionary<string, double> vars = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);

        ExpressionToken root = null;

        delegate ExpressionToken ParseFunc(List<ExpressionToken> toks, ref int pos);

        public Expression(string expr)
        {
            List<ExpressionToken> toks = LexTokens(expr);
            root = ParseTokens(toks);
        }
        
        public double Eval()
        {
            if (root == null)
                return 0.0;
            return root.Eval(this);
        }

        public int EvalInt()
        {
            if (root == null)
                return 0;
            return root.EvalInt(this);
        }

        ExpressionToken ParseFactor(List<ExpressionToken> toks,ref int pos) {
            var kind = GetKind(toks, pos);
            ExpressionToken res;
            if(kind==TokenKind.Identifier || kind==TokenKind.Number) {
                res=toks[pos];
                pos++;
            }
            else if (kind == TokenKind.Sub) //unary negation
            {
                res = toks[pos];
                pos++;
                res.Tok1 = ParseFactor(toks, ref pos);
            }
            else if (kind==TokenKind.LPar) {
                pos++;  //skip (
                res = ParseRoot(toks, ref pos);
                if (GetKind(toks, pos) != TokenKind.RPar)
                    throw new ExpressionException("Missing end parenthesis");
                pos++; //skip )
            }
            else
            {
                throw new ExpressionException("Syntax error");
            }
            
            return res;
        }


      
        ExpressionToken ParseMulDivMod(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseFactor, TokenKind.Mul, TokenKind.Div,TokenKind.Mod);
        }




        ExpressionToken ParseAddSub(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseMulDivMod, TokenKind.Add, TokenKind.Sub);
        }

        
        ExpressionToken ParseShlShr(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseAddSub, TokenKind.Shl,TokenKind.Shr);
        }


        ExpressionToken ParseLessGreater(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseShlShr, TokenKind.Le,TokenKind.Ge,TokenKind.Leq,TokenKind.Geq);
        }

        ExpressionToken ParseEqNotEq(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseLessGreater, TokenKind.Eq,TokenKind.Neq);
        }


        ExpressionToken ParseBitAnd(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseEqNotEq, TokenKind.BitAnd);
        }


        ExpressionToken ParseBitXOr(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseBitAnd, TokenKind.BitXor);
        }


        ExpressionToken ParseBitOr(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseBitXOr, TokenKind.BitOr);
        }


        ExpressionToken ParseAnd(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseBitOr, TokenKind.And);
        }


        ExpressionToken ParseOr(List<ExpressionToken> toks, ref int pos)
        {
            return ParseBinary(toks, ref pos, ParseAnd, TokenKind.Or);
        }

        ExpressionToken ParseBinary(List<ExpressionToken> toks, ref int pos, ParseFunc nextlevel, params TokenKind[] kinds)
        {
            var res = nextlevel(toks, ref pos);

            while (true)
            {
                var kind = GetKind(toks, pos);
                if (Array.IndexOf(kinds,kind)>=0)
                {
                    var newres = toks[pos];
                    newres.Tok1 = res;
                    pos++;
                    newres.Tok2 = nextlevel(toks, ref pos);
                    res = newres;
                }
                else break;
            }

            return res;
        }

        ExpressionToken ParseIf(List<ExpressionToken> toks, ref int pos)
        {
            var res = ParseOr(toks, ref pos);
            if (GetKind(toks, pos) == TokenKind.If)
            {
                ExpressionToken newres = toks[pos];
                newres.Tok1 = res;
                pos++;  // skip ?
                newres.Tok2 = ParseOr(toks, ref pos);
                if (GetKind(toks, pos) != TokenKind.Colon)
                    throw new ExpressionException("Colon expected after ?");
                pos++;  //skip :
                newres.Tok3 = ParseOr(toks, ref pos);
                res = newres;
            }

            return res;
        }

        ExpressionToken ParseRoot(List<ExpressionToken> toks, ref int pos)
        {
            return ParseIf(toks, ref pos);
        }

        TokenKind GetKind(List<ExpressionToken> toks,int pos) {
            if(pos>=toks.Count)
                return TokenKind.None;
            return toks[pos].Kind;
        }


        private ExpressionToken ParseTokens(List<ExpressionToken> toks)
        {
            int pos=0;
            ExpressionToken res = ParseIf(toks,ref pos);

            if (pos != toks.Count) //make sure we parsed evrything => garbage after expression not allowed
                throw new ExpressionException("Unexpected token");

            return res;
        }

        public void SetVar(string name, double value) {vars[name] = value;}

        public double GetVar(string name)
        {
            double res;
            if (vars.TryGetValue(name, out res))
                return res;

            throw new ExpressionException("Unknown variable '" + name+"'");
        }


        private char Get(string str, int pos)
        {
            if (pos >= str.Length)
                return '\0';
            return str[pos];
        }

        private bool ParseFor(ref string str, string what)
        {
            if (str.StartsWith(what))
            {
                str = str.Substring(what.Length);
                return true;
            }

            return false;
        }


        private List<ExpressionToken> LexTokens(string expr)
        {
            List<ExpressionToken> res = new List<ExpressionToken>();

            while (true)
            {
                ParseUtils.SkipWhite(ref expr);

                if (expr == "") break;
                char ch = expr[0];


                if (ParseFor(ref expr, "+")) res.Add(new ExpressionToken(TokenKind.Add));
                else if (ParseFor(ref expr, "-")) res.Add(new ExpressionToken(TokenKind.Sub));
                else if (ParseFor(ref expr, "*")) res.Add(new ExpressionToken(TokenKind.Mul));
                else if (ParseFor(ref expr, "/")) res.Add(new ExpressionToken(TokenKind.Div));
                else if (ParseFor(ref expr, "(")) res.Add(new ExpressionToken(TokenKind.LPar));
                else if (ParseFor(ref expr, ")")) res.Add(new ExpressionToken(TokenKind.RPar));
                else if (ParseFor(ref expr, ":")) res.Add(new ExpressionToken(TokenKind.Colon));
                else if (ParseFor(ref expr, "&&")) res.Add(new ExpressionToken(TokenKind.And));
                else if (ParseFor(ref expr, "&")) res.Add(new ExpressionToken(TokenKind.BitAnd));
                else if (ParseFor(ref expr, "||")) res.Add(new ExpressionToken(TokenKind.Or));
                else if (ParseFor(ref expr, "|")) res.Add(new ExpressionToken(TokenKind.BitOr));
                else if (ParseFor(ref expr, "^")) res.Add(new ExpressionToken(TokenKind.BitXor));
                else if (ParseFor(ref expr, "<<")) res.Add(new ExpressionToken(TokenKind.Shl));
                else if (ParseFor(ref expr, ">>")) res.Add(new ExpressionToken(TokenKind.Shr));
                else if (ParseFor(ref expr, "?")) res.Add(new ExpressionToken(TokenKind.If));
                else if (ParseFor(ref expr, "%")) res.Add(new ExpressionToken(TokenKind.Mod));
                else if (ParseFor(ref expr, "<=")) res.Add(new ExpressionToken(TokenKind.Leq));
                else if (ParseFor(ref expr, "<")) res.Add(new ExpressionToken(TokenKind.Le));
                else if (ParseFor(ref expr, ">=")) res.Add(new ExpressionToken(TokenKind.Geq));
                else if (ParseFor(ref expr, ">")) res.Add(new ExpressionToken(TokenKind.Ge));
                else if (ParseFor(ref expr, "!=")) res.Add(new ExpressionToken(TokenKind.Neq));
                else if (ParseFor(ref expr, "==")) res.Add(new ExpressionToken(TokenKind.Eq));
                else  //nothing good parsed so far, try for identifiers and numbers
                {
                    string ident;
                    double num;
                    if (ParseUtils.ParseIdentifier(ref expr, out ident))
                        res.Add(new ExpressionToken(TokenKind.Identifier) { Ident = ident });
                    else if (ParseUtils.ParseDouble(ref expr, out num))
                        res.Add(new ExpressionToken(TokenKind.Number) { Num = num });
                    else
                        throw new ExpressionException("Invalid expression");
                }
            }

            return res;
        }


        enum TokenKind
        {
            None,
            Identifier,
            Number,
            Mul,    // *
            Div,    // /
            Add,    // +
            Sub,    // -
            Eq,     // ==
            Neq,    // !=
            Leq,    // <=
            Geq,    // >=
            Ge,     // >
            Le,     // <
            Mod,    // %
            If,     // ?
            Shl,    // <<
            Shr,    // >>
            And,    // &&
            Or,     // ||
            BitAnd, // &
            BitXor, // ^
            BitOr,  // |
            //now follows special tokens that are not in the final AST
            Colon,  // :
            LPar,   // (
            RPar    // )
        }


        
        

        class ExpressionToken
        {
            public TokenKind Kind;
            public ExpressionToken Tok1, Tok2, Tok3;
            public string Ident; //used if kind is Identifier
            public double Num;  //used if kind is Number

            public ExpressionToken(TokenKind kind)
            {
                this.Kind = kind;
            }


            public double Eval(Expression es)
            {

                switch (Kind)
                {
                    case TokenKind.Number: return Num;
                    case TokenKind.Identifier: return es.GetVar(Ident);
                    case TokenKind.Add: return Tok1.Eval(es) + Tok2.Eval(es);
                    case TokenKind.Mul: return Tok1.Eval(es) * Tok2.Eval(es);
                    case TokenKind.Sub: return (Tok2==null) ? -Tok1.Eval(es) : (Tok1.Eval(es) - Tok2.Eval(es));
                    case TokenKind.Div: return Tok1.Eval(es) / Tok2.Eval(es);
                    case TokenKind.Eq: return (Math.Abs(Tok1.Eval(es)-Tok2.Eval(es)) < 1e-6) ? 1:0;
                    case TokenKind.Neq: return (Math.Abs(Tok1.Eval(es)-Tok2.Eval(es)) < 1e-6) ? 0:1;
                    case TokenKind.Leq: return (Tok1.Eval(es) <= Tok2.Eval(es)) ? 1:0;
                    case TokenKind.Geq:  return(Tok1.Eval(es) >= Tok2.Eval(es)) ? 1:0;
                    case TokenKind.Le: return (Tok1.Eval(es) < Tok2.Eval(es)) ? 1:0;
                    case TokenKind.Ge:  return(Tok1.Eval(es) > Tok2.Eval(es)) ? 1:0;
                    case TokenKind.Mod:  return(Tok1.Eval(es) % Tok2.Eval(es));
                    case TokenKind.If: return (Math.Abs(Tok1.Eval(es))>1e-6) ? Tok2.Eval(es):Tok3.Eval(es);
                    case TokenKind.Shl: return (int)Tok1.Eval(es)<<(int)Tok2.Eval(es);
                    case TokenKind.Shr: return (int)Tok1.Eval(es)>>(int)Tok2.Eval(es);
                    case TokenKind.And: return (Math.Abs(Tok1.Eval(es))>1e-6 && Math.Abs(Tok2.Eval(es))>1e-6) ?1:0;
                    case TokenKind.Or: return (Math.Abs(Tok1.Eval(es))>1e-6 || Math.Abs(Tok2.Eval(es))>1e-6) ?1:0;
                    case TokenKind.BitAnd: return (int)Tok1.Eval(es)&(int)Tok2.Eval(es);
                    case TokenKind.BitOr: return (int)Tok1.Eval(es)|(int)Tok2.Eval(es);
                    case TokenKind.BitXor: return (int)Tok1.Eval(es)^(int)Tok2.Eval(es);
                    default: throw new ExpressionException("Unknown operator: " + Kind.ToString());
                }
            }

            int RealToInt(double r)
            {
                if (r < 0.0)
                    return (int)(r - 0.5);
                else
                    return (int)(r + 0.5);
            }

            
            public int EvalInt(Expression es)
            {
                switch (Kind)
                {
                    case TokenKind.Number: return RealToInt(Num); //round to integer
                    case TokenKind.Identifier: return RealToInt(es.GetVar(Ident));
                    case TokenKind.Add: return Tok1.EvalInt(es) + Tok2.EvalInt(es);
                    case TokenKind.Mul: return Tok1.EvalInt(es) * Tok2.EvalInt(es);
                    case TokenKind.Sub: return (Tok2 == null) ? -Tok1.EvalInt(es) : (Tok1.EvalInt(es) - Tok2.EvalInt(es));
                    case TokenKind.Div: return Tok1.EvalInt(es) / Tok2.EvalInt(es);
                    case TokenKind.Eq: return (Tok1.EvalInt(es) == Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Neq: return (Tok1.EvalInt(es) != Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Leq: return (Tok1.EvalInt(es) <= Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Geq: return (Tok1.EvalInt(es) >= Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Le: return (Tok1.EvalInt(es) < Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Ge: return (Tok1.EvalInt(es) > Tok2.EvalInt(es)) ? 1 : 0;
                    case TokenKind.Mod: return (Tok1.EvalInt(es) % Tok2.EvalInt(es));
                    case TokenKind.If: return (Tok1.EvalInt(es) != 0) ? Tok2.EvalInt(es) : Tok3.EvalInt(es);
                    case TokenKind.Shl: return Tok1.EvalInt(es) << Tok2.EvalInt(es);
                    case TokenKind.Shr: return Tok1.EvalInt(es) >> Tok2.EvalInt(es);
                    case TokenKind.And: return ( (Tok1.EvalInt(es)!=0) && (Tok2.EvalInt(es)!=0)) ? 1 : 0;
                    case TokenKind.Or: return ((Tok1.EvalInt(es) != 0) || (Tok2.EvalInt(es) != 0)) ? 1 : 0;
                    case TokenKind.BitAnd: return Tok1.EvalInt(es) & Tok2.EvalInt(es);
                    case TokenKind.BitOr: return Tok1.EvalInt(es) | Tok2.EvalInt(es);
                    case TokenKind.BitXor: return Tok1.EvalInt(es) ^ Tok2.EvalInt(es);
                    default: throw new ExpressionException("Unknown operator: " + Kind.ToString());
                }
            }

            public override string ToString()
            {
                if (Kind == TokenKind.Number)
                    return Num.ToString();
                else if (Kind == TokenKind.Identifier)
                    return Ident;

                List<string> strs = new List<string>();
                if(Tok1!=null) strs.Add(Tok1.ToString());
                if(Tok2!=null) strs.Add(Tok2.ToString());
                if(Tok3!=null) strs.Add(Tok3.ToString());
                string r="("+Kind.ToString()+" "+string.Join(" ", strs)+")";
                return r;
            }
        }

    }

    public class ExpressionException : Exception
    {
        public ExpressionException(string msg)
            : base(msg)
        {

        }
    }
}
