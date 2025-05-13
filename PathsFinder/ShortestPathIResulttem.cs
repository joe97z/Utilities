namespace PathsFinder;

/// <summary>
/// Represents a single item in the shortest path result, containing location data and node ID with int Id.
/// </summary>
public class ShortestPathResultItem : ShortestPathResultItem<int>
{
}


/// <summary>
/// Represents a single item in the shortest path result, containing location data and node ID with generic Id.
/// </summary>
public class ShortestPathResultItem<TID> where TID : IEquatable<TID>
{
    /// <summary>
    /// Gets or sets the unique identifier of the node in the path.
    /// </summary>
    public required TID Id { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the node.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the node.
    /// </summary>
    public double Longitude { get; set; }
}