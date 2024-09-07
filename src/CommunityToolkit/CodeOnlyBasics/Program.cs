using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics; // This was added
using Stride.Input;
using Stride.Physics;
using Stride.Rendering; // This was added
using Stride.UI; // This was added
using Stride.UI.Controls; // This was added
using Stride.UI.Panels; // This was added

float movementSpeed = 1f;
float force = 3f;
Entity? cube1 = null;
Entity? cube2 = null;

CameraComponent? camera = null;
Simulation? simulation = null;
ModelComponent? cube1Component = null;

SpriteFont? font = null; // This was added

using var game = new Game();

game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
    game.AddGraphicsCompositor().AddCleanUIStage(); // This was updated
    game.Add3DCamera().Add3DCameraController();
    game.AddDirectionalLight();
    game.Add3DGround();
    game.AddProfiler();
    game.AddSkybox();
    game.AddGroundGizmo(position: new Vector3(-5, 0.1f, -5), showAxisName: true);

    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);
    entity.Transform.Position = new Vector3(0, 8, 0);
    entity.Scene = rootScene;

    cube1 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.Gold),
        IncludeCollider = false // No collider for simple movement
    });
    cube1.Scene = rootScene;

    cube2 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.Orange)
    });
    cube2.Transform.Position = new Vector3(-3, 5, 0);
    cube2.Scene = rootScene;

    camera = rootScene.GetCamera();
    simulation = game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;
    cube1Component = cube1.Get<ModelComponent>();

    // This below was added: Create and display a UI text block
    font = game.Content.Load<SpriteFont>("StrideDefaultFont");
    var canvas = new Canvas
    {
        Width = 300,
        Height = 100,
        BackgroundColor = new Color(248, 177, 149, 100),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
    };

    canvas.Children.Add(new TextBlock
    {
        Text = "Hello, Stride!",
        TextColor = Color.White,
        Font = font,
        TextSize = 24,
        Margin = new Thickness(3, 3, 3, 0),
    });

    var uiEntity = new Entity
    {
        new UIComponent
        {
            Page = new UIPage { RootElement = canvas },
            RenderGroup = RenderGroup.Group31
        }
    };

    uiEntity.Scene = rootScene;
}

void Update(Scene scene, GameTime time)
{
    game.DebugTextSystem.Print("Some text", new Int2(50, 50));

    var deltaTime = (float)time.Elapsed.TotalSeconds;

    // Handle non-physical movement for cube1
    if (cube1 != null)
    {
        if (game.Input.IsKeyDown(Keys.Z))
        {
            cube1.Transform.Position -= new Vector3(movementSpeed * deltaTime, 0, 0);
        }
        else if (game.Input.IsKeyDown(Keys.X))
        {
            cube1.Transform.Position += new Vector3(movementSpeed * deltaTime, 0, 0);
        }
    }

    // Handle physics-based movement for cube2
    if (cube2 != null)
    {
        var rigidBody = cube2.Get<RigidbodyComponent>();

        if (game.Input.IsKeyPressed(Keys.C))
        {
            rigidBody.ApplyImpulse(new Vector3(-force, 0, 0));
        }
        else if (game.Input.IsKeyPressed(Keys.V))
        {
            rigidBody.ApplyImpulse(new Vector3(force, 0, 0));
        }
    }

    if (camera == null || simulation == null) return;

    if (game.Input.HasMouse && game.Input.IsMouseButtonPressed(MouseButton.Left))
    {
        // Check for collisions with physics-based entities using raycasting
        var hitResult = camera.RaycastMouse(simulation, game.Input.MousePosition);

        if (hitResult.Succeeded)
        {
            var message = $"Hit: {hitResult.Collider.Entity.Name}";
            Console.WriteLine(message);
            GlobalLogger.GetLogger("Program.cs").Info(message);

            var rigidBody = hitResult.Collider.Entity.Get<RigidbodyComponent>();

            if (rigidBody != null)
            {
                var direction = new Vector3(0, 3, 0); // Apply impulse upward

                rigidBody.ApplyImpulse(direction);
            }
        }
        else
        {
            Console.WriteLine("No hit detected.");
        }

        // Check for intersections with non-physical entities using ray picking
        var ray = camera.GetPickRay(game.Input.MousePosition);

        if (cube1Component?.BoundingBox.Intersects(ref ray) ?? false)
        {
            Console.WriteLine("Cube 1 hit!");
        }
    }
}
