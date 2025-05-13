# PathsFinder

## Overview
The `PathsFinder` NuGet package provides a simple and efficient way to find the shortest path between nodes in a graph using Dijkstra's algorithm. It is designed for scenarios where you need to calculate the shortest path from a target node to the closest entry node, optionally considering a current location to prioritize the nearest entry point. The package supports generic node IDs, allowing flexibility to use any type for node identification (e.g., `int`, `Guid`, `string`), with `int` as the default.

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
The primary functionality is provided by the `PathFinder` class in the `PathsFinder` namespace. The package supports both non-generic (`Node` with `int` IDs) and generic (`Node<TId>` with any type for IDs) node types. Below are examples for both scenarios.

### Example 1: Using Default `Node` with `int` IDs
```csharp
using PathsFinder;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Define nodes with int IDs
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

### Example 2: Using Generic `Node<Guid>` with `Guid` IDs
```csharp
using PathsFinder;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Define nodes with Guid IDs
        var node1Id = Guid.NewGuid();
        var node2Id = Guid.NewGuid();
        var node3Id = Guid.NewGuid();

        var nodes = new List<Node<Guid>>
        {
            new Node<Guid> { Id = node1Id, Latitude = 0, Longitude = 0, IsEntry = true, ConnectedNodes = new List<ConnectedNode<Guid>> { new ConnectedNode<Guid> { Id = node2Id, Distance = 1 } } },
            new Node<Guid> { Id = node2Id, Latitude = 1, Longitude = 1, IsEntry = false, ConnectedNodes = new List<ConnectedNode<Guid>> { new ConnectedNode<Guid> { Id = node1Id, Distance = 1 }, new ConnectedNode<Guid> { Id = node3Id, Distance = 1 } } },
            new Node<Guid> { Id = node3Id, Latitude = 2, Longitude = 2, IsEntry = true, ConnectedNodes = new List<ConnectedNode<Guid>> { new ConnectedNode<Guid> { Id = node2Id, Distance = 1 } } }
        };

        // Define current location (optional)
        var currentLocation = new CurrentLocation { Latitude = 0.5, Longitude = 0.5 };

        // Find shortest path
        var path = PathFinder.FindShortestPath<Guid>(nodes, targetNodeId: node2Id, currentLocation);

        // Output the path
        foreach (var node in path)
        {
            Console.WriteLine($"Node ID: {node.Id}, Latitude: {node.Latitude}, Longitude: {node.Longitude}");
        }
    }
}
```

### Key Classes and Methods
- **PathFinder.FindShortestPath** (non-generic, for `int` IDs): Finds the shortest path from the target node to the closest entry node using `Node` objects with `int` IDs.
  - **Parameters**:
    - `nodes`: A list of `Node` objects representing the graph.
    - `targetNodeId`: The `int` ID of the target node.
    - `currentLocation`: Optional. A `CurrentLocation` object to find the closest entry node to this point.
  - **Returns**: A list of `ShortestPathResultItem` objects representing the shortest path.

- **PathFinder.FindShortestPath<TId>** (generic, for any ID type): Finds the shortest path using `Node<TId>` objects with custom ID types (e.g., `Guid`, `string`).
  - **Parameters**:
    - `nodes`: A list of `Node<TId>` objects representing the graph.
    - `targetNodeId`: The ID of the target node (of type `TId`).
    - `currentLocation`: Optional. A `CurrentLocation` object to find the closest entry node to this point.
  - **Returns**: A list of `ShortestPathResultItem<TId>` objects representing the shortest path.

- **Node** / **Node<TId>**: Represents a node in the graph with properties like `Id` (or `Id` of type `TId`), `Latitude`, `Longitude`, `IsEntry`, and `ConnectedNodes`.
- **ConnectedNode** / **ConnectedNode<TId>**: Represents a connection to another node with an `Id` (or `Id` of type `TId`) and `Distance`.
- **CurrentLocation**: Holds `Latitude` and `Longitude` for the current location.
- **ShortestPathResultItem** / **ShortestPathResultItem<TId>**: Contains `Id` (or `Id` of type `TId`), `Latitude`, and `Longitude` for each node in the resulting path.

## Requirements
- .NET 9

## Changelog
### Version 2.0.0 (2025-05-13)
- **Added**: Support for generic node IDs. Users can now use `Node<TId>` and `PathFinder.FindShortestPath<TId>` with any type for node IDs (e.g., `Guid`, `string`), while maintaining `int` as the default with `Node` and `PathFinder.FindShortestPath`.
- **Improved**: Enhanced flexibility in graph construction for diverse use cases.

### Version 1.0.0 (Initial Release)
- Initial release with Dijkstra's algorithm for shortest path calculation.
- Support for `int` node IDs with `Node` and `PathFinder.FindShortestPath`.
- Optional `CurrentLocation` to prioritize the nearest entry node.

## License
This package is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.