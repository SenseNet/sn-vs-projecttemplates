param(
    [string]$datasource = ".",
    [string]$initialcatalog = "sensenet"
    )
	
$error.clear()

.\SnWebApplication\Admin\bin\SnAdmin.exe install-services datasource:$datasource initialCatalog:$initialcatalog forcedreinstall:true

if ($error) {
	throw "Error during package install."
}
