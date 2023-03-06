namespace DatabaseChamp.Exceptions
{
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException(string datatype) : base($"No table found for datatype \"{datatype}\"")
        {
        }
    }

    public class WrongDatatypeException : Exception
    {
        public WrongDatatypeException(string expectedType, string actualType) : base($"Expected Type \"{expectedType}\" but found \"{actualType}\"")
        {
        }
    }
}
