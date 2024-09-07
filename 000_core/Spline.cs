
using FLCore;

using Godot;

using System.Collections;
using System.Collections.Generic;

public partial class Spline : GodotObject, IList<Vector3>
{
	#region Properties
	
	private float time;
	private float duration;
	
	public bool IsReadOnly => false;
	public int Count => this.Points.Count;
	public Type InterpolationType { get; set; } = Type.Linear;
	public Loop LoopType { get; set; } = Loop.None;
	public List<Vector3> Points { get; set; } = new List<Vector3>();
	public bool IsFinished => this.time >= this.duration;
	
	public float Duration
	{
		get => this.duration;
		set => this.duration = Mathf.Abs(value);
	}
	
	public float Time
	{
		get
		{
			float t = this.time / this.duration;
			
			if(this.IsBackwards)
			{
				t = 1.0f - t;
			}
			
			return t;
		}
		set
		{
			this.time = Mathf.Clamp(value, 0.0f, 1.0f) * this.duration;
		}
	}
	
	public Vector3 this[int index]
	{
		get
		{
			if(index < 0 || index >= this.Count)
			{
				GDX.PrintError("Index out of array");
			}
			return this.Points[index];
		}
		set
		{
			if(index < 0 || index >= this.Count)
			{
				GDX.PrintError("Index out of array");
			}
			this.Points[index] = value;
		}
	}
	
	public Vector3 Value
	{
		get
		{
			if(this.Count == 0) { return Vector3.Zero; }
			if(this.Count == 1) { return this.Points[0]; }
			
			switch(this.InterpolationType)
			{
				default: case Type.Linear: return this.GetValueLinearly(this.Time);
				case Type.CatmullRom: return this.GetValueByCatmullRom(this.Time);
			}
		}
	}
	
	private bool IsBackwards => (int)this.LoopType % 2 == 1;
	private bool IsFullLooped => (int)this.LoopType / 2 == 1;
	
	public Spline() : this(1.0f, new List<Vector3>()) {}
	
	public Spline(float duration, IList<Vector3> points)
	{
		this.time = 0.0f;
		this.duration = duration;
		this.Points = new List<Vector3>(points);
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static Spline WithUnitDuration(float unitDuration, IList<Vector3> points)
	{
		if(points.Count <= 1) { return new Spline(0.0f, points); }
		if(points.Count == 2) { return new Spline(unitDuration, points); }
		
		Vector3 diff = points[1] - points[0];
		float sum = 1.0f;
		float max = diff.LengthSquared();
		float delta = 0.0f;
		
		for(int i = 1; i < points.Count - 1; ++i)
		{
			diff = points[i + 1] - points[i];
			delta = diff.LengthSquared();
			sum += delta / max;
		}
		
		return new Spline(sum * unitDuration, points);
	}
	
	public void Add(Vector3 item) => this.Points.Add(item);
	public bool Remove(Vector3 item) => this.Points.Remove(item);
	public void Clear() => this.Points.Clear();
	public bool Contains(Vector3 item) => this.Points.Contains(item);
	public int IndexOf(Vector3 item) => this.Points.IndexOf(item);
	public void Insert(int index, Vector3 item) => this.Points.Insert(index, item);
	public void RemoveAt(int index) => this.Points.RemoveAt(index);
	public void CopyTo(Vector3[] array, int arrayIndex) => this.Points.CopyTo(array, arrayIndex);
	public IEnumerator<Vector3> GetEnumerator() => this.Points.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => this.Points.GetEnumerator();
	
	public Vector3 GetPointAt(float time)
	{
		switch(this.InterpolationType)
		{
			default: case Type.Linear: return this.GetValueLinearly(time);
			case Type.CatmullRom: return this.GetValueByCatmullRom(time);
		}
	}
	
	public void Process(float delta)
	{
		if((int)this.LoopType < 2)
		{
			this.time = Mathf.Clamp(this.time + delta, 0.0f, this.duration);
		}
		else
		{
			if(this.time + delta > this.duration)
			{
				if(this.LoopType == Loop.Yoyo)
				{
					this.LoopType = Loop.YoyoBackwards;
				}
				else if(this.LoopType == Loop.YoyoBackwards)
				{
					this.LoopType = Loop.Yoyo;
				}
			}
			this.time = (this.time + delta) % this.duration;
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private Vector3 GetValueLinearly(float time)
	{
		if(this.Count == 0) { return Vector3.Zero; }
		else if(this.Count == 1) { return this.Points[0]; }
		
		float t = time * (this.IsFullLooped
			? this.Count
			: this.Count - 1
		);
		int index = (int)t;
		
		t = Mathf.Clamp(t - index, 0.0f, 1.0f);
		
		if(this.IsFullLooped)
		{
			if(index == this.Count)
			{
				index = 0;
			}
		}
		else if(index >= this.Count - 1)
		{
			return this.Points[this.Count - 1];
		}
		
		return this.Points[index].Lerp(this.Points[index == this.Count - 1 ? 0 : index + 1], t);
	}
	
	private Vector3 GetValueByCatmullRom(float time)
	{
		if(this.Count == 0) { return Vector3.Zero; }
		else if(this.Count == 1) { return this.Points[0]; }
		
		float segments = this.IsFullLooped ? this.Count : this.Count - 1;
		int index = (int)(this.Time * segments);
		int p0 = this.GetLimits(index - 1);
		int p1 = this.GetLimits(index);
		int p2 = this.GetLimits(index + 1);
		int p3 = this.GetLimits(index + 2);
		float t = (time - (float)index / segments) * segments;
		float t2 = t * t;
		float t3 = t2 * t;
		Vector3 temp = this.Points[p0] * 0.5f * (-t3 + 2.0f * t2 - t);
		Vector3 temp2 = this.Points[p1] * 0.5f * (3.0f * t3 - 5.0f * t2 + 2.0f);
		
		temp += temp2;
		
		return 0.5f * (
			this.Points[p0] * (-t3 + 2.0f * t2 - t)
			+ this.Points[p1] * (3.0f * t3 - 5.0f * t2 + 2.0f)
			+ this.Points[p2] * (-3.0f * t3 + 4.0f * t2 + t)
			+ this.Points[p3] * (t3 - t2)
		);
	}
	
	private int GetLimits(int index)
	{
		if(this.IsFullLooped) { return index % (this.Count - 1); }
		
		if(index < 0) { return 0; }
		else if(index >= this.Count) { return this.Count - 1; }
		
		return index;
	}
	
	#endregion // Private Methods
	
	#region Types
	
	public enum Type
	{
		Linear,
		CatmullRom,
	}
	
	public enum Loop : int
	{
		None = 0,
		NoneBackwards = 1,
		Full = 2,
		FullBackwards = 3,
		Yoyo = 4,
		YoyoBackwards = 5,
	}
	
	#endregion // Types
}
