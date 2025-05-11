namespace PathsFinder;

/// <summary>
/// Represents a single item in the shortest path result, containing location data and node ID.
/// </summary>
public class ShortestPathResultItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the node in the path.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the node.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the node.
    /// </summary>
    public double Longitude { get; set; }
}