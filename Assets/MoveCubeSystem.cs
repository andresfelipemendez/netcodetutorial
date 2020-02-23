using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MoveCubeSystem : ComponentSystem
{
    
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime * 100;
        Entities.ForEach((DynamicBuffer<CubeInput> inputBuffer, ref Translation trans, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;

            CubeInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            var translation = trans.Value;
            

            if (input.horizontal > 0)
                translation.x += deltaTime;
            if (input.horizontal < 0){
                translation.x -= deltaTime;
                Debug.Log("left" + deltaTime);
            }
            if (input.vertical > 0)
                translation.z += deltaTime;
            if (input.vertical < 0)
                translation.z -= deltaTime;

            
            trans.Value = translation ;
        });
    }
}