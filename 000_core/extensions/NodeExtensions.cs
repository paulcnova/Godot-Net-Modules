
namespace FLCore;

using Godot;
using Godot.Collections;

using System.Collections.Generic;

internal static class Extension_Node
{
	#region Public Methods
	
	public static void QueueFreeChildren(this Node node)
	{
		Array<Node> children = node.GetChildren();
		
		foreach(Node child in children)
		{
			node.RemoveChild(child);
			child.QueueFree();
		}
	}
	
	public static PlaneBasis FlattenBasisToPlane(this Node3D node, Vector3 planeUp)
	{
		Vector3 up = planeUp.Normalized();
		Vector3 right = up.Cross(node.GlobalBasis.Z).Normalized();
		Vector3 forward = -right.Cross(up).Normalized();
		
		return new PlaneBasis(forward, right, up);
	}
	
	public static Array<T> GetAllChildrenOf<[MustBeVariant] T>(this Node node) where T : Node
	{
		Array<T> result = new Array<T>();
		Queue<Node> queue = new Queue<Node>();
		
		queue.Enqueue(node);
		
		while(queue.Count > 0)
		{
			Node temp = queue.Dequeue();
			
			if(temp is T)
			{
				result.Add(temp as T);
			}
			
			foreach(Node child in temp.GetChildren())
			{
				queue.Enqueue(child);
			}
		}
		
		return result;
	}
	
	public static T GetChildOfType<T>(this Node node) where T : Node
	{
		Queue<Node> queue = new Queue<Node>();
		
		queue.Enqueue(node);
		
		while(queue.Count > 0)
		{
			Node temp = queue.Dequeue();
			
			if(temp == null) { break; }
			
			if(temp is T result)
			{
				return result;
			}
			
			foreach(Node child in temp.GetChildren())
			{
				queue.Enqueue(child);
			}
		}
		
		return null;
	}
	
	public static void SetForwardAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			node.Basis = Basis.LookingAt(forward, Vector3.Up);
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, -0.5f * Mathf.Pi);
				node.Basis = basis;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, 0.5f * Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	public static void SetRightAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		float rightAngle = Mathf.Abs(forward.Dot(Vector3.Right));
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			Basis basis = Basis.LookingAt(forward, Vector3.Up);
			Vector3 right = forward.Cross(Vector3.Up).Normalized();
			Vector3 up = forward.Cross(right);
			
			basis = basis.Rotated(up, 0.5f * Mathf.Pi);
			basis = basis.Rotated(forward, -0.5f * Mathf.Pi);
			node.Basis = basis;
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Back, 0.5f * Mathf.Pi);
				node.Basis = basis;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Back, -0.5f * Mathf.Pi);
				basis = basis.Rotated(forward, Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	public static void SetUpAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			Basis basis = Basis.LookingAt(forward, Vector3.Up);
			Vector3 right = forward.Cross(Vector3.Up).Normalized();
			
			basis = basis.Rotated(right, 0.5f * Mathf.Pi);
			node.Basis = basis;
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				node.Basis = Basis.Identity;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	public static void SetActive(this CanvasItem item, bool isActive)
	{
		item.Visible = isActive;
		item.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	public static void SetEnabled(this CanvasItem item, bool isEnabled)
	{
		item.ProcessMode = isEnabled ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	public static void SetActive(this Node3D node, bool isActive)
	{
		node.Visible = isActive;
		node.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	public static void SetEnabled(this Node3D node, bool isEnabled)
	{
		node.ProcessMode = isEnabled ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	public static void SetActive(this Node2D node, bool isActive)
	{
		node.Visible = isActive;
		node.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	public static void SetEnabled(this Node2D node, bool isEnabled)
	{
		node.ProcessMode = isEnabled ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	#endregion // Public Methods
}
