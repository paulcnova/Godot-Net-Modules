
namespace FLCore;

using Godot;
using Godot.Collections;

public sealed class RaycastInfo
{
	#region Properties
	
	private Dictionary dictionary;
	
	public bool IsHit => this.dictionary.ContainsKey("position");
	
	public Vector3 Position => this.dictionary.TryGetValue("position", out Variant position) ? position.AsVector3() : Vector3.Zero;
	public Vector3 Normal => this.dictionary.TryGetValue("normal", out Variant normal) ? normal.AsVector3() : Vector3.Zero;
	public int FaceIndex => this.dictionary.TryGetValue("face_index", out Variant faceIndex) ? faceIndex.AsInt32() : -1;
	public long ColliderID => this.dictionary.TryGetValue("collider_id", out Variant colliderID) ? colliderID.AsInt64() : -1;
	public Variant Collider => this.dictionary.TryGetValue("collider", out Variant collider) ? collider : default;
	public int Shape => this.dictionary.TryGetValue("shape", out Variant shape) ? shape.AsInt32() : -1;
	public Rid RID => this.dictionary.TryGetValue("rid", out Variant rid) ? rid.AsRid() : default;
	public Dictionary Dictionary => this.dictionary;
	
	public RaycastInfo(Dictionary dictionary)
	{
		this.dictionary = dictionary;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static implicit operator RaycastInfo(Dictionary dictionary) => new RaycastInfo(dictionary);
	
	#endregion // Public Methods
}
