using Newtonsoft.Json;
using static Cypher2.PersonToMovieRelationships;

namespace Cypher2;

[AttributeUsage(AttributeTargets.Class)]
public class CypherLabelAttribute : Attribute
{
    public string Label { get; }
    public CypherLabelAttribute(string label)
    {
        Label = label;
    }
}

public enum Direction { FROM, TO }

[AttributeUsage(AttributeTargets.Property)]
public class CypherRelationshipAttribute : Attribute
{
    public string Type { get; }
    public Direction Direction { get; }
    public CypherRelationshipAttribute(string type, Direction direction)
    {
        Type = type;
        Direction = direction;
    }
}


[AttributeUsage(AttributeTargets.Property)]
public class IdAttribute : Attribute
{
}

public interface ICypherNode
{
    [JsonIgnore]
    public long Id { get; set; }
}

[CypherLabel("Person")]
public record Person([property:Id] Guid id, long? born, string name) : ICypherNode
{
    public Person() : this(Guid.Empty, null, "") { }

    [JsonIgnore]
    public long Id { get; set; }

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.ACTED_IN), Direction.FROM)]
    public List<(ACTED_IN, Movie)> ACTED_IN { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.REVIEWED), Direction.FROM)]
    public List<(REVIEWED, Movie)> REVIEWED { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.PRODUCED), Direction.FROM)]
    public List<Movie> PRODUCED { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.WROTE), Direction.FROM)]
    public List<Movie> WROTE { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToPersonRelationships.FOLLOWS), Direction.FROM)]
    public List<Person> FOLLOWS { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.DIRECTED), Direction.FROM)]
    public List<Movie> DIRECTED { get; } = new();
}

[CypherLabel("Movie")]
public record Movie([property: Id] Guid id, string? tagline, string title, long released) : ICypherNode
{
    public Movie() : this(Guid.Empty, null, "", 0) { }

    [JsonIgnore]
    public long Id { get; set; }

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.ACTED_IN), Direction.TO)]
    public List<(ACTED_IN, Person)> ACTED_IN { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.REVIEWED), Direction.TO)]
    public List<(REVIEWED, Person)> REVIEWED { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.PRODUCED), Direction.TO)]
    public List<Person> PRODUCED { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.WROTE), Direction.TO)]
    public List<Person> WROTE { get; } = new();

    [JsonIgnore]
    [CypherRelationship(nameof(PersonToMovieRelationships.DIRECTED), Direction.TO)]
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
