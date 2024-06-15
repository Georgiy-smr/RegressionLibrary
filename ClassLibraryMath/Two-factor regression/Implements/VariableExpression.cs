using Regression.Two_factor_regression.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace Regression.Two_factor_regression.Implements
{
    public class VariableExpression : IPolynomialExpression
    {
        public VariableExpression(SymbolicExpression expression,
            IEnumerable<SymbolicExpression> variables)
        {
            Function = expression;
            Variables = variables;
        }
        public SymbolicExpression Function { get; }
        public IEnumerable<SymbolicExpression> Variables { get; }
    }
}
