namespace HotChocolate.Stitching.Types;

public class SchemaCoordinatePrinter
{
    public static string Print(ISchemaCoordinate2 coordinate)
    {
        return coordinate?.Name?.Value
               ?? coordinate?.GetHashCode().ToString()
               ?? "(null)";
    }
}