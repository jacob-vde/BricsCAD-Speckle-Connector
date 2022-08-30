using System.Collections.Generic;
using System.Linq;
using Bricscad.ApplicationServices;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace BricsCADConverter
{
  public class Converter: ISpeckleConverter
  {
    public string Description => "BricsCAD Converter";
    public string Name => "BricsCAD Converter";
    public string Author => "The BricsCAD Peeps)";
    public string WebsiteOrEmail { get; }
    
    public ProgressReport Report { get; }
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.Create;
    
    public Document Doc { get; set; }
    public Base ConvertToSpeckle(object @object)
    {
      throw new System.NotImplementedException();
    }

    public List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();

    public bool CanConvertToSpeckle(object @object)
    {
      //TODO: Update as you add conversions
      Doc.Editor.WriteMessage("Checking if we can convert to Speckle: " + @object);
      return false;
    }

    public object ConvertToNative(Base @object)
    {
      throw new System.NotImplementedException();
    }

    public List<object> ConvertToNative(List<Base> objects) => objects.Select(ConvertToNative).ToList();

    public bool CanConvertToNative(Base @object)
    {
      Doc.Editor.WriteMessage("\n    Checking if we can convert to Native: " + @object);
      return false;
    }

    public IEnumerable<string> GetServicedApplications() => new[] { "BricsCAD" };
    

    public void SetContextDocument(object doc)
    {
      // TODO: Here's were you pass in the BricsCAD document.
      if (doc is not Document document)
        throw new System.Exception("Input 'Doc'  must be a BricsCAD document");
      
      Doc = document;
    }

    public void SetContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      // TODO: This is to enable updating behaviour
    }

    public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      // TODO: This is to enable updating behaviour
    }

    public void SetConverterSettings(object settings)
    {
      // TODO: Not mandatory.
    }
  }
}