using CityGeneration.Contracts.Entities;
using CityGeneration.Contracts.gRPC;
using CityGeneration.Contracts.Models;
using Grpc.Core;

namespace CityGeneration.Api.gRPC.Services;

public class CityGenDataService(ILogger<CityGenDataService> logger) : CityGenData.CityGenDataBase
{
    public override Task<RoadData> GetRoadNetwork(RoadRequest request, ServerCallContext context)
    {
        logger.LogInformation("Roads requested");
        var cityResult = CityDataManager.CreateAndRetrieveCityData(request.Seed, request.Width, request.Height, request.SplitIterations, request.BlockChance, request.Deviation);
        var result = new RoadData
        {
            VertexList = {},
            VertexEdges = {},
            RoadSegments = {},
            Sectors = {}
        };
        foreach (var point in cityResult.VertexList) result.VertexList.Add(new Vector2() { X = point.X, Y = point.Y });
        foreach (var item in cityResult.RoadSegments) result.RoadSegments.Add(new RoadSegment() {StartId = item.StartId, EndId = item.EndId, Type = item.Type});
        foreach (var item in cityResult.Sectors)
        {
            RoadSector sector = new RoadSector();
            sector.Edges.AddRange(item.Edges);
            sector.VertexList.AddRange(item.VertexList);
            result.Sectors.Add(sector);
        }
        foreach (var item in cityResult.VertexEdges)
        {
            result.VertexEdges.Add(item.Key, new IntList());
            result.VertexEdges[item.Key].Values.AddRange(item.Value);
        }
        return Task.FromResult(result);
    }
    
    public override Task<BuildingReply> GetBuildings(RoadData request, ServerCallContext context)
    {
        logger.LogInformation("Buildings requested");
        var roadNetworkData = new RoadPlacementData()
        {
            VertexList = [],
            RoadSegments = [],
            Sectors = [],
            VertexEdges = []
        };
        foreach (var item in request.VertexList) roadNetworkData.VertexList.Add(new Core.Data.DataModels.Vector2(item.X, item.Y));
        foreach (var item in request.RoadSegments) roadNetworkData.RoadSegments.Add(new Core.Data.DataModels.RoadSegment() {StartId = item.StartId, EndId = item.EndId, Type = item.Type});
        foreach (var item in request.Sectors)
        {
            var sector = new Core.Data.DataModels.RoadSector();
            sector.Edges.AddRange(item.Edges);
            sector.VertexList.AddRange(item.VertexList);
            roadNetworkData.Sectors.Add(sector);
        }
        foreach (var item in request.VertexEdges)
        {
            roadNetworkData.VertexEdges.Add(item.Key, []);
            roadNetworkData.VertexEdges[item.Key].AddRange(item.Value.Values);
        }
        var roadNetwork = CityDataManager.LoadRoadNetwork(roadNetworkData);
        var cityResult = CityDataManager.GetBuildingsInNetwork(roadNetwork, roadNetworkData.cellSize);
        BuildingReply  result = new BuildingReply();
        foreach (var item in cityResult)
        {
            var building = new Building();
            foreach (var corner in  item.Corners)
            {
                building.Corners.Add(new Vector2() {X = corner.X, Y = corner.Y});
            }
            result.Buildings.Add(building);
        }
        return Task.FromResult(result);
    }

    public override Task<RoadPathReply> GetPathBetweenPoints(RoadPathRequest request, ServerCallContext context)
    {
        logger.LogInformation("Path requested");
        logger.LogInformation("Buildings requested");
        var roadNetworkData = new RoadPlacementData()
        {
            VertexList = [],
            RoadSegments = [],
            Sectors = [],
            VertexEdges = []
        };
        foreach (var item in request.RoadData.VertexList) roadNetworkData.VertexList.Add(new Core.Data.DataModels.Vector2(item.X, item.Y));
        foreach (var item in request.RoadData.RoadSegments) roadNetworkData.RoadSegments.Add(new Core.Data.DataModels.RoadSegment() {StartId = item.StartId, EndId = item.EndId, Type = item.Type});
        foreach (var item in request.RoadData.Sectors)
        {
            var sector = new Core.Data.DataModels.RoadSector();
            sector.Edges.AddRange(item.Edges);
            sector.VertexList.AddRange(item.VertexList);
            roadNetworkData.Sectors.Add(sector);
        }
        foreach (var item in request.RoadData.VertexEdges)
        {
            roadNetworkData.VertexEdges.Add(item.Key, []);
            roadNetworkData.VertexEdges[item.Key].AddRange(item.Value.Values);
        }
        var roadNetwork = CityDataManager.LoadRoadNetwork(roadNetworkData);
        var result = CityDataManager.GetPathBetweenTwoVIds(roadNetwork, request.StartId, request.EndId);
        var path = new RoadPathReply();
        path.Path.AddRange(result);
        return Task.FromResult(path);
    }
}