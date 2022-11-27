# Ignite 3 Client GUI

* Connects to Ignite 3 cluster, shows a list of tables, performs SQL queries.
* Built with Avalonia.
* Cross-platform: Windows, Linux, macOS.
* Compiles to native code with Native AOT.

## Build

* `dotnet publish -r linux-x64 -c Release -o publish`

## TBD

* Internal schemas tab.
* Admin tab (uses REST API).
* Tables as tree view with columns.
* Generate POCOs from table schema.
