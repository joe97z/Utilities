namespace PathsFinder;

/// <summary>
/// Represents a node that is connected to another node in a graph.
/// </summary>
public class ConnectedNode
{
    /// <summary>
    /// Gets or sets the distance (or weight) to the connected node.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the connected node.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the connected node.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the connected node.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node is an entry point.
    /// </summary>
    public bool IsEntry { get; set; }
}