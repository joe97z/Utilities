using System.ComponentModel.DataAnnotations;

namespace PathsFinder;

/// <summary>
/// Represents a node in the graph with geographic coordinates and connections to other nodes.
/// </summary>
public class Node
{
    /// <summary>
    /// Gets or sets the unique identifier of the node.
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

    /// <summary>
    /// Gets or sets the list of nodes connected to this node.
    /// </summary>
    public List<ConnectedNode> ConnectedNodes { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this node is an entry point.
    /// </summary>
    public bool IsEntry { get; set; }
}