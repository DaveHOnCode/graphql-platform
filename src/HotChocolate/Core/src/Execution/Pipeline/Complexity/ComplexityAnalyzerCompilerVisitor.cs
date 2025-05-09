using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Execution.Options;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Types;
using HotChocolate.Validation;
using static System.Linq.Expressions.Expression;

namespace HotChocolate.Execution.Pipeline.Complexity;

internal sealed class ComplexityAnalyzerCompilerVisitor : TypeDocumentValidatorVisitor
{
    private static readonly MethodInfo _getService =
        typeof(IServiceProvider).GetMethod("GetService")!;
    private readonly Expression _settings;
    private readonly Expression _zero = Constant(0, typeof(int));
    private readonly ParameterExpression _variables =
        Parameter(typeof(IVariableValueCollection), "variables");
    private readonly ParameterExpression _services =
        Parameter(typeof(IServiceProvider), "services");

    public ComplexityAnalyzerCompilerVisitor(ComplexityAnalyzerSettings settings)
    {
        _settings = Constant(settings, typeof(ComplexityAnalyzerSettings));
    }

    protected override ISyntaxVisitorAction Enter(
        DocumentNode node,
        IDocumentValidatorContext context)
    {
        context.List.Push(new List<OperationComplexityAnalyzer>());
        return base.Enter(node, context);
    }

    protected override ISyntaxVisitorAction Enter(
        OperationDefinitionNode node,
        IDocumentValidatorContext context)
    {
        context.List.Push(new List<Expression>());
        return base.Enter(node, context);
    }

    protected override ISyntaxVisitorAction Leave(
        OperationDefinitionNode node,
        IDocumentValidatorContext context)
    {
        var expressions = (List<Expression>)context.List.Pop()!;
        var analyzers = (List<OperationComplexityAnalyzer>)context.List.Peek()!;

        analyzers.Add(new OperationComplexityAnalyzer(
            node,
            Lambda<ComplexityAnalyzerDelegate>(
                Combine(expressions),
                _services,
                _variables).Compile()));

        return base.Leave(node, context);
    }

    protected override ISyntaxVisitorAction Enter(
        FieldNode node,
        IDocumentValidatorContext context)
    {
        if (context.Types.TryPeek(out var type) &&
            type.NamedType() is IComplexOutputType ot &&
            ot.Fields.TryGetField(node.Name.Value, out var of))
        {
            context.List.Push(new List<Expression>());
            context.OutputFields.Push(of);
            context.Types.Push(of.Type);
            return Continue;
        }

        context.UnexpectedErrorsDetected = true;
        return Skip;
    }

    protected override ISyntaxVisitorAction Leave(
        FieldNode node,
        IDocumentValidatorContext context)
    {
        var field = context.OutputFields.Pop();
        var children = (List<Expression>)context.List.Pop()!;
        context.Types.Pop();

        var parent = (List<Expression>)context.List.Peek()!;
        parent.Add(CreateFieldComplexityExpression(context, field, node, children));

        return Continue;
    }

    private Expression CreateFieldComplexityExpression(
        IDocumentValidatorContext context,
        IOutputField field,
        FieldNode selection,
        List<Expression> children)
    {
        return children.Count switch
        {
            0 => CreateCalculateExpression(context, field, selection, _zero),
            1 => CreateCalculateExpression(context, field, selection, children[0]),
            _ => CreateCalculateExpression(context, field, selection, Combine(children))
        };
    }

    private Expression CreateCalculateExpression(
        IDocumentValidatorContext context,
        IOutputField field,
        FieldNode selection,
        Expression childComplexity)
    {
        var costDirective =
            field.Directives["cost"]
                .FirstOrDefault()?
                .AsValue<CostDirective>();

        return Call(
            Helper.CalculateMethod,
            Constant(field, typeof(IOutputField)),
            Constant(selection, typeof(FieldNode)),
            Constant(costDirective, typeof(CostDirective)),
            Constant(context.Fields.Count + 1, typeof(int)),
            Constant(context.Path.Count + 1, typeof(int)),
            childComplexity,
            _variables,
            GetInputParser(),
            _settings);
    }

    private Expression GetInputParser()
        => Convert(
            Call(_services, _getService, Constant(typeof(InputParser))),
            typeof(InputParser));

    private Expression Combine(IReadOnlyList<Expression> expressions)
    {
        var combinedComplexity = expressions[0];

        for (var i = 1; i < expressions.Count; i++)
        {
            combinedComplexity = Add(combinedComplexity, expressions[i]);
        }

        return combinedComplexity;
    }

    private static class Helper
    {
        public static readonly MethodInfo CalculateMethod =
            typeof(Helper).GetMethod(nameof(Calculate))!;

        public static int Calculate(
            IOutputField field,
            FieldNode selection,
            CostDirective costDirective,
            int fieldDepth,
            int nodeDepth,
            int childComplexity,
            IVariableValueCollection variables,
            InputParser inputParser,
            ComplexityAnalyzerSettings analyzerSettings)
        {
            return analyzerSettings.Calculation.Invoke(
                new ComplexityContext(
                    field,
                    selection,
                    costDirective,
                    fieldDepth,
                    nodeDepth,
                    childComplexity,
                    analyzerSettings.DefaultComplexity,
                    variables,
                    inputParser));
        }
    }
}
