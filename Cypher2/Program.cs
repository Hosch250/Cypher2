namespace Cypher2
{
    using Neo4j.Driver;
    using CypherGenerator;

    public partial class Program
    {

        public const string RelationshipPropertyTemplate =
@"    [CypherRelationship(""{0}"", {1})]
    public {2} {3} {{ get; }}";

        public const string RecordTemplate = 
@"[CypherLabel(""{0}"")]
public record {0}({1})
{{
{2}
}}";

        static async Task Main()
        {
            var typeMap = new Dictionary<string, string>()
            {
                ["Boolean"] = "bool",
                ["Long"] = "long",
                ["Double"] = "double",
                ["String"] = "string",
                ["DateTime"] = "DateTime",
                ["LocalDateTime"] = "DateTime",
                ["Date"] = "DateOnly",
                ["LocalDate"] = "DateOnly",
                ["Time"] = "TimeOnly",
                ["LocalTime"] = "TimeOnly",
                ["Duration"] = "TimeSpan",
                ["BooleanArray"] = "List<bool>",
                ["LongArray"] = "List<long>",
                ["DoubleArray"] = "List<double>",
                ["StringArray"] = "List<string>",
                ["DateTimeArray"] = "List<DateTime>",
                ["DateArray"] = "List<DateOnly>",
                ["LocalDateArray"] = "List<DateOnly>",
                ["TimeArray"] = "List<TimeOnly>",
                ["LocalTimeArray"] = "List<TimeOnly>",
                ["DurationArray"] = "List<TimeSpan>",
            };

            var driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "test"));
            var session = driver.AsyncSession();

            var nodeResults = await session.RunAsync("CALL db.schema.nodeTypeProperties()");
            var nodes = await nodeResults.ToListAsync(f => new
            {
                nodeType = f.Values["nodeType"].As<string>(),
                propertyName = f.Values["propertyName"].As<string>(),
                propertyTypes = f.Values["propertyTypes"].As<List<string>>(),
                mandatory = f.Values["mandatory"].As<bool>(),
            });

            var relResults = await session.RunAsync("CALL db.schema.relTypeProperties()");
            var relationships = await relResults.ToListAsync(f => new {
                nodeType = f.Values["relType"].As<string>(),
                propertyName = f.Values["propertyName"].As<string>(),
                propertyTypes = f.Values["propertyTypes"].As<List<string>>(),
                mandatory = f.Values["mandatory"].As<bool>(),
            });

            var relRecords = relationships.Select(s => (s.nodeType, arg: s.propertyTypes is null ? "" : $"{typeMap[s.propertyTypes[0]]}{(s.mandatory ? "" : "?")} {s.propertyName}"))
                .GroupBy(g => g.nodeType)
                .Select(s => $"public record {s.Key[2..^1]}({string.Join(", ", s.Select(x => x.arg))});")
                .ToList();

            var visResults = await session.RunAsync("CALL db.schema.visualization()");
            var vis = await visResults.SingleAsync(f => new {
                nodes = f.Values["nodes"].As<List<INode>>(),
                relations = f.Values["relationships"].As<List<IRelationship>>()
            });

            var nodeRecords = nodes
            .GroupBy(g => g.nodeType)
            .Select(s => {
                var node = vis.nodes.Single(n => n.Labels.Contains(s.Key[2..^1]));
                var connections = vis.relations.Where(w => w.StartNodeId == node.Id || w.EndNodeId == node.Id).ToList();

                var relProperties = connections.Select(c => {
                    var direction = c.StartNodeId == node.Id ? "FROM" : "TO";
                    var connectedTo = c.StartNodeId == node.Id ? c.EndNodeId : c.StartNodeId;

                    var connectedToNode = vis.nodes.First(f => f.Id == connectedTo).Labels[0];
                    var relationship = relationships.First(f => f.nodeType[2..^1] == c.Type);

                    var type = relationship.propertyTypes is null ? $"List<{connectedToNode}>" : $"List<({relationship.nodeType[2..^1]}, {connectedToNode})>";

                    return string.Format(RelationshipPropertyTemplate, c.Type, "Direction." + direction, type, c.Type);
                });

                var relProps = string.Join(Environment.NewLine + Environment.NewLine, relProperties);

                var recordArgs = s.Select(n => $"{typeMap[n.propertyTypes[0]]}{(n.mandatory ? "" : "?")} {n.propertyName}");

                return string.Format(RecordTemplate, node.Labels[0], string.Join(", ", recordArgs), relProps);
            }).ToList();
        }
        
    }
}

// todo: add ids to each node
// todo: parameterize and write records to file
// todo: generate all attributes and supporting enums in source generator
// todo: generate IRepository<T> for each node record in source generator
// todo: generate supporting query methods for reading connected nodes/relationships
//       in source generator and update relationship properties
// question: generate IRepository<T> for relationships as well?