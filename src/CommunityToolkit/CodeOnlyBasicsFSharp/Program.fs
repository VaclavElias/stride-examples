open Stride.CommunityToolkit.Engine
open Stride.CommunityToolkit.Skyboxes
open Stride.CommunityToolkit.Rendering.ProceduralModels
open Stride.Core.Mathematics
open Stride.Engine
open Stride.CommunityToolkit.Rendering.Compositing
open Stride.Physics
open Stride.Games
open Stride.Input

let mutable movementSpeed = 1.0f
let mutable force = 3.0f
let mutable cube1: Entity option = None
let mutable cube2: Entity option = None

let mutable camera: CameraComponent option = None
let mutable simulation: Simulation option = None
let mutable cube1Component: ModelComponent option = None

let game = new Game()

let Start (rootScene: Scene) =
    game.AddGraphicsCompositor().AddCleanUIStage() |> ignore
    game.Add3DCamera().Add3DCameraController() |> ignore
    game.AddDirectionalLight() |> ignore
    game.Add3DGround() |> ignore
    game.AddProfiler() |> ignore
    game.AddSkybox() |> ignore
    game.AddGroundGizmo(Vector3(-5.0f, 0.1f, -5.0f), showAxisName = true)

    let entity = game.Create3DPrimitive(PrimitiveModelType.Capsule)
    entity.Transform.Position <- new Vector3(0f, 8f, 0f)
    entity.Scene <- rootScene

    // Create the first cube (no collider)
    let primitive1 = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions(
        Material = game.CreateMaterial(Color.Gold),
        IncludeCollider = false
    ))
    primitive1.Scene <- rootScene
    cube1 <- Some primitive1 // Assign to the mutable variable

    // Create the second cube (with collider)
    let primitive2 = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions(
        Material = game.CreateMaterial(Color.Orange)
    ))
    primitive2.Transform.Position <- Vector3(-3.0f, 5.0f, 0.0f)
    primitive2.Scene <- rootScene
    cube2 <- Some primitive2 // Assign to the mutable variable

    // Initialize camera, simulation, and model component for interactions
    camera <- Some (rootScene.GetCamera())
    simulation <- game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation |> Option.ofObj
    cube1Component <- primitive1.Get<ModelComponent>() |> Option.ofObj

let Update (scene: Scene) (time: GameTime) =
    game.DebugTextSystem.Print(sprintf "Entities: %d" scene.Entities.Count, Int2(50, 50))

    // Calculate the time elapsed since the last frame for consistent movement
    let deltaTime = float32 time.Elapsed.TotalSeconds

    // Handle non-physical movement for cube1
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

[<EntryPoint>]
let main argv =
    game.Run(start = System.Action<Scene>(Start), update = System.Action<Scene, GameTime>(Update))
    0