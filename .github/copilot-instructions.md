# WebMaestro Copilot onboarding instructions

Purpose
- WebMaestro is a Windows desktop HTTP client/workbench (similar to Postman) built with WPF and MVVM. It lets you create, save, and run HTTP requests, organize collections, manage environments/variables, and import requests from OpenAPI and WSDL.

High-level repo details
- Tech stack: C#, WPF, MVVM (CommunityToolkit.Mvvm), Fluent.Ribbon UI, OxyPlot.Wpf for charts, dialogs via MvvmDialogs.
- Target runtime: .NET10.0 for Windows (`net10.0-windows`), x64 (`win-x64`). This project builds and runs on Windows only.
- Projects
 - `src/WebMaestro/WebMaestro.csproj` � WPF application.
 - `src/WebMaestro.Tests/WebMaestro.Tests.csproj` � xUnit test project with Verify snapshots.
- Package highlights
 - `CommunityToolkit.Mvvm` (source generators for `ObservableProperty`, `RelayCommand`)
 - `Microsoft.Extensions.DependencyInjection`/`Options`
 - `Microsoft.OpenApi.Readers` (OpenAPI import)
 - `MvvmDialogs`, `Fluent.Ribbon`, `OxyPlot.Wpf`
 - `GitVersion.MsBuild` (versioning during build)

Environment and prerequisites
- OS: Windows10/11 (required for WPF).
- .NET SDK:10.0
- Git repository: recommended. `GitVersion.MsBuild` determines version from Git history; ensure the repo is cloned (not a plain folder copy) to avoid versioning anomalies.

Always do this before building
- Use explicit project paths for `dotnet` commands since there is no `.sln` in the repo root.
- Run restore for both projects:
 - `dotnet restore src/WebMaestro/WebMaestro.csproj`
 - `dotnet restore src/WebMaestro.Tests/WebMaestro.Tests.csproj`

Build, test, run
- Clean (optional but helpful when switching branches/SDKs)
 - `dotnet clean src/WebMaestro/WebMaestro.csproj`
 - `dotnet clean src/WebMaestro.Tests/WebMaestro.Tests.csproj`
- Build (Debug)
 - `dotnet build src/WebMaestro/WebMaestro.csproj -c Debug`
 - `dotnet build src/WebMaestro.Tests/WebMaestro.Tests.csproj -c Debug`
- Test
 - `dotnet test src/WebMaestro.Tests/WebMaestro.Tests.csproj -c Debug`
 - Snapshot tests use Verify. On failure you will see `.received.` files alongside `.verified.` files under `src/WebMaestro.Tests`. To approve intentional changes, replace the corresponding `.verified.` file with the `.received.` file content and re-run tests.
- Run the app
 - `dotnet run --project src/WebMaestro/WebMaestro.csproj -c Debug`
- Publish a single-file build (Release, x64)
 - `dotnet publish src/WebMaestro/WebMaestro.csproj -c Release -r win-x64 /p:PublishSingleFile=true`

Known build tips and pitfalls
- Windows-only: WPF (`UseWPF`) and `net10.0-windows` require a Windows build agent.
- `GitVersion.MsBuild`: If a build fails due to versioning or duplicate assembly attributes in certain environments:
 - Ensure the folder is a real Git clone (with history) so GitVersion can compute a version.
 - The app `csproj` contains a commented `GenerateAssemblyInfo` workaround. Do not change it unless you hit duplicate AssemblyInfo errors. If you must, uncomment `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to resolve duplicates, but prefer fixing the environment.
- No lint/format step is enforced. Default analyzers apply via SDK and packages. Honor existing code style (e.g., `partial` classes using `ObservableProperty`/`RelayCommand` attributes).

Repository layout and guidance
- App project (`src/WebMaestro`)
 - Entry point: `App.xaml`/`App.xaml.cs`; main window: `Views/MainWindow.xaml`.
 - MVVM structure:
 - `ViewModels/` � state and commands (e.g., `MainViewModel`, `WebViewModel`, `Explorer/*`). Uses `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`).
 - `Views/` � WPF XAML views and dialogs (e.g., `Views/WebView.xaml`, `Views/Dialogs/*`).
 - `Models/` � domain models (requests, responses, collections, environments).
 - `Services/` � I/O and HTTP request logic (e.g., `HttpRequestService`, `FileService`).
 - `Importers/` � `OpenApiImporter`, `WsdlImporter`.
 - `Serializers/` � e.g., `RequestModelSerializer`.
 - `Controls/`, `Converters/`, `Behaviors/`, `Resources/` � WPF helpers and assets.
 - Composition/DI: `ViewModelLocator.cs`, `Registry.cs` configure services and view-model wiring.
 - App configuration and assets: under `Properties/` and `Resources/Images/`.
- Test project (`src/WebMaestro.Tests`)
 - Uses `xunit`, `Verify.Xunit`, and `coverlet.collector`.
 - Resources for tests are in `src/WebMaestro.Tests/Resources` (HTTP samples and WSDL XMLs). Some are marked as `EmbeddedResource`, others copied to output.

What to check before submitting a PR
- Always:
 - Restore, build both projects (Debug or Release as appropriate).
 - Run `dotnet test src/WebMaestro.Tests/WebMaestro.Tests.csproj` and ensure all tests pass or update Verify snapshots when intentional.
 - If you changed UI or MVVM bindings, run the app to smoke-test startup and key views.
- If you added `ObservableProperty` or `RelayCommand` usages, ensure the containing class is `partial` so source generators can emit the backing members.
- If you introduced new resources (images, XAML), ensure they are included as `Resource` or `Page` in the `csproj` when needed.

CI and automation
- No GitHub Actions workflows are present at the time of writing. Validation is performed via local build and test steps above. If adding CI later, mirror these local steps on a Windows runner with .NET SDK10.

Non-obvious dependencies
- Windows x64 only for run/debug. Tests also target `net10.0-windows`.
- Versioning is influenced by Git history via `GitVersion.MsBuild`.

Operate with these defaults
- Prefer `dotnet` CLI with explicit project paths from the repo root.
- Assume Windows PowerShell or CMD shell.
- Trust these instructions first. Only search the codebase for build/run info if these steps are incomplete or produce errors in your environment.

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.
