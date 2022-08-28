using System.Linq.Expressions;

namespace Cypher2;
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
        //var driver = new Neo4jClient.GraphClient(new Uri("http://localhost:7474"), "neo4j", "test");
        //await driver.ConnectAsync();
        var repo = new PersonRepository(null!);

        //var person = await repo.Get(288);
        //var persons = await repo.GetWhere(n => n.born == 1995);
        // var person = await repo.Create(new Person(Guid.NewGuid(), 1995, "Abraham Hosch"));
        //var person = await repo.Merge(new Person(1995, "Abraham Hosch"));
        //var person = await repo.Update(new Person(1995, "Abraham Hosch") { Id = 289 });
        //await repo.Delete(289);
        //await repo.DeleteWhere(n => n.born == 1995);
        //var count = await repo.Count(x => x.born == 1995);
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
    ValueTask<T?> Get<TId>(TId id) where TId : notnull;
    ValueTask<List<T>> GetWhere(Expression<Func<T, bool>> filter, int? skip = null, int? limit = null);
    ValueTask<T> Create(T record);
    ValueTask<T> Merge(T record);
    ValueTask<T?> Update(T record);
    ValueTask Delete<TId>(TId id) where TId : notnull;
    ValueTask DeleteWhere(Expression<Func<T, bool>> filter);
    ValueTask<long> Count(Expression<Func<T, bool>> filter);
    ValueTask<TRelationship> Connect<TId, TRelationship>(TId fromId, TId toId, TRelationship relationshipRecord)
        where TId : notnull
        where TRelationship : struct, ICypherNode;
    ValueTask<TRelationship> Merge<TId, TRelationship>(TId fromId, TId toId, TRelationship relationshipRecord)
         where TId : notnull
        where TRelationship : struct, ICypherNode;

    internal record QueryResult
    {
        public long Id { get; init; }
        public T Result { get; init; }
    };
}

// todo: support user-provided id instead of using Neo4j id
public class PersonRepository : IRepository<Person>
{
    private readonly Neo4jClient.GraphClient driver;

    public PersonRepository(Neo4jClient.GraphClient driver)
    {
        this.driver = driver;
    }

    public async ValueTask<Person?> Get<TId>(TId id)
        where TId : notnull
    {
        var idProp = $"n.{typeof(Person).GetIdPropertyName()}" ?? "ID(n)";

        var query = driver.Cypher
            .WithParams(new Dictionary<string, object>() { ["nodeId"] = id })
            .Match("(n:Person)")
            .Where($"{idProp} = $nodeId")
            .Return(n => new
            {
                id = n.Id(),
                person = n.As<Person>()
            });

        var response = (await query.ResultsAsync).First();
        return response.person with { Id = response.id };
    }

    public async ValueTask<List<Person>> GetWhere(Expression<Func<Person, bool>> filter, int? skip = null, int? limit = null)
    {
        var argName = filter.GetArgName() ?? "n";

        var returnExpr = Expression.Lambda<Func<Neo4jClient.Cypher.ICypherResultItem, IRepository<Person>.QueryResult>>(
            Expression.MemberInit(
                Expression.New(typeof(IRepository<Person>.QueryResult)),
                Expression.Bind(
                    typeof(IRepository<Person>.QueryResult).GetProperty("Id")!,
                    Expression.Call(
                        Expression.Parameter(typeof(Neo4jClient.Cypher.ICypherResultItem), argName),
                        typeof(Neo4jClient.Cypher.ICypherResultItem).GetMethod("Id")!)),
                    Expression.Bind(
                        typeof(IRepository<Person>.QueryResult).GetProperty("Result")!,
                        Expression.Call(
                            Expression.Parameter(typeof(Neo4jClient.Cypher.ICypherResultItem), argName),
                            "As", new Type[1] { typeof(Person) }))),
            Expression.Parameter(typeof(Neo4jClient.Cypher.ICypherResultItem), argName));

        var query = driver.Cypher
            .Match($"({argName}:Person)")
            .Where(filter)
            .Skip(skip)
            .Limit(limit)
            .Return(returnExpr);

        return (await query.ResultsAsync).Select(response => response.Result with { Id = response.Id }).ToList();
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
        var idProp = typeof(Person).GetIdPropertyName();
        var idFieldName = $"n.{idProp}" ?? "ID(n)";
        var idValue = idProp is null ? record.Id : typeof(Person).GetProperty(idProp)!.GetValue(record);

        var query = driver.Cypher
            .Match("(n:Person)")
            .WithParam("node", record)
            .WithParam("nodeId", idValue)
            .Where($"{idProp} = $nodeId")
            .Set("n = $node")
            .Return(n => new
            {
                id = n.Id(),
                person = n.As<Person>()
            });

        var response = (await query.ResultsAsync).FirstOrDefault();
        return response is null ? null : response.person with { Id = response.id };
    }

