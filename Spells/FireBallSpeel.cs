using Footprinting;
using LabApi.Features.Wrappers;
using Magic.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Spells;

internal class FireBallSpell : Spell
{
    public static readonly Color DefaultFireBallColor = new Color(1, 0, 0, 1f);
    public static readonly Color DefaultFireBallLightColor = new Color(1f, 0f, 0, 0.5f);

    public Color FireBallColor = DefaultFireBallColor;
    public Color FireBallLightColor = DefaultFireBallLightColor;
    public int Speed = 20;
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
        ball.Color = FireBallColor;
        ball.MovementSmoothing = 128; // Fine
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        ball.Spawn();

        var light = LightSourceToy.Create(ball.Transform, false);
        light.Intensity = 1;
        light.Range = Size;
        light.Color = FireBallLightColor;
        light.ShadowType = LightShadows.None;
        light.Spawn();

        _coilider = ball.GameObject.AddComponent<SphereCollider>();
        _coilider.radius = 0.01f * Size;

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects
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
