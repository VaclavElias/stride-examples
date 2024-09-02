using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using Stride.Physics;

float movementSpeed = 5f;

Vector3 center = new Vector3(0, 0, 0);
float radius = 5f; // Radius of the orbit (e.g., 5 units away from the center)
float angularSpeed = 1f; // Speed of rotation (radians per second)
float angle = 0f; // Current angle in the orbit

Entity? box1 = null;
Entity? box2 = null;
Entity? box3 = null;
RigidbodyComponent? rigidBody = null;
ModelComponent? modelComponent2 = null;
ModelComponent? modelComponent3 = null;

using var game = new Game();

game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
    game.AddGraphicsCompositor();
    game.Add3DCamera().Add3DCameraController();
    game.AddDirectionalLight();
    game.Add3DGround();
    game.AddProfiler();
    game.AddSkybox();

    // This was added to see the axis directions
    game.AddGroundGizmo(position: new Vector3(-5, 0.1f, -5), showAxisName: true);

    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);
    entity.Transform.Position = new Vector3(0, 8, 0);
    entity.Scene = rootScene;

    // This was added
    // Note that we are disabling the collider for the box and
    // adding a material to it so that we can change the color of the box
    // The box is hanging in the air, so it won't collide with the ground
    box1 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.Gold),
        IncludeCollider = false
    });
    box1.Transform.Position = new Vector3(0, 0, 0);
    box1.Scene = rootScene;

    box2 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.DarkRed),
        IncludeCollider = false
    });
    box2.Transform.Position = new Vector3(0, 1, 0);
    box2.Scene = rootScene;
    modelComponent2 = box2.Get<ModelComponent>();

    box3 = game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
        Material = game.CreateMaterial(Color.DarkRed),
        IncludeCollider = false
    });
    box3.Transform.Position = new Vector3(0, 1, 0);
    box3.Scene = rootScene;
    modelComponent3 = box3.Get<ModelComponent>();

    rigidBody = box1.Get<RigidbodyComponent>();
}

void Update(Scene scene, GameTime time)
{
    if (box1 != null)
    {
        var deltaTime = (float)time.Elapsed.TotalSeconds;

        box1.Transform.Position -= new Vector3(movementSpeed * deltaTime, 0, 0);
    }

    //if (rigidBody != null)
    //{
    //    // Calculate movement speed (units per second)
    //    var moveDirection = Vector3.Zero;

    //    if (game.Input.IsKeyDown(Keys.Z))
    //    {
    //        moveDirection.X += movementSpeed;
    //    }
    //    else if (game.Input.IsKeyDown(Keys.X))
    //    {
    //        moveDirection.X -= movementSpeed;
    //    }

    //    rigidBody.LinearVelocity = moveDirection;
    //    //rigidBody.ApplyImpulse(moveDirection);
    //}
    //else

    if (box2 != null)
    {
        var deltaMovement = movementSpeed * (float)time.Elapsed.TotalSeconds;

        if (game.Input.IsKeyDown(Keys.Z))
        {
            box2.Transform.Position += new Vector3(-deltaMovement, 0, 0);
        }
        else if (game.Input.IsKeyDown(Keys.X))
        {
            box2.Transform.Position += new Vector3(deltaMovement, 0, 0);
        }
    }

    if (box3 != null)
    {
        angle += angularSpeed * (float)time.Elapsed.TotalSeconds;

        // Calculate the new position on the circle
        float x = center.X + radius * MathF.Cos(angle);
        float z = center.Z + radius * MathF.Sin(angle);

        box3.Transform.Position = new Vector3(x, box3.Transform.Position.Y, z);
    }

    //var result = CollisionHelper.DistanceBoxBox(modelComponent2.BoundingBox, modelComponent3.BoundingBox);
    //Console.WriteLine(result);
    var result2 = modelComponent3.BoundingBox.Intersects(modelComponent2.BoundingBox);
    Console.WriteLine(result2);

}