    public async ValueTask Delete<TId>(TId id)
        where TId : notnull
    {
        var idProp = $"n.{typeof(Person).GetIdPropertyName()}" ?? "ID(n)";

        var query = driver.Cypher
            .Match("(n:Person)")
            .WithParam("nodeId", id)
            .Where($"{idProp} = $nodeId")
            .DetachDelete("n");

        await query.ExecuteWithoutResultsAsync();
    }

    public async ValueTask DeleteWhere(Expression<Func<Person, bool>> filter)
    {
        var argName = filter.GetArgName() ?? "n";

        var query = driver.Cypher
            .Match($"({argName}:Person)")
            .Where(filter)
            .DetachDelete(argName);

        await query.ExecuteWithoutResultsAsync();
    }

    public async ValueTask<long> Count(Expression<Func<Person, bool>> filter)
    {
        var argName = filter.GetArgName() ?? "n";

        var returnExpr = Expression.Lambda<Func<Neo4jClient.Cypher.ICypherResultItem, long>>(
            Expression.Call(
                Expression.Parameter(typeof(Neo4jClient.Cypher.ICypherResultItem), argName),
                typeof(Neo4jClient.Cypher.ICypherResultItem).GetMethod("Count")!),
            Expression.Parameter(typeof(Neo4jClient.Cypher.ICypherResultItem), argName));

        var query = driver.Cypher
            .Match($"({argName}:Person)")
            .Where(filter)
            .Return(returnExpr);

        return (await query.ResultsAsync).First();
    }

    public async ValueTask<TRelationship> Connect<TId, TRelationship>(TId fromId, TId toId, TRelationship relationshipRecord)
        where TId : notnull
        where TRelationship : struct, ICypherNode
    {
        var fromIdProp = $"from.{typeof(Person).GetIdPropertyName()}" ?? "ID(from)";
        var toIdProp = $"to.{typeof(Person).GetIdPropertyName()}" ?? "ID(to)";

        var query = driver.Cypher
            .Match("(from)")
            .Match("(to)")
            .Where($"{fromIdProp} = $fromId AND {toIdProp} = $toId")
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
    public async ValueTask<TRelationship> Merge<TId, TRelationship>(TId fromId, TId toId, TRelationship relationshipRecord)
        where TId : notnull
        where TRelationship : struct, ICypherNode
    {
        var fromIdProp = $"from.{typeof(Person).GetIdPropertyName()}" ?? "ID(from)";
        var toIdProp = $"to.{typeof(Person).GetIdPropertyName()}" ?? "ID(to)";

        var query = driver.Cypher
            .Match("(from)")
            .Match("(to)")
            .Where($"{fromIdProp} = $fromId AND {toIdProp} = $toId")
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

public static class Extensions
{
    public static string? GetArgName<T>(this Expression<Func<T, bool>> expression)
    {
        return expression.Parameters[0].Name;
    }

    public static string? GetIdPropertyName(this Type type)
    {
        var idAttribute = typeof(Person).GetProperties().SingleOrDefault(f => f.CustomAttributes.Any(a => a.GetType() == typeof(IdAttribute)));
        return idAttribute?.Name;
    }
}

// todo: parameterize and write records to file
// todo: generate all attributes and supporting enums in source generator
// todo: generate IRepository<T> for each node record in source generator
// todo: generate supporting query methods for reading connected nodes/relationships
//       in source generator and update relationship properties
// question: generate IRepository<T> for relationships as well?