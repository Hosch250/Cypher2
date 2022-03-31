namespace Cypher2
{
    using Neo4j.Driver;
    using CypherGenerator;
    using System.Linq.Expressions;
    using static Cypher2.PersonToMovieRelationships;
    using Newtonsoft.Json;

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
            var driver = new Neo4jClient.GraphClient(new Uri("http://localhost:7474"), "neo4j", "test");
            await driver.ConnectAsync();
            var repo = new PersonRepository(driver);

            //var person = await repo.Get(288);
            //var persons = await repo.GetWhere(n => n.born == 1995);  // todo: requires parameter name to match `match` parameter name
            //var person = await repo.Create(new Person(1995, "Abraham Hosch"));
            //var person = await repo.Merge(new Person(1995, "Abraham Hosch"));
            //var person = await repo.Update(new Person(1995, "Abraham Hosch") { Id = 289 });
            //await repo.Delete(289);
            //await repo.DeleteWhere(n => n.born == 1995);  // todo: requires parameter name to match `match` parameter name
            //var count = await repo.Count(n => n.born == 1995);  // todo: requires parameter name to match `match` parameter name
            //var rel = await repo.Connect(289, 1, new PersonToPersonRelationships.FOLLOWS());
            //var rel = await repo.Merge(289, 1, new PersonToPersonRelationships.FOLLOWS());


            var x = 0;

            //var typeMap = new Dictionary<string, string>()
            //{
            //    ["Boolean"] = "bool",
            //    ["Long"] = "long",
            //    ["Double"] = "double",
            //    ["String"] = "string",
            //    ["DateTime"] = "DateTime",
            //    ["LocalDateTime"] = "DateTime",
            //    ["Date"] = "DateOnly",
            //    ["LocalDate"] = "DateOnly",
            //    ["Time"] = "TimeOnly",
            //    ["LocalTime"] = "TimeOnly",
            //    ["Duration"] = "TimeSpan",
            //    ["BooleanArray"] = "List<bool>",
            //    ["LongArray"] = "List<long>",
            //    ["DoubleArray"] = "List<double>",
            //    ["StringArray"] = "List<string>",
            //    ["DateTimeArray"] = "List<DateTime>",
            //    ["DateArray"] = "List<DateOnly>",
            //    ["LocalDateArray"] = "List<DateOnly>",
            //    ["TimeArray"] = "List<TimeOnly>",
            //    ["LocalTimeArray"] = "List<TimeOnly>",
            //    ["DurationArray"] = "List<TimeSpan>",
            //};

            //var driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "test"));
            //var session = driver.AsyncSession();

            //var nodeResults = await session.RunAsync("CALL db.schema.nodeTypeProperties()");
            //var nodes = await nodeResults.ToListAsync(f => new
            //{
            //    nodeType = f.Values["nodeType"].As<string>(),
            //    propertyName = f.Values["propertyName"].As<string>(),
            //    propertyTypes = f.Values["propertyTypes"].As<List<string>>(),
            //    mandatory = f.Values["mandatory"].As<bool>(),
            //});

            //var relResults = await session.RunAsync("CALL db.schema.relTypeProperties()");
            //var relationships = await relResults.ToListAsync(f => new {
            //    nodeType = f.Values["relType"].As<string>(),
            //    propertyName = f.Values["propertyName"].As<string>(),
            //    propertyTypes = f.Values["propertyTypes"].As<List<string>>(),
            //    mandatory = f.Values["mandatory"].As<bool>(),
            //});

            //var relRecords = relationships.Select(s => (s.nodeType, arg: s.propertyTypes is null ? "" : $"{typeMap[s.propertyTypes[0]]}{(s.mandatory ? "" : "?")} {s.propertyName}"))
            //    .GroupBy(g => g.nodeType)
            //    .Select(s => $"public record {s.Key[2..^1]}({string.Join(", ", s.Select(x => x.arg))});")
            //    .ToList();

            //var visResults = await session.RunAsync("CALL db.schema.visualization()");
            //var vis = await visResults.SingleAsync(f => new {
            //    nodes = f.Values["nodes"].As<List<INode>>(),
            //    relations = f.Values["relationships"].As<List<IRelationship>>()
            //});

            //var nodeRecords = nodes
            //.GroupBy(g => g.nodeType)
            //.Select(s => {
            //    var node = vis.nodes.Single(n => n.Labels.Contains(s.Key[2..^1]));
            //    var connections = vis.relations.Where(w => w.StartNodeId == node.Id || w.EndNodeId == node.Id).ToList();

            //    var relProperties = connections.Select(c => {
            //        var direction = c.StartNodeId == node.Id ? "FROM" : "TO";
            //        var connectedTo = c.StartNodeId == node.Id ? c.EndNodeId : c.StartNodeId;

            //        var connectedToNode = vis.nodes.First(f => f.Id == connectedTo).Labels[0];
            //        var relationship = relationships.First(f => f.nodeType[2..^1] == c.Type);

            //        var type = relationship.propertyTypes is null ? $"List<{connectedToNode}>" : $"List<({relationship.nodeType[2..^1]}, {connectedToNode})>";

            //        return string.Format(RelationshipPropertyTemplate, c.Type, "Direction." + direction, type, c.Type);
            //    });

            //    var relProps = string.Join(Environment.NewLine + Environment.NewLine, relProperties);

            //    var recordArgs = s.Select(n => $"{typeMap[n.propertyTypes[0]]}{(n.mandatory ? "" : "?")} {n.propertyName}");

            //    return string.Format(RecordTemplate, node.Labels[0], string.Join(", ", recordArgs), relProps);
            //}).ToList();
        }
        
    }
    public interface IRepository<T>
    {
        ValueTask<T?> Get(long id);
        ValueTask<List<T>> GetWhere(Expression<Func<T, bool>> whereFilter, int? skip = null, int? limit = null);
        ValueTask<T> Create(T record);
        ValueTask<T> Merge(T record);
        ValueTask<T?> Update(T record);
        ValueTask Delete(long id);
        ValueTask DeleteWhere(Expression<Func<T, bool>> whereFilter);
        ValueTask<long> Count(Expression<Func<T, bool>> filter);
        ValueTask<TRelationship> Connect<TRelationship>(long fromId, long toId, TRelationship relationshipRecord)
            where TRelationship : struct, ICypherNode;
        ValueTask<TRelationship> Merge<TRelationship>(long fromId, long toId, TRelationship relationshipRecord)
            where TRelationship : struct, ICypherNode;
    }

    // todo: support user-provided id instead of using Neo4j id
    public class PersonRepository : IRepository<Person>
    {
        private readonly Neo4jClient.GraphClient driver;

        public PersonRepository(Neo4jClient.GraphClient driver)
        {
            this.driver = driver;
        }

        public async ValueTask<Person?> Get(long id)
        {
            var query = driver.Cypher
                .WithParams(new Dictionary<string, object>() { ["nodeId"] = id })
                .Match("(n:Person)")
                .Where("ID(n) = $nodeId")
                .Return(n => new
                {
                    id = n.Id(),
                    person = n.As<Person>()
                });

            var response = (await query.ResultsAsync).First();
            return response.person with { Id = response.id };
        }

        public async ValueTask<List<Person>> GetWhere(Expression<Func<Person, bool>> whereFilter, int? skip = null, int? limit = null)
        {
            var query = driver.Cypher
                .Match("(n:Person)")
                .Where(whereFilter)
                .Skip(skip)
                .Limit(limit)
                .Return(n => new
                {
                    id = n.Id(),
                    person = n.As<Person>()
                });

            return (await query.ResultsAsync).Select(response => response.person with { Id = response.id }).ToList();
        }

        public async ValueTask<Person> Create(Person record)
        {
            var query = driver.Cypher
                .Create("(n:Person $node)")
                .WithParam("node", record)
                .Return(n => new
                {
                    id = n.Id(),
                    person = n.As<Person>()
                });

            var response = (await query.ResultsAsync).First();
            return response.person with { Id = response.id };
        }

        public async ValueTask<Person> Merge(Person record)
        {
            var query = driver.Cypher
                .Merge("(n:Person $node)")
                .WithParam("node", record)
                .Return(n => new
                {
                    id = n.Id(),
                    person = n.As<Person>()
                });

            var response = (await query.ResultsAsync).First();
            return response.person with { Id = response.id };
        }

        public async ValueTask<Person?> Update(Person record)
        {
            var query = driver.Cypher
                .Match("(n:Person)")
                .WithParam("node", record)
                .WithParam("nodeId", record.Id)
                .Where("ID(n) = $nodeId")
                .Set("n = $node")
                .Return(n => new
                {
                    id = n.Id(),
                    person = n.As<Person>()
                });

            var response = (await query.ResultsAsync).FirstOrDefault();
            return response is null ? null : response.person with { Id = response.id };
        }

        public async ValueTask Delete(long id)
        {
            var query = driver.Cypher
                .Match("(n:Person)")
                .WithParam("nodeId", id)
                .Where("ID(n) = $nodeId")
                .DetachDelete("n");

            await query.ExecuteWithoutResultsAsync();
        }

        public async ValueTask DeleteWhere(Expression<Func<Person, bool>> whereFilter)
        {
            var query = driver.Cypher
                .Match("(n:Person)")
                .Where(whereFilter)
                .DetachDelete("n");

            await query.ExecuteWithoutResultsAsync();
        }

        public async ValueTask<long> Count(Expression<Func<Person, bool>> filter)
        {
            var query = driver.Cypher
                .Match("(n:Person)")
                .Where(filter)
                .Return(n => n.Count());

            return (await query.ResultsAsync).First();
        }

        public async ValueTask<TRelationship> Connect<TRelationship>(long fromId, long toId, TRelationship relationshipRecord)
            where TRelationship : struct, ICypherNode
        {
            var query = driver.Cypher
                .Match("(from)")
                .Match("(to)")
                .Where("ID(from) = $fromId AND ID(to) = $toId")
                .Create($"(from)-[rel:{typeof(TRelationship).Name} $relationship]->(to)")
                .WithParams(new Dictionary<string, object>()
                {
                    ["fromId"] = fromId,
                    ["toId"] = toId,
                    ["relationship"] = relationshipRecord,
                }).Return(rel => rel.Id());

            var id = (await query.ResultsAsync).First();
            return relationshipRecord with { Id = id };
        }

        // todo: merge with props as well (empty obj can't be used in merge)
        public async ValueTask<TRelationship> Merge<TRelationship>(long fromId, long toId, TRelationship relationshipRecord)
            where TRelationship : struct, ICypherNode
        {
            var query = driver.Cypher
                .Match("(from)")
                .Match("(to)")
                .Where("ID(from) = $fromId AND ID(to) = $toId")
                .Merge($"(from)-[rel:{typeof(TRelationship).Name}]->(to)")
                .WithParams(new Dictionary<string, object>()
                {
                    ["fromId"] = fromId,
                    ["toId"] = toId,
                    ["relationship"] = relationshipRecord,
                }).Return(rel => rel.Id());

            var id = (await query.ResultsAsync).First();
            return relationshipRecord with { Id = id };
        }
    }

    public interface ICypherNode
    {
        [JsonIgnore]
        public long Id { get; set; }
    }

    [CypherLabel("Person")]
    public record Person(long? born, string name) : ICypherNode
    {
        public Person() : this(null, "") { }

        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [CypherRelationship("ACTED_IN", Direction.FROM)]
        public List<(ACTED_IN, Movie)> ACTED_IN { get; } = new();

        [JsonIgnore]
        [CypherRelationship("REVIEWED", Direction.FROM)]
        public List<(REVIEWED, Movie)> REVIEWED { get; } = new();

        [JsonIgnore]
        [CypherRelationship("PRODUCED", Direction.FROM)]
        public List<Movie> PRODUCED { get; } = new();

        [JsonIgnore]
        [CypherRelationship("WROTE", Direction.FROM)]
        public List<Movie> WROTE { get; } = new();

        [JsonIgnore]
        [CypherRelationship("FOLLOWS", Direction.FROM)]
        public List<Person> FOLLOWS { get; } = new();

        [JsonIgnore]
        [CypherRelationship("DIRECTED", Direction.FROM)]
        public List<Movie> DIRECTED { get; } = new();
    }

    [CypherLabel("Movie")]
    public record Movie(string? tagline, string title, long released) : ICypherNode
    {
        public Movie() : this(null, "", 0) { }

        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [CypherRelationship("ACTED_IN", Direction.TO)]
        public List<(ACTED_IN, Person)> ACTED_IN { get; } = new();

        [JsonIgnore]
        [CypherRelationship("REVIEWED", Direction.TO)]
        public List<(REVIEWED, Person)> REVIEWED { get; } = new();

        [JsonIgnore]
        [CypherRelationship("PRODUCED", Direction.TO)]
        public List<Person> PRODUCED { get; } = new();

        [JsonIgnore]
        [CypherRelationship("WROTE", Direction.TO)]
        public List<Person> WROTE { get; } = new();

        [JsonIgnore]
        [CypherRelationship("DIRECTED", Direction.TO)]
        public List<Person> DIRECTED { get; } = new();
    }

    public record PersonToMovieRelationships
    {
        public record struct ACTED_IN(List<string> roles) : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
        }

        public record struct REVIEWED() : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
        }

        public record struct PRODUCED() : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
        }

        public record struct WROTE() : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
        }

        public record struct DIRECTED() : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
        }
    }

    public record PersonToPersonRelationships
    {
        public record struct FOLLOWS() : ICypherNode
        {
            [JsonIgnore]
            public long Id { get; set; } = -1;
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