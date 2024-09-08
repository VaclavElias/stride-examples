using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Helpers;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

float movementSpeed = 1f;
float force = 3f;
Entity? cube1 = null;
Entity? cube2 = null;

CameraComponent? camera = null;
Simulation? simulation = null;
ModelComponent? cube1Component = null;

SpriteFont? font = null;

// Create an instance of the game
using var game = new Game();

// Start the game loop and provide the Start and Update methods as callbacks
// This method initializes the game, begins running the game loop,
// and starts processing events.
game.Run(start: Start, update: Update);

// Define the Start method to set up the scene
void Start(Scene rootScene)
{
    // Add the default graphics compositor to handle rendering and UI stages
    game.AddGraphicsCompositor().AddCleanUIStage();

    // Add a 3D camera and a controller for basic camera movement
    game.Add3DCamera().Add3DCameraController();

    // Add a directional light to illuminate the scene
    game.AddDirectionalLight();

    // Add a 3D ground plane to catch the capsule
    game.Add3DGround();

    // Add a performance profiler to monitor FPS and other metrics
    game.AddProfiler();

    // Add a skybox to enhance the scene's visuals
    game.AddSkybox();

    // Add a ground gizmo to visualize axis directions
    game.AddGroundGizmo(position: new Vector3(-5, 0.1f, -5), showAxisName: true);

    // Create a 3D primitive capsule and store it in an entity
    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);

    // Reposition the capsule 8 units above the origin in the scene
    entity.Transform.Position = new Vector3(0, 8, 0);

    // Add the entity to the root scene so it becomes part of the scene graph
    entity.Scene = rootScene;

    // Create a cube without a collider and add it to the scene (non-physical movement)
    cube1 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.Gold),
        IncludeCollider = false // No collider for simple movement
    });
    cube1.Scene = rootScene;

    // Create a second cube with a collider for physics-based interaction
    cube2 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.Orange)
    });
    cube2.Transform.Position = new Vector3(-3, 5, 0); // Reposition the cube above the ground
    cube2.Scene = rootScene;

    // Initialize camera, simulation, and model component for interactions
    camera = rootScene.GetCamera();
    simulation = game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;
    cube1Component = cube1.Get<ModelComponent>();

    // Create and display a UI text block
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

// Define the Update method, called every frame to update the game state
void Update(Scene scene, GameTime time)
{
    game.DebugTextSystem.Print($"Entities: {scene.Entities.Count}", new Int2(50, 50));

    // Calculate the time elapsed since the last frame for consistent movemen
    var deltaTime = (float)time.Elapsed.TotalSeconds;

    // Handle non-physical movement for cube1
    // Move the first cube along the X-axis (non-physical movement)
    if (cube1 != null)
    {
        // Move the first cube along the negative X-axis when the Z key is held down
        if (game.Input.IsKeyDown(Keys.Z))
        {
            cube1.Transform.Position -= new Vector3(movementSpeed * deltaTime, 0, 0);
        }
        // Move the first cube along the positive X-axis when the X key is held down
        else if (game.Input.IsKeyDown(Keys.X))
        {
            cube1.Transform.Position += new Vector3(movementSpeed * deltaTime, 0, 0);
        }
    }

    // Handle physics-based movement for cube2
    if (cube2 != null)
    {
        // Retrieve the RigidbodyComponent, which handles physics interactions
        var rigidBody = cube2.Get<RigidbodyComponent>();

        // We use KeyPressed instead of KeyDown to apply impulses only once per key press.
        // This means the player needs to press and release the key to apply an impulse,
        // preventing multiple impulses from being applied while the key is held down.

        // Apply an impulse to the left when the C key is pressed (and released)
        if (game.Input.IsKeyPressed(Keys.C))
        {
            rigidBody.ApplyImpulse(new Vector3(-force, 0, 0));
        }
        // Apply an impulse to the right when the V key is pressed (and released)
        else if (game.Input.IsKeyPressed(Keys.V))
        {
            rigidBody.ApplyImpulse(new Vector3(force, 0, 0));
        }
    }

    if (game.Input.IsKeyDown(Keys.Space))
    {
        var entity = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
        {
            Material = game.CreateMaterial(Color.Green),
            Size = new Vector3(0.5f, 0.5f, 0.5f),
        });

        entity.Transform.Position = new Vector3(0, 10, 0);
        entity.Scene = scene;
    }

    // Ensure camera and simulation are initialized before handling mouse input
    if (camera == null || simulation == null || !game.Input.HasMouse) return;

    // Handle mouse input for interactions
    if (game.Input.IsMouseButtonDown(MouseButton.Middle))
    {
        // Check for collisions with physics-based entities using raycasting
        var hitResult = camera.RaycastMouse(simulation, game.Input.MousePosition);

        if (hitResult.Succeeded)
        {
            var rigidBody = hitResult.Collider.Entity.Get<RigidbodyComponent>();

            if (rigidBody != null)
            {
                var direction = VectorHelper.RandomVector3([-20, 20], [0, 20], [-20, 20]);

                rigidBody.ApplyImpulse(direction);
            }
        }
    }

    // Handle mouse input for interactions
    if (game.Input.IsMouseButtonPressed(MouseButton.Left))
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
