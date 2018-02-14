param(
    [string]$datasource = ".",
    [string]$initialcatalog = "sensenet"
    )

.\SnWebApplication\Admin\bin\SnAdmin.exe install-services datasource:$datasource initialCatalog:$initialcatalog forcedreinstall:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-webpages importdemo:true