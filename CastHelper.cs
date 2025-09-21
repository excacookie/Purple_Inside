using CustomPlayerEffects;
using InventorySystem.Items.Usables;
using InventorySystem.Items.Usables.Scp244.Hypothermia;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerStatsSystem;

namespace Magic;

public static class CastHelper
{
    public static Color FireBallColor = new Color(1, 0, 0, 1f);
    public static Color FireBallLightColor = new Color(1f, 0f, 0, 0.5f);
    public static Color ExplosiveBallColor = new Color(1, 0, 0, 0.5f);
    public static Color ExplosiveBallLightColor = new Color(0.75f, 0.5f, 0, 0.5f);

    /// <summary>
    /// Spawn a fire ball and move until coliding.
    /// An explosion occure when coliding.
    /// </summary>
    /// <param name="origine">The origne of the fire ball.</param>
    /// <param name="direction">The noramlized directino.</param>
    /// <param name="caster">
    /// The player casting the fire ball, witch is the player dealing damage.
    /// If <see langword="null"/> the host will be use instade.
    /// </param>

    public static void FireBall(Vector3 origine, Vector3 direction, float speed = 10, ReferenceHub? caster = null)
    {
        var ball = PrimitiveObjectToy.Create(origine, networkSpawn: false);
        ball.Type = PrimitiveType.Sphere;
        ball.Color = ExplosiveBallColor;

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
        if (caster != null)
            explodeScript.Attacker = new(caster);

        rigidbody.AddForce(direction * speed, ForceMode.VelocityChange);

        var lifetime = ball.GameObject.AddComponent<DestroyAfterTime>();
        lifetime.SetDuration(5f);
    }

    public static void ExplosivBall(Vector3 origine, Vector3 direction, float speed = 10, ReferenceHub? caster = null)
    {
        var ball = PrimitiveObjectToy.Create(origine, networkSpawn: false);
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

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        if (caster != null)
            explodeScript.Attacker = new(caster);

        rigidbody.AddForce(direction * speed, ForceMode.VelocityChange);
    }

    // TODO size
    public static void IceBall(Vector3 origine, Vector3 direction, float speed = 1, ReferenceHub? caster = null)
    {
        var ball = PrimitiveObjectToy.Create(origine, networkSpawn: false);
        ball.Transform.LookAt(origine + direction);
        ball.Type = PrimitiveType.Sphere;
        ball.Color = new Color(0f, 1f, 1f, 0.5f);
        ball.Scale = new Vector3(1 / speed, 1 / speed, 1f);
        ball.MovementSmoothing = 2; // Fine
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        ball.Spawn();

        var light = LightSourceToy.Create(ball.Transform, false);
        light.Intensity = 1;
        light.Color = new Color(0, 0, 0.1f);
        light.ShadowType = LightShadows.None;
        light.Spawn();

        var collider = ball.GameObject.AddComponent<SphereCollider>();
        collider.radius = 0.25f; // Fine

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        if (caster != null)
            explodeScript.Attacker = new(caster);

        // TODO: move in variable or method
        explodeScript.OnExplode = (pos, attacker) =>
        {
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (Vector3.Distance(hub.transform.position, pos) > 1) continue;
                if (!HitboxIdentity.IsDamageable(attacker.Role, hub.GetRoleId())) continue;

                hub.playerEffectsController.EnableEffect<Deafened>(10, true);
                var hypo = hub.playerEffectsController.EnableEffect<Hypothermia>(5, true);
                var slow = hub.playerEffectsController.EnableEffect<Slowness>(3, true);
                slow.Intensity = 60;
                hypo.Intensity = 255;
            }
        };

        rigidbody.AddForce(direction * speed, ForceMode.VelocityChange);
    }

    public static void Heal(Vector3 origine, float heal, float size = 1)
    {
        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (Vector3.Distance(hub.transform.position, origine) > size)
                continue;
            hub.playerStats.GetModule<HealthStat>().ServerHeal(heal);
        }
    }


    public static void Heal(Vector3 origine, AnimationCurve? curve, float size = 1)
    {
        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (Vector3.Distance(hub.transform.position, origine) > size)
                continue;
            if (curve == null)
            {
                curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(2, 11), new Keyframe(2.1f, 0), new Keyframe(4.1f, 11));
            }
            
            UsableItemsController.GetHandler(hub).ActiveRegenerations.Add(new RegenerationProcess(curve, 1, 1));
        }
    }

}
