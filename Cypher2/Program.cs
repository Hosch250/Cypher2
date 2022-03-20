namespace Cypher2
{
    using Neo4j.Driver;
    using CypherGenerator;

    public partial class Program
    {
        static void Main()
        {
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