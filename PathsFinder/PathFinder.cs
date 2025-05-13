
using System.Linq;
using System.Security.Cryptography;

namespace PathsFinder;
/// <summary>
/// Provides methods for finding the shortest path between nodes.
/// </summary>
public static class PathFinder
{
    /// <summary>
    /// Finds the shortest path from the start node to the closest entry node.
    /// If current location is provided, it will choose the closest entry node to the location.
    /// </summary>
    /// <param name="nodes">A list of nodes representing the graph.</param>
    /// <param name="targetNodeId">The ID of the target node.</param>
    /// <param name="currentLocation">Optional current location to find the closest entry node to this point.</param>
    /// <returns>A list of <see cref="ShortestPathResultItem"/> representing the shortest path.</returns>
    public static List<ShortestPathResultItem> FindShortestPath(List<Node> nodes, int targetNodeId, CurrentLocation? currentLocation = null)
    {
        var distances = nodes.ToDictionary(node => node.Id, node => double.MaxValue);
        var previousNodes = nodes.ToDictionary(node => node.Id, node => (int?)null);
        var unvisitedNodes = new HashSet<int>(nodes.Select(n => n.Id));
        var roadNodes = new Dictionary<int, double>();

        distances[targetNodeId] = 0;

        while (unvisitedNodes.Count > 0)
        {
            var currentNodeId = unvisitedNodes.OrderBy(n => distances[n]).First();
            var currentNode = nodes.First(n => n.Id == currentNodeId);

            if (currentNode.IsEntry && (currentLocation?.Latitude == null || currentLocation?.Longitude == null))
            {
                return GetPath(nodes, previousNodes, currentNodeId, targetNodeId);
            }

            if (currentNode.IsEntry)
            {
                roadNodes[currentNodeId] = distances[currentNodeId];
            }

            unvisitedNodes.Remove(currentNodeId);

            foreach (var connectedNode in currentNode.ConnectedNodes)
            {
                if (!unvisitedNodes.Contains(connectedNode.Id)) continue;

                var newDistance = distances[currentNodeId] + connectedNode.Distance;
                if (newDistance < distances[connectedNode.Id])
                {
                    distances[connectedNode.Id] = newDistance;
                    previousNodes[connectedNode.Id] = currentNodeId;
                }
            }
        }

        if (roadNodes.Count == 0)
        {
            return [];
        }

        if (currentLocation?.Latitude != null && currentLocation?.Longitude != null)
        {
            var closestRoadNodeId = roadNodes.Keys
                .Select(id => new
                {
                    Id = id,
                    DistanceToLocation = CalculateEuclideanDistance(
                        nodes.First(n => n.Id == id).Latitude,
                        nodes.First(n => n.Id == id).Longitude,
                        (double)currentLocation.Latitude,
                        (double)currentLocation.Longitude)
                })
                .OrderBy(x => x.DistanceToLocation)
                .First()
                .Id;

            return GetPath(nodes, previousNodes, closestRoadNodeId, targetNodeId);
        }

        return [];
    }

    public static List<ShortestPathResultItem<TID>> FindShortesPath<TID>(List<Node<TID>> nodes, TID targetNodeId, CurrentLocation? currentLocation = null) where TID : IEquatable<TID>
    {
        var distances = nodes.ToDictionary(node => node.Id, node => double.MaxValue);
        Dictionary<TID, TID?> previousNodes = nodes.ToDictionary(n => n.Id, _ => default(TID?));
        var unvisitedNodes = new HashSet<TID>(nodes.Select(n => n.Id));
        var roadNodes = new Dictionary<TID, double>();

        distances[targetNodeId] = 0;

        while (unvisitedNodes.Count > 0)
        {
            var currentNodeId = unvisitedNodes.OrderBy(n => distances[n]).First();
            var currentNode = nodes.First(n => EqualityComparer<TID>.Default.Equals(n.Id, currentNodeId));

            if (currentNode.IsEntry && (currentLocation?.Latitude == null || currentLocation?.Longitude == null))
            {
                return GetPath<TID>(nodes, previousNodes, currentNodeId, targetNodeId);
            }

            if (currentNode.IsEntry)
            {
                roadNodes[currentNodeId] = distances[currentNodeId];
            }

            unvisitedNodes.Remove(currentNodeId);

            foreach (var connectedNode in currentNode.ConnectedNodes)
            {
                if (!unvisitedNodes.Contains(connectedNode.Id)) continue;

                var newDistance = distances[currentNodeId] + connectedNode.Distance;
                if (newDistance < distances[connectedNode.Id])
                {
                    distances[connectedNode.Id] = newDistance;
                    previousNodes[connectedNode.Id] = currentNodeId;
                }
            }
        }

        if (roadNodes.Count == 0)
        {
            return [];
        }

        if (currentLocation?.Latitude != null && currentLocation?.Longitude != null)
        {
            var closestRoadNodeId = roadNodes.Keys
                .Select(id => new
                {
                    Id = id,
                    DistanceToLocation = CalculateEuclideanDistance(
                        nodes.First(n => EqualityComparer<TID>.Default.Equals(n.Id, id)).Latitude,
                       nodes.First(n => EqualityComparer<TID>.Default.Equals(n.Id, id)).Longitude
,
                        (double)currentLocation.Latitude,
                        (double)currentLocation.Longitude)
                })
                .OrderBy(x => x.DistanceToLocation)
                .First()
                .Id;

            return GetPath<TID>(nodes, previousNodes, closestRoadNodeId, targetNodeId);
        }

        return [];
    }
    private static List<ShortestPathResultItem> GetPath(List<Node> nodes, Dictionary<int, int?> previousNodes, int currentNodeId, int targetNodeId)
    {
        var path = new List<ShortestPathResultItem>();

        while (previousNodes[currentNodeId] != null)
        {
            var node = nodes.First(n => n.Id == currentNodeId);
            path.Add(new ShortestPathResultItem
            {
                Id = node.Id,
                Latitude = node.Latitude,
                Longitude = node.Longitude
            });
            currentNodeId = previousNodes[currentNodeId]!.Value;
        }

        var targetNode = nodes.First(n => n.Id == targetNodeId);
        path.Add(new ShortestPathResultItem
        {
            Id = targetNode.Id,
            Latitude = targetNode.Latitude,
            Longitude = targetNode.Longitude
        });

        path.Reverse();
        return path;
    }



    private static List<ShortestPathResultItem<TID>> GetPath<TID>(List<Node<TID>> nodes, Dictionary<TID, TID?> previousNodes, TID currentNodeId, TID targetNodeId) where TID : IEquatable<TID>
    {
        var path = new List<ShortestPathResultItem<TID>>();

        while (previousNodes[currentNodeId] != null)
        {
            var node = nodes.First(n => EqualityComparer<TID>.Default.Equals(n.Id, currentNodeId));
            path.Add(new ShortestPathResultItem<TID>
            {
                Id = node.Id,
                Latitude = node.Latitude,
                Longitude = node.Longitude
            });
            currentNodeId = previousNodes[currentNodeId]!;
        }

        var targetNode = nodes.First(n => EqualityComparer<TID>.Default.Equals(n.Id, targetNodeId));
        path.Add(new ShortestPathResultItem<TID>
        {
            Id = targetNode.Id,
            Latitude = targetNode.Latitude,
            Longitude = targetNode.Longitude
        });

        path.Reverse();
        return path;
    }
    private static double CalculateEuclideanDistance(double lat1, double lng1, double lat2, double lng2)
    {
        return Math.Sqrt(Math.Pow(lat2 - lat1, 2) + Math.Pow(lng2 - lng1, 2));
    }
}
