param(
    [string]$datasource = ".",
    [string]$initialcatalog = "sensenet"
    )

.\SnWebApplication\Admin\bin\SnAdmin.exe install-services datasource:$datasource initialCatalog:$initialcatalog forcedreinstall:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-webpages importdemo:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-workspaces importdemo:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-notification importdemo:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-workflow importdemo:true
.\SnWebApplication\Admin\bin\SnAdmin.exe install-oauth-google importdemo:true