using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expr = MathNet.Symbolics.SymbolicExpression;
using MathNet.Symbolics;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace Regression.Two_factor_regression
{
    public static class ExpressionCreator
    {
        public static IPolynomialExpression CreateThirdOrderPolynomialExpression(this IEnumerable<DataTwoFact> dataTwoFacts)
        {
            if (dataTwoFacts == null) throw new ArgumentNullException(nameof(dataTwoFacts));
            if (dataTwoFacts.Count() < 16)
                throw new ArgumentOutOfRangeException(nameof(dataTwoFacts));
            var variables = new List<string>()
            { "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10", "a11", "a12", "a13", "a14", "a15" };
            var varExpr = variables!.Select(Expr.Variable).ToArray();
            var poly = dataTwoFacts.Select(item => item.Y + (varExpr[0] + varExpr[1] * item.X2 + varExpr[2] * item.X2 * item.X2 + varExpr[3] * item.X1 + varExpr[4] * item.X1 * item.X1 + varExpr[5] * item.X1 * item.X2 + varExpr[6] * item.X2 * item.X1 * item.X1 + varExpr[7] * item.X2 * item.X2 * item.X1 + varExpr[8] * item.X1 * item.X1 * item.X2 * item.X2 + varExpr[9] * item.X1 * item.X1 * item.X1 + varExpr[10] * item.X2 * item.X1 * item.X1 * item.X1 + varExpr[11] * item.X2 * item.X2 * item.X1 * item.X1 * item.X1 + varExpr[12] * item.X2 * item.X2 * item.X2 + varExpr[13] * item.X2 * item.X2 * item.X2 * item.X1 + varExpr[14] * item.X2 * item.X2 * item.X2 * item.X1 * item.X1 + varExpr[15] * item.X2 * item.X2 * item.X2 * item.X1 * item.X1 * item.X1)).Aggregate<SymbolicExpression?, SymbolicExpression>(0, (current, expr) => current + expr * expr);
            return new VariableExpression(poly, varExpr);
        }
        /// <summary>
        /// Fourth-order (25-coefficient) form. Term order matches FourthOrderBasisExponents:
        /// a0..a15 are the same terms as CreateThirdOrderPolynomialExpression, a16..a24 are
        /// X1^4*X2^0..3, X1^0..3*X2^4, X1^4*X2^4.
        /// </summary>
        public static IPolynomialExpression CreateFourthOrderPolynomialExpression(this IEnumerable<DataTwoFact> dataTwoFacts)
        {
            if (dataTwoFacts == null) throw new ArgumentNullException(nameof(dataTwoFacts));
            if (dataTwoFacts.Count() < 25)
                throw new ArgumentOutOfRangeException(nameof(dataTwoFacts));
            var variables = new List<string>()
            {
                "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10", "a11", "a12", "a13", "a14", "a15",
                "a16", "a17", "a18", "a19", "a20", "a21", "a22", "a23", "a24"
            };
            var varExpr = variables.Select(Expr.Variable).ToArray();
            var poly = dataTwoFacts.Select(item => item.Y + (
                varExpr[0]
                + varExpr[1] * item.X2
                + varExpr[2] * item.X2 * item.X2
                + varExpr[3] * item.X1
                + varExpr[4] * item.X1 * item.X1
                + varExpr[5] * item.X1 * item.X2
                + varExpr[6] * item.X2 * item.X1 * item.X1
                + varExpr[7] * item.X2 * item.X2 * item.X1
                + varExpr[8] * item.X1 * item.X1 * item.X2 * item.X2
                + varExpr[9] * item.X1 * item.X1 * item.X1
                + varExpr[10] * item.X2 * item.X1 * item.X1 * item.X1
                + varExpr[11] * item.X2 * item.X2 * item.X1 * item.X1 * item.X1
                + varExpr[12] * item.X2 * item.X2 * item.X2
                + varExpr[13] * item.X2 * item.X2 * item.X2 * item.X1
                + varExpr[14] * item.X2 * item.X2 * item.X2 * item.X1 * item.X1
                + varExpr[15] * item.X2 * item.X2 * item.X2 * item.X1 * item.X1 * item.X1
                + varExpr[16] * item.X1 * item.X1 * item.X1 * item.X1
                + varExpr[17] * item.X1 * item.X1 * item.X1 * item.X1 * item.X2
                + varExpr[18] * item.X1 * item.X1 * item.X1 * item.X1 * item.X2 * item.X2
                + varExpr[19] * item.X1 * item.X1 * item.X1 * item.X1 * item.X2 * item.X2 * item.X2
                + varExpr[20] * item.X2 * item.X2 * item.X2 * item.X2
                + varExpr[21] * item.X2 * item.X2 * item.X2 * item.X2 * item.X1
                + varExpr[22] * item.X2 * item.X2 * item.X2 * item.X2 * item.X1 * item.X1
                + varExpr[23] * item.X2 * item.X2 * item.X2 * item.X2 * item.X1 * item.X1 * item.X1
                + varExpr[24] * item.X1 * item.X1 * item.X1 * item.X1 * item.X2 * item.X2 * item.X2 * item.X2
            )).Aggregate<SymbolicExpression?, SymbolicExpression>(0, (current, expr) => current + expr * expr);
            return new VariableExpression(poly, varExpr);
        }
        public static IPolynomialExpression CreateTwoOrderPolynomialExpression(
            this IEnumerable<DataTwoFact> dataTwoFacts)
        {
            if (dataTwoFacts == null)
                throw new ArgumentNullException(nameof(dataTwoFacts));
            if (dataTwoFacts.Count<DataTwoFact>() < 9)
                throw new ArgumentOutOfRangeException(nameof(dataTwoFacts));
            SymbolicExpression[] varExpr = new List<string>()
            { "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"
            }.Select<string, SymbolicExpression>(new Func<string, SymbolicExpression>(SymbolicExpression.Variable)).ToArray<SymbolicExpression>();
            return (IPolynomialExpression)new VariableExpression(dataTwoFacts.Select<DataTwoFact, SymbolicExpression>((Func<DataTwoFact, SymbolicExpression>)(item => (SymbolicExpression)item.Y +
                    (varExpr[0] +
                     varExpr[1] * (SymbolicExpression)item.X2 +
                     varExpr[2] * (SymbolicExpression)item.X2 * (SymbolicExpression)item.X2 +
                     varExpr[3] * (SymbolicExpression)item.X1 +
                     varExpr[4] * (SymbolicExpression)item.X1 * (SymbolicExpression)item.X1 +
                     varExpr[5] * (SymbolicExpression)item.X1 * (SymbolicExpression)item.X2 +
                     varExpr[6] * (SymbolicExpression)item.X2 * (SymbolicExpression)item.X1 * (SymbolicExpression)item.X1 +
                     varExpr[7] * (SymbolicExpression)item.X2 * (SymbolicExpression)item.X2 * (SymbolicExpression)item.X1 +
                     varExpr[8] * (SymbolicExpression)item.X1 * (SymbolicExpression)item.X1 * (SymbolicExpression)item.X2 * (SymbolicExpression)item.X2)
                )).Aggregate<SymbolicExpression, SymbolicExpression>((SymbolicExpression)0, (Func<SymbolicExpression, SymbolicExpression, SymbolicExpression>)((current, expr) => current + expr * expr)), (IEnumerable<SymbolicExpression>)varExpr);
        }
    }
}
