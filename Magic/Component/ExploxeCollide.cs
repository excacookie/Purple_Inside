using Footprinting;
using InventorySystem;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;
using MEC;
using Utils;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;

namespace Magic.Component;

internal class ExploxeCollide : MonoBehaviour
{
    #region Properties & Variables
    public static Action<Vector3, Footprint> DefaultExplsoion = (pos, attacker) =>
    {
        ExplosionUtils.ServerSpawnEffect(pos, ItemType.GrenadeHE);
        if (InventoryItemLoader.TryGetItem<ThrowableItem>(ItemType.GrenadeHE, out var result) && Instantiate(result.Projectile) is ExplosionGrenade grenade)
        {
            grenade.Position = pos;
            grenade.PreviousOwner = attacker;
            grenade.ServerFuseEnd();
        }
    };
    
    public Footprint Attacker;
    public Action<Vector3, Footprint> OnExplode = DefaultExplsoion;
    #endregion

    #region Methods
    public void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;

        Logger.Info("collisition de la boule avec un mur");
        var pos = transform.position; // store after destroy we can't access tranform
        Destroy(gameObject);
        enabled = false;
#if DEBUG
        Timing.CallDelayed(0.1f, () =>
        {
            var ballexplosion=PrimitiveObjectToy.Create(pos);
            ballexplosion.Type = PrimitiveType.Sphere;
            ballexplosion.Color = Color.yellow;

            Timing.CallDelayed(3f, () =>
            {
                Destroy(ballexplosion.GameObject);
            });
        });
#endif
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            OnExplode(pos, Attacker);
        });
    }

    public void Start()
    {
        if (Attacker.Equals(default))
        {
            if (!ReferenceHub.TryGetHostHub(out var owner))
            {
                throw new Exception("host not found for the fire ball caste");
            }
            Attacker = new Footprint(owner);
        }
    }
    #endregion
}
