using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nexus.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmanagedComponentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NX0001";
    private static readonly LocalizableString Title = "Type must be unmanaged";
    private static readonly LocalizableString MessageFormat = "Type '{0}' is marked with [MustBeUnmanaged] or used as a component but contains managed references";
    private static readonly LocalizableString Description = "Nexus components and types marked with [MustBeUnmanaged] must be unmanaged structs to avoid GC overhead.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeGenericRegistryCall, SyntaxKind.InvocationExpression);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        var attributes = namedType.GetAttributes();
        
        bool hasMustBeUnmanaged = false;
        foreach (var attr in attributes)
        {
            if (attr.AttributeClass?.Name == "MustBeUnmanagedAttribute" || attr.AttributeClass?.Name == "MustBeUnmanaged")
            {
                hasMustBeUnmanaged = true;
                break;
            }
        }

        if (hasMustBeUnmanaged)
        {
            if (!namedType.IsUnmanagedType)
            {
                var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private void AnalyzeGenericRegistryCall(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            (memberAccess.Name.Identifier.Text == "Add" || memberAccess.Name.Identifier.Text == "Get" || memberAccess.Name.Identifier.Text == "Has"))
        {
            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol != null && symbol.ContainingType.Name == "Registry" && symbol.IsGenericMethod)
            {
                var typeArg = symbol.TypeArguments[0];
                if (!typeArg.IsUnmanagedType)
                {
                    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), typeArg.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
