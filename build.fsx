// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#r "packages/Suave/lib/net40/Suave.dll"
open Fake
open System
open System.IO
open FSharp.Data
open Suave
open Suave.Operators
open Suave.Web
open Microsoft.FSharp.Compiler.Interactive.Shell

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

// --------------------------------------------------------------------------------------
// For deployed run - compile as an executable
// --------------------------------------------------------------------------------------

Target "clean" (fun _ ->
  CleanDirs ["bin"]
)

Target "build" (fun _ ->
  [ "eliza.sln" ]
  |> MSBuildRelease "" "Rebuild"
  |> Log ""
)

"clean" ==> "build"

// --------------------------------------------------------------------------------------
// For local run - automatically reloads scripts
// --------------------------------------------------------------------------------------

Target "run" (fun _ ->
  let sbOut = new Text.StringBuilder()
  let sbErr = new Text.StringBuilder()

  let fsiSession =
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let argv = Array.append [|"/fake/fsi.exe"; "--quiet"; "--noninteractive" |] [||]
    FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream)

  let reportFsiError (e:exn) =
    traceError "Reloading app.fsx script failed."
    traceError (sprintf "Message: %s\nError: %s" e.Message (sbErr.ToString().Trim()))
    sbErr.Clear() |> ignore

  let reloadScript () =
    try
      traceImportant "Reloading 'app.fsx' script..." 
      fsiSession.EvalInteraction(sprintf "#load @\"%s\"" (__SOURCE_DIRECTORY__ </> "src" </> "app.fsx"))
      match fsiSession.EvalExpression("App.app") with
      | Some app -> Some(app.ReflectionValue :?> WebPart)
      | None -> failwith "Couldn't get 'app' value." 
    with e -> reportFsiError e; None

  let getLocalServerConfig port =
    { defaultConfig with
        homeFolder = Some __SOURCE_DIRECTORY__
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Debug
        bindings = [ HttpBinding.mkSimple HTTP  "127.0.0.1" port ] }

  let mutable currentApp : WebPart = Successful.OK "Loading..."

  let reloadAppServer (changedFiles: string seq) =
    reloadScript () |> Option.iter (fun app -> 
      currentApp <- app
      traceImportant "Refreshed server." )

  let port = 8899
  let app = 
    Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
    >=> Writers.setHeader "Pragma" "no-cache"
    >=> Writers.setHeader "Expires" "0"
    >=> fun ctx -> currentApp ctx
  
  let _, server = startWebServerAsync (getLocalServerConfig port) app

  // Start Suave to host it on localhost
  let sources = { BaseDirectory = __SOURCE_DIRECTORY__ </> "src"; Includes = [ "*.fs*" ]; Excludes = [] }
  reloadAppServer sources
  Async.Start(server)

  // Watch for changes & reload when server.fsx changes
  let watcher = sources |> WatchChanges (Seq.map (fun x -> x.FullPath) >> reloadAppServer)
  traceImportant "Waiting for app.fsx edits. Press any key to stop."
  System.Diagnostics.Process.Start("http://localhost:8899/") |> ignore
  System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)
)

// --------------------------------------------------------------------------------------
// Azure - deploy copies the binary to wwwroot/bin
// --------------------------------------------------------------------------------------

let newName prefix f = 
  Seq.initInfinite (sprintf "%s_%d" prefix) |> Seq.skipWhile (f >> not) |> Seq.head

Target "deploy" (fun _ ->
  // Pick a subfolder that does not exist
  let wwwroot = "../wwwroot"
  let subdir = newName "deploy" (fun sub -> not (Directory.Exists(wwwroot </> sub)))
  
  // Deploy everything into new empty folder
  let deployroot = wwwroot </> subdir
  CleanDir deployroot
  CleanDir (deployroot </> "bin")
  CleanDir (deployroot </> "web")
  CleanDir (deployroot </> "data")
  CopyRecursive "bin" (deployroot </> "bin") false |> ignore
  CopyRecursive "web" (deployroot </> "web") false |> ignore
  CopyRecursive "data" (deployroot </> "data") false |> ignore
  
  let config = File.ReadAllText("web.config").Replace("%DEPLOY_SUBDIRECTORY%", subdir)
  File.WriteAllText(wwwroot </> "web.config", config)

  // Try to delete previous folders, but ignore failures
  for dir in Directory.GetDirectories(wwwroot) do
    if Path.GetFileName(dir) <> subdir then 
      try CleanDir dir; DeleteDir dir with _ -> ()
)

"build" ==> "deploy"

RunTargetOrDefault "run"
