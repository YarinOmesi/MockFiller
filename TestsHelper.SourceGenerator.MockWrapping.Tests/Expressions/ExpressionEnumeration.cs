/* Copyright (C) 2007 - 2008  Versant Inc.  http://www.db4o.com */
/* https://github.com/lytico/db4o/blob/master/db4o.net/Db4objects.Db4o.Linq/Db4objects.Db4o.Linq/Expressions */

#nullable disable

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Tests.Expressions
{
    internal class ExpressionEnumeration : ExpressionVisitor, IEnumerable<Expression>
    {
        private List<Expression> _expressions = new List<Expression>();

        public ExpressionEnumeration(Expression expression)
        {
            Visit(expression);
        }

        protected override void Visit(Expression expression)
        {
            if (expression == null) return;

            _expressions.Add(expression);
            base.Visit(expression);
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return _expressions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}