namespace DatabaseChamp.Exceptions
{
    public class DublicateException : Exception
    {
        public DublicateException() : base($"Item already exists in collection")
        {
        }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string datatype) : base($"No matching item found in the collection for datatype \"{datatype}\"")
        {
        }
    }

    public class TableNotFoundException : Exception
    {
        public TableNotFoundException(string datatype) : base($"No table found for datatype \"{datatype}\"")
        {
        }
    }
}
