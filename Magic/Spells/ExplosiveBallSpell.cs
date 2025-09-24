using Footprinting;
using LabApi.Features.Wrappers;
using Magic.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Spells;

internal class ExplosiveBallSpell : Spell
{
    public static Color ExplosiveBallColor = new Color(1, 0, 0, 0.5f);
    public static Color ExplosiveBallLightColor = new Color(0.75f, 0.5f, 0, 0.5f);

    public Color FireBallColor = ExplosiveBallColor;
    public Color FireBallLightColor = ExplosiveBallLightColor;
    public int Speed = 10;
    public float LiftTime = 5;
    public float Size = 1;
    
    // TODO: Changer avec le styeme de particule
    private LightSourceToy? _castedLight;
    private PrimitiveObjectToy? _ball;
    private SphereCollider? _coilider;

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
        ball.Color = ExplosiveBallColor;
        //ball.MovementSmoothing = byte.MaxValue; // BUGGED :) 
        ball.MovementSmoothing = 160; // Fine
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        ball.Spawn();

        var light = LightSourceToy.Create(ball.Transform, false);
        light.Intensity = 1;
        light.Color = ExplosiveBallLightColor;
        light.ShadowType = LightShadows.None;
        light.Spawn();

        var collider = ball.GameObject.AddComponent<SphereCollider>();
        collider.radius = 0.25f; // Fine

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects

        var gravityScript = ball.GameObject.AddComponent<CustomGravity>();
        gravityScript.GravityDirection = Vector3.down;
        gravityScript.GravityStrength = 5.81f; // Ajuste comme tu veux

        

        rigidbody.AddForce(Caster.Hub.PlayerCameraReference.forward * Speed, ForceMode.VelocityChange);

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        explodeScript.Attacker = Caster;

        var destryCallBack = ball.GameObject.AddComponent<DestroyCallBack>();
        destryCallBack.CallBack = InvokeDestroy;

        var lifetime = ball.GameObject.AddComponent<DestroyAfterTime>();
        lifetime.SetDuration(LiftTime);
        base.Cast();
    }
}
