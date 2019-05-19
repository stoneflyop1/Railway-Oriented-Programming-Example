// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open FsRopExample
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting

module Program =
    let exitCode = 0
    let CreateWebHostBuilder args = WebHost.CreateDefaultBuilder(args).UseStartup<Startup>()
    
    [<EntryPoint>]
    let main args =
        CreateWebHostBuilder(args).Build().Run()
        exitCode