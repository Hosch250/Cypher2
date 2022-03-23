namespace Cypher2
{
    using Neo4j.Driver;
    using CypherGenerator;

    public partial class Program
    {
        static async Task Main()
        {
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

            var nodeRecords = nodes.Select(s => (s.nodeType, arg: $"{s.propertyTypes[0]}{(s.mandatory ? "" : "?")} {s.propertyName}"))
                .GroupBy(g => g.nodeType)
                .Select(s => (label: s.Key[2..^1], record: $"public record {s.Key[2..^1]}({string.Join(", ", s.Select(x => x.arg))});"))
                .ToList();

            var relResults = await session.RunAsync("CALL db.schema.relTypeProperties()");
            var relationships = await relResults.ToListAsync(f => new {
                nodeType = f.Values["relType"].As<string>(),
                propertyName = f.Values["propertyName"].As<string>(),
                propertyTypes = f.Values["propertyTypes"].As<List<string>>(),
                mandatory = f.Values["mandatory"].As<bool>(),
            });

            var relRecords = relationships.Select(s => (s.nodeType, arg: s.propertyTypes is null ? "" : $"{s.propertyTypes[0]}{(s.mandatory ? "" : "?")} {s.propertyName}"))
                .GroupBy(g => g.nodeType)
                .Select(s => (label: s.Key[2..^1], record: $"public record {s.Key[2..^1]}({string.Join(", ", s.Select(x => x.arg))});"))
                .ToList();

            var visResults = await session.RunAsync("CALL db.schema.visualization()");
            var vis = await visResults.SingleAsync(f => new {
                nodes = f.Values["nodes"].As<List<INode>>(),
                relations = f.Values["relationships"].As<List<IRelationship>>()
            });

            var compiled = new
            {
                nodeRecords = nodeRecords.Select(s => {
                    var node = vis.nodes.Single(n => n.Labels.Contains(s.label));

                    return new
                    {
                        s.label,
                        s.record,
                        fromRels = vis.relations.Where(w => w.StartNodeId == node.Id).Select(s => relRecords.Single(r => r.label == s.Type)).ToList(),
                        toRels = vis.relations.Where(w => w.EndNodeId == node.Id).Select(s => relRecords.Single(r => r.label == s.Type)).ToList()
                    };
                }).ToList(),
                relRecords
            };
        }
        
        [Cypher("match (n:Person) return n.name as name, n.born as born")]
        public partial Task<IEnumerable<Person>> GetPersons();

        [Cypher("match (n:Person { name: \"Tom Hanks\"}) return n.name as name, n.born as born")]
        public partial Task<Person> GetPerson();

        [Cypher("match (n:Movie) return n.title as title, n.tagline as tagline, n.released as released")]
        public partial Task<IEnumerable<Movie>> GetMovies();
    }

    public record Person(string Name, long? Born);
    public record Movie(string Title, string Tagline, int Released);
}