using Grpc.Net.Client;
using CityGeneration.Contracts.gRPC; // Namespace of your shared library

// Test client
using var channel = GrpcChannel.ForAddress("https://localhost:7026");
var client = new CityGenData.CityGenDataClient(channel);
// 1. GetRoadNetwork
var roadData = await client.GetRoadNetworkAsync(new RoadRequest
{
    Seed = 0,
    Width = 512,
    Height = 512,
    SplitIterations = 3,
    BlockChance = 0.3f,
    Deviation = 0.2f
});
Console.WriteLine($"[1] Road network: {roadData.VertexList.Count} vertices, " +
                  $"{roadData.RoadSegments.Count} segments, " +
                  $"{roadData.Sectors.Count} sectors, cell_size={roadData.CellSize}");

// 2. GetBuildings
var buildings = await client.GetBuildingsAsync(roadData);
Console.WriteLine($"[2] Buildings: {buildings.Buildings.Count}");

// 3. GetPathBetweenPoints
if (roadData.VertexList.Count >= 2)
{
    var path = await client.GetPathBetweenPointsAsync(new RoadPathRequest
    {
        RoadData = roadData,
        StartId = 0,
        EndId = roadData.VertexList.Count - 1
    });
    Console.WriteLine($"[3] Path: {path.Path.Count} points");
}
else
{
    Console.WriteLine("[3] Skipped (not enough vertices).");
}