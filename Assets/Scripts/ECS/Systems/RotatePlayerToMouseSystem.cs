using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RotatePlayerToMouseSystem : SystemBase
{
    protected override void OnUpdate(){

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
        }

        Entities
            .WithoutBurst()
            .WithAll<PlayerTag>()
            .ForEach((ref Translation trans, ref Rotation rot) =>
            {
                var blah = new Vector3(trans.Value.x, trans.Value.y, trans.Value.z);            
                Vector3 rotation = Quaternion.LookRotation(hit.point - blah).eulerAngles;
                rotation.x = 0f;
                rotation.z = 0f;

                rot.Value = Quaternion.Euler(rotation);
            })
            .Run();
    
    }
}
