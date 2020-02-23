using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

#if SERVER_INPUT_SETUP
    var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
    var ghostId = GhostSerializerCollection.FindGhostType<CubeSnapshotData>();
    var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
    var player = EntityManager.Instantiate(prefab);
    EntityManager.SetComponentData(player, new MovableCubeComponent { PlayerId = EntityManager.GetComponentData<NetworkIdComponent>(req.SourceConnection).Value});

    PostUpdateCommands.AddBuffer<CubeInput>(player);
    PostUpdateCommands.SetComponent(req.SourceConnection, new CommandTargetComponent {targetEntity = player});
#endif

public struct CubeInput : ICommandData<CubeInput>
{
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;

    public void Deserialize(uint tick, DataStreamReader reader, ref DataStreamReader.Context ctx)
    {
        this.tick = tick;
        horizontal = reader.ReadInt(ref ctx);
        vertical = reader.ReadInt(ref ctx);
    }

    public void Serialize(DataStreamWriter writer)
    {
        writer.Write(horizontal);
        writer.Write(vertical);
    }

    public void Deserialize(uint tck, DataStreamReader reader, ref DataStreamReader.Context ctx, CubeInput baseline,
        NetworkCompressionModel compressionModel)
    {
        Deserialize(tck, reader, ref ctx);
    }

    public void Serialize(DataStreamWriter writer, CubeInput baseLine, NetworkCompressionModel compressionModel)
    {
        Serialize(writer);
    }
}

public class NetCodeSendCommandSystem : CommandSendSystem<CubeInput>
{

}

public class NetCodeReceiveCommandSystem : CommandReceiveSystem<CubeInput>
{

}

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SampleCubeInput : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableNetCodeGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<CubeInput>().ForEach((Entity ent, ref MovableCubeComponent cube) =>
            {
                if (cube.PlayerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<CubeInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent{targetEntity = ent});
                }
            });
            return;
        }

        var input = default(CubeInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        if (Input.GetKey(KeyCode.A)){
            input.horizontal -= 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            input.horizontal += 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            input.vertical -= 1;
        }
        if (Input.GetKey(KeyCode.W)) {
            input.vertical += 1;
        }
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}