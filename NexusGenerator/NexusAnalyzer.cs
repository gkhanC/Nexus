using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace NexusGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NexusAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NX001";
        private static readonly LocalizableString Title = "ECS Component must be unmanaged";
        private static readonly LocalizableString MessageFormat = "Type '{0}' is used as an ECS component but contains managed fields. Components must be unmanaged structs for performance.";
        private static readonly LocalizableString Description = "All Nexus ECS components must be unmanaged structs to ensure Burst compatibility and zero-GC impact.";
        
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, "Nexus.Engine", 
            DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            // Logic: Check if the struct is used as an ECS component (e.g., via [Sync] or Registry methods)
            // Simplified check: Any struct in a namespace containing ".Components" or having specific attributes.
            if (typeSymbol.TypeKind == TypeKind.Struct && typeSymbol.IsUnmanagedType == false)
            {
                var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
