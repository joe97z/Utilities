# PathsFinder

## Overview
The `PathsFinder` NuGet package provides a simple and efficient way to find the shortest path between nodes in a graph using Dijkstra's algorithm. It is designed for scenarios where you need to calculate the shortest path from a target node to the closest entry node, optionally considering a current location to prioritize the nearest entry point.

## Installation
To install the `PathsFinder` package, use the following command in the Package Manager Console:

```bash
Install-Package PathsFinder
```

Alternatively, you can install it via the .NET CLI:

```bash
dotnet add package PathsFinder
```

## Usage
The primary functionality is provided by the `PathFinder` class in the `PathsFinder` namespace. Below is an example of how to use it:

### Example
```csharp
using PathsFinder;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Define nodes
        var nodes = new List<Node>
        {
            new Node { Id = 1, Latitude = 0, Longitude = 0, IsEntry = true, ConnectedNodes = new List<ConnectedNode> { new ConnectedNode { Id = 2, Distance = 1 } } },
            new Node { Id = 2, Latitude = 1, Longitude = 1, IsEntry = false, ConnectedNodes = new List<ConnectedNode> { new ConnectedNode { Id = 1, Distance = 1 }, new ConnectedNode { Id = 3, Distance = 1 } } },
            new Node { Id = 3, Latitude = 2, Longitude = 2, IsEntry = true, ConnectedNodes = new List<ConnectedNode> { new ConnectedNode { Id = 2, Distance = 1 } } }
        };

        // Define current location (optional)
        var currentLocation = new CurrentLocation { Latitude = 0.5, Longitude = 0.5 };

        // Find shortest path
        var path = PathFinder.FindShortestPath(nodes, targetNodeId: 2, currentLocation);

        // Output the path
        foreach (var node in path)
        {
            Console.WriteLine($"Node ID: {node.Id}, Latitude: {node.Latitude}, Longitude: {node.Longitude}");
        }
    }
}
```

### Key Classes and Methods
- **PathFinder.FindShortestPath**: Finds the shortest path from the target node to the closest entry node. If a `currentLocation` is provided, it prioritizes the entry node closest to that location.
  - **Parameters**:
    - `nodes`: A list of `Node` objects representing the graph.
    - `targetNodeId`: The ID of the target node.
    - `currentLocation`: Optional. A `CurrentLocation` object to find the closest entry node to this point.
  - **Returns**: A list of `ShortestPathResultItem` objects representing the shortest path.

- **Node**: Represents a node in the graph with properties like `Id`, `Latitude`, `Longitude`, `IsEntry`, and `ConnectedNodes`.
- **ConnectedNode**: Represents a connection to another node with an `Id` and `Distance`.
- **CurrentLocation**: Holds `Latitude` and `Longitude` for the current location.
- **ShortestPathResultItem**: Contains `Id`, `Latitude`, and `Longitude` for each node in the resulting path.

## Requirements
- .NET 9

## License
This package is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
