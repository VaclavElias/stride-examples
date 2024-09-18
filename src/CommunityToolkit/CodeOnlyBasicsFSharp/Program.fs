open Stride.CommunityToolkit.Engine
open Stride.CommunityToolkit.Skyboxes
open Stride.CommunityToolkit.Rendering.ProceduralModels
open Stride.Core.Mathematics
open Stride.Engine
open Stride.CommunityToolkit.Rendering.Compositing
open Stride.Physics
open Stride.Games
open Stride.Input
open Stride.CommunityToolkit.Helpers
open System
open Stride.Core.Diagnostics
open Stride.Graphics
open Stride.UI.Panels
open Stride.UI.Controls
open Stride.UI
open Stride.Rendering

let mutable movementSpeed = 1.0f
let mutable force = 3.0f
let mutable cube1: Entity option = None
let mutable cube2: Entity option = None

let mutable camera: CameraComponent option = None
let mutable simulation: Simulation option = None
let mutable cube1Component: ModelComponent option = None

let mutable font: SpriteFont = null

let game = new Game()

let Start (scene: Scene) =
    game.AddGraphicsCompositor().AddCleanUIStage() |> ignore
    game.Add3DCamera().Add3DCameraController() |> ignore
    game.AddDirectionalLight() |> ignore
    game.Add3DGround() |> ignore
    game.AddProfiler() |> ignore
    game.AddSkybox() |> ignore
    game.AddGroundGizmo(Vector3(-5.0f, 0.1f, -5.0f), showAxisName = true)

    let entity = game.Create3DPrimitive(PrimitiveModelType.Capsule)
    entity.Transform.Position <- new Vector3(0f, 8f, 0f)
    entity.Scene <- scene

    // Create the first cube (no collider)
    let primitive1 = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions(
        Material = game.CreateMaterial(Color.Gold),
        IncludeCollider = false
    ))
    primitive1.Scene <- scene
    cube1 <- Some primitive1

    // Create the second cube (with collider)
    let primitive2 = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions(
        Material = game.CreateMaterial(Color.Orange)
    ))
    primitive2.Transform.Position <- Vector3(-3.0f, 5.0f, 0.0f)
    primitive2.Scene <- scene
    cube2 <- Some primitive2

    // Initialize camera, simulation, and model component for interactions
    camera <- Some (scene.GetCamera())
    simulation <- game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation |> Option.ofObj
    cube1Component <- primitive1.Get<ModelComponent>() |> Option.ofObj

    // Create and display a UI text block
    font <- game.Content.Load<SpriteFont>("StrideDefaultFont")

    let canvas = Canvas(
        Width = 300.0f,
        Height = 100.0f,
        BackgroundColor = Color(byte 248, byte 177, byte 149, byte 100),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom)

    // Add a text block to the canvas
    let textBlock = TextBlock(
        Text = "Hello, Stride!",
        TextColor = Color.White,
        Font = font,
        TextSize = 24.0f,
        Margin = Thickness(3.0f, 3.0f, 3.0f, 0.0f))

    canvas.Children.Add(textBlock)

    let uiEntity = new Entity()
    uiEntity.Add(UIComponent(
        Page = new UIPage(RootElement = canvas),
        RenderGroup = RenderGroup.Group31))

    uiEntity.Scene <- scene

// Define the Update method, called every frame to update the game state
let Update (scene: Scene) (time: GameTime) =
    game.DebugTextSystem.Print(sprintf "Entities: %d" scene.Entities.Count, Int2(50, 50))

    // Calculate the time elapsed since the last frame for consistent movement
    // This is crucial for frame-independent movement, ensuring consistent
    // behaviour regardless of frame rate.
    let deltaTime = float32 time.Elapsed.TotalSeconds

    // Handle non-physical movement for cube1
    // Move the first cube along the X-axis (non-physical movement)
    match cube1 with
    | Some cube ->
        let position = cube.Transform.Position
        let newPosition =
            if game.Input.IsKeyDown(Keys.Z) then
                Vector3(position.X - movementSpeed * deltaTime, position.Y, position.Z)
            elif game.Input.IsKeyDown(Keys.X) then
                Vector3(position.X + movementSpeed * deltaTime, position.Y, position.Z)
            else
                position
        cube.Transform.Position <- newPosition
    | None -> ()

    // Handle physics-based movement for cube2
    match cube2 with
    | Some cube ->
        let rigidBody = cube.Get<RigidbodyComponent>()
        if game.Input.IsKeyPressed(Keys.C) then
            rigidBody.ApplyImpulse(Vector3(-force, 0.0f, 0.0f))
        elif game.Input.IsKeyPressed(Keys.V) then
            rigidBody.ApplyImpulse(Vector3(force, 0.0f, 0.0f))
    | None -> ()

    if game.Input.IsKeyDown(Keys.Space) then
        let entity = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions(
            Material = game.CreateMaterial(Color.Green),
            Size = new Vector3(0.5f)
        ))
        entity.Transform.Position <- Vector3(0f, 10f, 0f)
        entity.Scene <- scene

    // Ensure camera and simulation are initialized before handling mouse input
    if camera.IsNone || simulation.IsNone || not game.Input.HasMouse then
        ()
    else
        if game.Input.IsMouseButtonDown(MouseButton.Middle) then
            let hitResult = camera.Value.RaycastMouse(simulation.Value, game.Input.MousePosition)
            if hitResult.Succeeded then
                let rigidBody = hitResult.Collider.Entity.Get<RigidbodyComponent>()
                if rigidBody <> null then
                    let direction = VectorHelper.RandomVector3([| -20.0f; 20.0f |], [| 0.0f; 20.0f |], [| -20.0f; 20.0f |])
                    rigidBody.ApplyImpulse(direction)
            // Return after handling middle mouse input

        // Handle left mouse button input
        if game.Input.IsMouseButtonPressed(MouseButton.Left) then
            let hitResult = camera.Value.RaycastMouse(simulation.Value, game.Input.MousePosition)
            if hitResult.Succeeded then
                let message = sprintf "Hit: %s" hitResult.Collider.Entity.Name
                Console.WriteLine(message)
                GlobalLogger.GetLogger("Program.fs").Info(message)

                let rigidBody = hitResult.Collider.Entity.Get<RigidbodyComponent>()
                if rigidBody <> null then
                    let direction = Vector3(0.0f, 3.0f, 0.0f) // Apply upward impulse
                    rigidBody.ApplyImpulse(direction)
            else
                Console.WriteLine("No hit detected.")

            // Check for intersections with non-physical entities using ray picking
            let ray = camera.Value.GetPickRay(game.Input.MousePosition)
            if cube1Component.IsSome && cube1Component.Value.BoundingBox.Intersects(&ray) then
                Console.WriteLine("Cube 1 hit!")

[<EntryPoint>]
// Start the game loop and provide the Start and Update methods as callbacks
// This method initializes the game, begins running the game loop,
// and starts processing events.
let main argv =
    game.Run(start = System.Action<Scene>(Start), update = System.Action<Scene, GameTime>(Update))
    0