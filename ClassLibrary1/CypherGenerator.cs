using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        private const string CypherAttribute = @"// Auto-generated code
namespace CypherGenerator;

//public class CypherAttribute : System.Attribute
//{
//    public string Statement { get; }
//    public CypherAttribute(string statement)
//    {
//        Statement = statement;
//    }
//}

public class CypherLabelAttribute : System.Attribute
{
    public string Label { get; }
    public CypherLabelAttribute(string label)
    {
        Label = label;
    }
}

public enum Direction { FROM, TO }

public class CypherRelationshipAttribute : System.Attribute
{
    public string Type { get; }
    public Direction Direction { get; }
    public CypherRelationshipAttribute(string type, Direction direction)
    {
        Type = type;
        Direction = direction;
    }
}";

        private const string Repository = @"// Auto-generated code
namespace CypherGenerator;

public interface IRepository<T>
{
    T Get<TId>(TId id);
    List<T> GetWhere(Expression<Func<T, bool>> whereFilter, int skip, int limit);
    T Create(T record);
    T Merge(Expression<Func<T, bool>> filter, T onCreate);
    T Update(T record);
    void Delete<TId>(TId id);
    void DeleteWhere(Expression<Func<T, bool>> whereFilter);
    int Count(Expression<Func<T, bool>> filter);
    TRelationship Connect<TId, TRelationship>(TId fromId, TId toId, TRelationship relationshipRecord);
}";

        public void Execute(GeneratorExecutionContext context)
        {
            var types = GetCypherMethods(context.Compilation);
            foreach (var declaration in types.GroupBy(g => g.ContainingType, SymbolEqualityComparer.Default))
            {
                var builder = new StringBuilder();

                var methods = declaration.Select(symbol =>
                {
                    var querySyntaxNode = symbol.GetAttributes().First(f => f.AttributeClass.Name == "Cypher" || f.AttributeClass.Name == "CypherAttribute").ApplicationSyntaxReference.GetSyntax();
                    var query = (querySyntaxNode as AttributeSyntax).ArgumentList.Arguments[0].Expression.ToFullString();

                    var methodReturnType = (symbol.ReturnType as INamedTypeSymbol).TypeArguments[0];

                    var resultMethod = methodReturnType.AllInterfaces.Any(i => i.Name == "IEnumerable") || methodReturnType.Name == "IEnumerable"
                        ? "ToListAsync"
                        : "SingleAsync";

                    var resultType = methodReturnType.AllInterfaces.Any(i => i.Name == "IEnumerable") || methodReturnType.Name == "IEnumerable"
                        ? (methodReturnType as INamedTypeSymbol).TypeArguments[0] as INamedTypeSymbol
                        : methodReturnType as INamedTypeSymbol;

                    var resultCtor = resultType.Constructors.First(f => f.Parameters.Length > 0 && !f.Parameters[0].Type.Equals(resultType, SymbolEqualityComparer.IncludeNullability));
                    var ctorArgs = resultCtor.Parameters.Select(s => $@"f[""{s.Name[0].ToString().ToLower()}{s.Name.Substring(1)}""].As<{s.Type.ToDisplayString()}>()");

                    return
$@"    public partial async {symbol.ReturnType.ToDisplayString()} {symbol.Name}()
    {{
        using var session = driver.AsyncSession();

        var query = new Neo4j.Driver.Query({query});
        var result = await session.RunAsync(query);

        return await result.{resultMethod}(f => new {resultType.ToDisplayString()}({string.Join(", ", ctorArgs)}));
    }}";
        });

                var typeSymbol = declaration.Key;
                builder.Append(
$@"// Auto-generated code
namespace {typeSymbol.ContainingNamespace.ToDisplayString()};
using Neo4j.Driver;

public partial class {typeSymbol.Name}
{{
    private IDriver driver;

    public {typeSymbol.Name}(IDriver driver)
    {{
        this.driver = driver;
    }}

{string.Join($"{Environment.NewLine}{Environment.NewLine}", methods)}
}}");

                context.AddSource($"{typeSymbol.Name}.g.cs", builder.ToString());
            }

            // Add the source code to the compilation
            context.AddSource("CypherAttribute.g.cs", CypherAttribute);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif 
        }

        private static ImmutableArray<IMethodSymbol> GetCypherMethods(Compilation compilation)
        {
            IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<MethodDeclarationSyntax> allMethods = allNodes
                .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>();

            return allMethods
                .Where(component => component.AttributeLists
                    .SelectMany(s => s.Attributes)
                    .Any(w => w.Name.ToString() == "Cypher" || w.Name.ToString() == "CypherAttribute"))
                .Select(s => compilation.GetSemanticModel(s.SyntaxTree).GetDeclaredSymbol(s))
                .ToImmutableArray();
        }
    }
}