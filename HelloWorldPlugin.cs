using System;
using System.Collections;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using UnityEngine;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Entries;
using Logger = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;
using System.Collections.Generic;

namespace HelloWorldPlugin;

internal class HelloWorldPlugin : Plugin
{
    public override string Name { get; } = "Hello World";

    public override string Description { get; } = "Simple example plugin that demonstrates showing a broadcast to players when they join. Using Custom Event Handlers.";

    public override string Author { get; } = "Northwood";

    public override Version Version { get; } = new Version(1, 0, 0, 0);

    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public MyCustomEventsHandler Events { get;  } = new();

    public override void Enable()
    {
        CustomHandlersManager.RegisterEventsHandler(Events);
        UserSettings.ServerSpecific.ServerSpecificSettingsSync.DefinedSettings = [ 
            new SSGroupHeader(1,"Sort"),
            new SSKeybindSetting(2,"boulle de feu",allowSpectatorTrigger:false)

        ];
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
    }

    private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        switch (setting.SettingId)
        {
            case 2:

                if (setting is SSKeybindSetting { SyncIsPressed: not false })
                {
                    CastFireBall(sender.transform.position, sender.PlayerCameraReference.forward,sender);
                    Logger.Info($"La boule de feu de {sender.GetNickname()} à été envoyer");
                }
                break;
        }

    }

    private void CastFireBall(Vector3 origine,Vector3 direction, ReferenceHub caster)
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
        explodeScript.Attacker = new (caster);

        //ball.GameObject.AddComponent<ExploxeCollide>();

        Timing.RunCoroutine(MoveBall(ball, direction, 1));

    }

    private IEnumerator<float> MoveBall(PrimitiveObjectToy ball, Vector3 direction, float speed)
    {
        while (ball?.GameObject != null)
        {
          
            ball.Transform.Translate(direction*speed*Time.deltaTime);
            yield return Timing.WaitForOneFrame;
        }
    }

    

    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(Events);
    }
}