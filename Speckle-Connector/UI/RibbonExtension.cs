using Bricscad.ApplicationServices;
using Teigha.Runtime;

[assembly: ExtensionApplication(typeof(BSC.UI.SetUpUI))]

namespace BSC.UI
{
  public class SetUpUI : IExtensionApplication
  {
    public void Initialize()
    {
      if (!Application.IsMenuGroupLoaded("Speckle"))
      {
        var cuiFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
        cuiFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(cuiFile), "Speckle-BricsCAD-Connector.cui");
        Application.LoadPartialMenu(cuiFile);
      }
    }

    public void Terminate()
    {
      // 
    }
  }
}
