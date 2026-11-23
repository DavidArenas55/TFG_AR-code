$ErrorActionPreference = 'Stop'
$repository = 'https://github.com/dronsEETAC/MoreTelloLink.git'
$revision = '75eebfbc0ee1b8fc1f055fec0bc3f80b7ebd18df'
$destination = Join-Path $PSScriptRoot 'MoreTelloLink'

if (-not (Test-Path -LiteralPath $destination)) {
    git clone --no-checkout -- $repository $destination
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo clonar MoreTelloLink.' }

    git -C $destination checkout --detach $revision
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo seleccionar la revisión de MoreTelloLink.' }
}

$currentRevision = git -C $destination rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $currentRevision -ne $revision) {
    throw "La copia de MoreTelloLink debe estar en la revisión $revision."
}
