# Script para limpiar, reconstruir y publicar el proyecto DotNetBcnModule.Presentation
# Asegura que IntegrationCustom.css se incluya correctamente en la publicación

Write-Host "=== Script de Publicación DotNetBcnModule ===" -ForegroundColor Green
Write-Host ""

# 1. Limpiar el proyecto
Write-Host "1. Limpiando el proyecto..." -ForegroundColor Yellow
dotnet clean DotNetBcnModule.Presentation.sln --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al limpiar el proyecto" -ForegroundColor Red
    exit 1
}

# 2. Restaurar paquetes NuGet
Write-Host "2. Restaurando paquetes NuGet..." -ForegroundColor Yellow
nuget restore DotNetBcnModule.Presentation.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al restaurar paquetes NuGet" -ForegroundColor Red
    exit 1
}

# 3. Reconstruir el proyecto en modo Release
Write-Host "3. Reconstruyendo el proyecto en modo Release..." -ForegroundColor Yellow
dotnet build DotNetBcnModule.Presentation.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al reconstruir el proyecto" -ForegroundColor Red
    exit 1
}

# 4. Verificar que IntegrationCustom.css existe
Write-Host "4. Verificando archivos CSS..." -ForegroundColor Yellow
$cssFile = "Content\IntegrationCustom.css"
if (Test-Path $cssFile) {
    Write-Host "✓ IntegrationCustom.css encontrado" -ForegroundColor Green
} else {
    Write-Host "✗ IntegrationCustom.css NO encontrado" -ForegroundColor Red
    exit 1
}

# 5. Crear directorio de publicación
$publishDir = "publish"
if (Test-Path $publishDir) {
    Write-Host "5. Limpiando directorio de publicación anterior..." -ForegroundColor Yellow
    Remove-Item $publishDir -Recurse -Force
}

# 6. Publicar el proyecto
Write-Host "6. Publicando el proyecto..." -ForegroundColor Yellow
dotnet publish DotNetBcnModule.Presentation.sln --configuration Release --output $publishDir --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al publicar el proyecto" -ForegroundColor Red
    exit 1
}

# 7. Verificar que los archivos CSS se copiaron correctamente
Write-Host "7. Verificando archivos en la publicación..." -ForegroundColor Yellow
$publishedCssFile = "$publishDir\Content\IntegrationCustom.css"
if (Test-Path $publishedCssFile) {
    Write-Host "✓ IntegrationCustom.css copiado correctamente a la publicación" -ForegroundColor Green
} else {
    Write-Host "✗ IntegrationCustom.css NO se copió a la publicación" -ForegroundColor Red
    exit 1
}

# 8. Verificar otros archivos importantes
$importantFiles = @(
    "Content\bootstrap.css",
    "Content\Site.css",
    "Scripts\jquery-3.7.0.min.js",
    "Scripts\bootstrap.min.js"
)

foreach ($file in $importantFiles) {
    $publishedFile = "$publishDir\$file"
    if (Test-Path $publishedFile) {
        Write-Host "✓ $file copiado correctamente" -ForegroundColor Green
    } else {
        Write-Host "✗ $file NO se copió" -ForegroundColor Red
    }
}

# 9. Verificar archivos de configuración
Write-Host "8. Verificando archivos de configuración..." -ForegroundColor Yellow
$configFiles = @(
    "Web.config",
    "Web.Release.config"
)

foreach ($file in $configFiles) {
    $publishedFile = "$publishDir\$file"
    if (Test-Path $publishedFile) {
        Write-Host "✓ $file copiado correctamente" -ForegroundColor Green
    } else {
        Write-Host "✗ $file NO se copió" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Publicación Completada ===" -ForegroundColor Green
Write-Host "Directorio de publicación: $publishDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para desplegar en IIS:" -ForegroundColor Yellow
Write-Host "1. Copie todo el contenido de '$publishDir' a su directorio web en IIS" -ForegroundColor White
Write-Host "2. Configure el Application Pool para .NET Framework 4.8" -ForegroundColor White
Write-Host "3. Asegúrese de que IIS tenga permisos de lectura en el directorio" -ForegroundColor White
Write-Host ""
Write-Host "Para verificar que IntegrationCustom.css se carga correctamente:" -ForegroundColor Yellow
Write-Host "- Abra las herramientas de desarrollador del navegador (F12)" -ForegroundColor White
Write-Host "- Vaya a la pestaña Network/Red" -ForegroundColor White
Write-Host "- Recargue la página y busque 'IntegrationCustom.css' en la lista" -ForegroundColor White
Write-Host "" 