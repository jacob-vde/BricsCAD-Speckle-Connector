# BricsCAD-Speckle-Connector
Bricsys hackaton project: Speckle Connector

## Testing: 
- Replace [BCAD INSTALLATION FOLDER] with the path to your Bricscad installation folder (default: C:\Program Files\Bricsys\BricsCAD V22 en_US) in Speckle-Connector.csproj (4 times)
- Build project
- Open BricsCad
- in BCad type NETLOAD
- select previously build Speckle-Connector.dll
- You should now be able to type commands specklesend and specklereceive in BricsCAD
