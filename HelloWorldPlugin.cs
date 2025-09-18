using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using UserSettings.ServerSpecific;

namespace Magic;

internal class HelloWorldPlugin : Plugin
{
    #region Properties & Variables
    public override string Author => PluginInfo.PLUGIN_AUTHORS;

    public override string Description => PluginInfo.PLUGIN_DESCRIPTION;

    public MyCustomEventsHandler Events { get; } = new();
    public override string Name => PluginInfo.PLUGIN_NAME;

    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public override Version Version { get; } = new Version(PluginInfo.PLUGIN_VERSION);
    #endregion

    #region Methods
    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(Events);
    }

    public override void Enable()
    {
        CustomHandlersManager.RegisterEventsHandler(Events);
        UserSettings.ServerSpecific.ServerSpecificSettingsSync.DefinedSettings = [
            new SSGroupHeader(1, "Sort"),
            new SSKeybindSetting(2, "boulle de feu", allowSpectatorTrigger: false)
        ];
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
    }

    private void CastFireBall(Vector3 origine, Vector3 direction, ReferenceHub caster)
    {
        var ball = PrimitiveObjectToy.Create(origine+direction*2);
        ball.Type = PrimitiveType.Sphere;
        ball.Color = Color.red;
        //ball.MovementSmoothing = byte.MaxValue;
        ball.GameObject.layer = LayerMask.GetMask("Grenade");
        var collider = ball.GameObject.AddComponent<SphereCollider>();
        collider.radius = 1.0f;

        var rigidbody = ball.GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false; // Fireballs typically don't fall
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // For fast-moving objects


        Logger.Info($"{ball.GameObject.TryGetComponent<Collider>(out _)}");

        var explodeScript = ball.GameObject.AddComponent<ExploxeCollide>();
        explodeScript.Attacker = new(caster);

        //ball.GameObject.AddComponent<ExploxeCollide>();

        Timing.RunCoroutine(MoveBall(ball, direction, 1));

    }

    private IEnumerator<float> MoveBall(PrimitiveObjectToy ball, Vector3 direction, float speed)
    {
        while (ball?.GameObject != null)
        {

            ball.Transform.Translate(direction * speed * Time.deltaTime);
            yield return Timing.WaitForOneFrame;
        }
    }

    private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        switch (setting.SettingId)
        {
            case 2:

                if (setting is SSKeybindSetting { SyncIsPressed: not false })
                {
                    CastFireBall(sender.transform.position, sender.PlayerCameraReference.forward, sender);
                    Logger.Info($"La boule de feu de {sender.GetNickname()} à été envoyer");
                }
                break;
        }

    }
    #endregion
}