using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Spells;

internal class FireBallSpell : Spell
{
    public static Color FireBallColor = new Color(1, 0, 0, 1f);
    public static Color FireBallLightColor = new Color(1f, 0f, 0, 0.5f);

    private static Vector3 GetPositionCameraForward(ReferenceHub player)
    {
        var result = player.PlayerCameraReference.position; //+ player.PlayerCameraReference.forward * 3;
        result += player.PlayerCameraReference.forward * player.transform.localScale.z;
        return result;
    }

    public override void Cast()
    {
        var ball = PrimitiveObjectToy.Create(GetPositionCameraForward(Caster.Hub), networkSpawn: false);
        ball.Type = PrimitiveType.Sphere;
        ball.Color = FireBallColor;

        ball.MovementSmoothing = 128; // Fine
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        ball.Spawn();

        var light = LightSourceToy.Create(ball.Transform, false);
        light.Intensity = 1;
        light.Color = FireBallLightColor;
        light.ShadowType = LightShadows.None;
        light.Spawn();

        var collider = ball.GameObject.AddComponent<SphereCollider>();
        collider.radius = 0.01f;

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        
        explodeScript.Attacker = Caster;

        var speed = 20;
        rigidbody.AddForce(Caster.Hub.PlayerCameraReference.forward * speed, ForceMode.VelocityChange);

        var lifetime = ball.GameObject.AddComponent<DestroyAfterTime>();
        lifetime.SetDuration(5f);
    }

    public override void PlayerStopCast()
    {
        throw new NotImplementedException();
    }

    public override void StartCast()
    {
        Cast();
    }

    public override void StopCast()
    {
        throw new NotImplementedException();
    }
}
