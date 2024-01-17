# r2modman profile to test with
$Profile = "Dev"
$ProfilePath = "$env:APPDATA\r2modmanPlus-local\LethalCompany\profiles\$Profile"

# Start game
& 'C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company.exe' `
    --doorstop-enable true `
    --doorstop-target "$ProfilePath\BepInEx\core\BepInEx.Preloader.dll"
