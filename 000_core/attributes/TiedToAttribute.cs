
namespace FLCore;

public sealed class TiedToAttribute : System.Attribute
{
	#region Properties
	
	public System.Type LinkedType { get; private set; }
	
	public TiedToAttribute(System.Type linkedType)
	{
		this.LinkedType = linkedType;
	}
	
	#endregion // Properties
}
