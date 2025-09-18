using Footprinting;
using InventorySystem;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;
using Utils;

namespace Magic
{
    internal class ExploxeCollide : MonoBehaviour
    {
        #region Properties & Variables
        public Footprint Attacker;
        #endregion

        #region Methods
        /*
        public void Update()
        {
        }
        */

        public void OnCollisionEnter(Collision collision)
        {
            Logger.Info("collisition de la boule avec un mur");

            // Code
            if (InventoryItemLoader.TryGetItem<InventorySystem.Items.ThrowableProjectiles.ThrowableItem>(ItemType.GrenadeHE, out var result) && UnityEngine.Object.Instantiate(result.Projectile) is TimeGrenade timeGrenade)
            {
                ExplosionUtils.ServerSpawnEffect(transform.position, ItemType.GrenadeHE);
                timeGrenade.Position = transform.position;
                timeGrenade.PreviousOwner = Attacker;
                timeGrenade.ServerFuseEnd();
                var ballexplosion=PrimitiveObjectToy.Create(transform.position);
                ballexplosion.Type = PrimitiveType.Sphere;
                ballexplosion.Color = Color.yellow;
                enabled = false;
                Destroy(gameObject);

            }
        }

        public void Start()
        {
            Logger.Info("start");
            if (Attacker.Equals(default(Footprint)))
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

}
