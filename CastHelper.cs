using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabApi.Features.Wrappers;
using MEC;

namespace Magic;

public class CastHelper
{
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
    public static void CastFireBall(Vector3 origine, Vector3 direction, float size = 1, ReferenceHub? caster = null)
    {
        var ball = PrimitiveObjectToy.Create(origine + direction * 2); // HACK: Bug if the player is bigger than normal
        ball.Type = PrimitiveType.Sphere;
        ball.Color = new Color(1, 0, 0, 0.5f);
        //ball.MovementSmoothing = byte.MaxValue; // BUGGED :) 
        ball.MovementSmoothing = 2; // Fine
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        var collider = ball.GameObject.AddComponent<SphereCollider>();
        collider.radius = 0.25f; // Fine

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        if (caster != null)
            explodeScript.Attacker = new(caster);

        rigidbody.AddForce(direction * 2, ForceMode.VelocityChange);
    }

    // private static IEnumerator<float> MoveBall(PrimitiveObjectToy ball, Vector3 direction, float speed)
    // {
    //     while (ball?.Base?.gameObject) // BUG when object is destroy null ref bc of unity
    //     {
    //         ball.Transform.Translate(direction * speed * Time.deltaTime);
    //         yield return Timing.WaitForOneFrame;
    //     }
    // }

}
