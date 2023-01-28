/* Copyright (C) 2007 - 2008  Versant Inc.  http://www.db4o.com */
/* https://github.com/lytico/db4o/blob/master/db4o.net/Db4objects.Db4o.Linq/Db4objects.Db4o.Linq/Expressions */

using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestsHelper.SourceGenerator.MockWrapping.Tests.Expressions
{
    internal class ExpressionComparison : ExpressionVisitor
    {
        private Queue<Expression> _candidates = null!;
        private Expression? _candidate;

        private const string TypeMismatchTemplate = "Expression Mismatch\n  Expected: {0}\n  Actual: {1}";
        private const string TypeParameterTemplate = "\n\tString: {0}\n\tNodeType: {1}\n\tType: {2}";

        public void AssertEquals(Expression a, Expression b)
        {
            var prevFilter = StackFilter.DefaultFilter;
            StackFilter.DefaultFilter = new StackFilter(".", ".");
            Assert.Multiple(() =>
            {
                _candidates = new Queue<Expression>(new ExpressionEnumeration(b));
                Visit(a);
            });
            StackFilter.DefaultFilter = prevFilter;
        }

        protected override void Visit(Expression? expression)
        {
            if (expression == null) return;

            if (!_candidates.TryPeek(out _candidate) || _candidate == null) return;

            if (CheckEqual(expression.NodeType, _candidate.NodeType) && CheckEqual(expression.Type, _candidate.Type))
            {
                _candidates.Dequeue();

                base.Visit(expression);
                return;
            }

            Assert.Fail(
                TypeMismatchTemplate,
                string.Format(TypeParameterTemplate, expression, expression.NodeType, expression.Type),
                string.Format(TypeParameterTemplate, _candidate, _candidate.NodeType, _candidate.Type)
            );
        }

        private bool CheckEqual<T>(T? t, T? candidate) => EqualityComparer<T>.Default.Equals(t, candidate);


        private T CandidateFor<T>(T original) where T : Expression
        {
            return (_candidate as T)!;
        }

        protected override void VisitConstant(ConstantExpression constant)
        {
            var candidate = CandidateFor(constant);
            Assert.AreEqual(constant.Value, candidate.Value, "Constant Values Are Not Equal");
        }

        protected override void VisitMemberAccess(MemberExpression member)
        {
            var candidate = CandidateFor(member);
            Assert.AreEqual(member.Member, candidate.Member, "Members of MemberAccess Are Not Equal");

            base.VisitMemberAccess(member);
        }

        protected override void VisitMethodCall(MethodCallExpression methodCall)
        {
            var candidate = CandidateFor(methodCall);
            Assert.AreEqual(methodCall.Method, candidate.Method, "Method of MethodCall Are Not Equal");

            base.VisitMethodCall(methodCall);
        }

        protected override void VisitParameter(ParameterExpression parameter)
        {
            var candidate = CandidateFor(parameter);
            Assert.AreEqual(parameter.Name, candidate.Name, "Parameter Name Are Not Equal");
        }

        protected override void VisitTypeIs(TypeBinaryExpression type)
        {
            var candidate = CandidateFor(type);
            Assert.AreEqual(type.TypeOperand, candidate.TypeOperand, "TypeOperand Are Not Equal");

            base.VisitTypeIs(type);
        }

        protected override void VisitBinary(BinaryExpression binary)
        {
            var candidate = CandidateFor(binary);
            Assert.AreEqual(binary.Method, candidate.Method, "Method Are Not Equal");
            Assert.AreEqual(binary.IsLifted, candidate.IsLifted, "IsLifted Are Not Equal");
            Assert.AreEqual(binary.IsLiftedToNull, candidate.IsLiftedToNull, "IsLiftedToNull Are Not Equal");

            base.VisitBinary(binary);
        }

        protected override void VisitUnary(UnaryExpression unary)
        {
            var candidate = CandidateFor(unary);
            Assert.AreEqual(unary.Method, candidate.Method, "Method Are Not Equal");
            Assert.AreEqual(unary.IsLifted, candidate.IsLifted, "IsLifted Are Not Equal");
            Assert.AreEqual(unary.IsLiftedToNull, candidate.IsLiftedToNull, "IsLiftedToNull Are Not Equal");

            base.VisitUnary(unary);
        }

        protected override void VisitNew(NewExpression nex)
        {
            var candidate = CandidateFor(nex);
            Assert.AreEqual(nex.Constructor, candidate.Constructor, "Constructor Are Not Equal");
            CollectionAssert.AreEqual(nex.Members, candidate.Members, "Members Are Not Equal");

            base.VisitNew(nex);
        }
    }
}