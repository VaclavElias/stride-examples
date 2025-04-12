using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Engine;
using System.Numerics;

// Create an instance of the game
using var game = new Game();

// Start the game loop
// This method initializes the game, begins running the game loop,
// and starts processing events.
game.Run(start: Start);

// Define the Start method to set up the scene
// Define the Start method to set up the scene
void Start(Scene scene)
{
    // Add the default graphics compositor to handle rendering
    game.AddGraphicsCompositor();

    // Add a 3D camera and a controller for basic camera movement
    game.Add3DCamera().Add3DCameraController();

    // Add a 3D ground plane to catch the capsule
    game.Add3DGround(); // This was added

    // Create a 3D primitive capsule and store it in an entity
    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);

    // Reposition the capsule 8 units above the origin in the scene
    entity.Transform.Position = new Vector3(0, 8, 0);

    // Add the entity to the root scene so it becomes part of the scene graph
    entity.Scene = scene;
}