
namespace FLCore.SaveLoad;

using FLCore;

using Godot;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using File = System.IO.File;

/// <summary>A custom dictionary class that serializes and deserializes objects dynamically</summary>
public sealed class SaveTable : IEnumerable<KeyValuePair<string, object>>
{
	#region Properties
	
	/// <summary>The type of members that the save table will look for</summary>
	internal const MemberTypes MemberTypesToLookFor = MemberTypes.Field | MemberTypes.Property;
	/// <summary>The type of bindings that the save table will look for</summary>
	internal const BindingFlags BindingFlagsToLookFor = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	
	private Dictionary<string, object> table = new Dictionary<string, object>();
	
	/// <summary>Gets and sets the items within this save table</summary>
	public object this[string key] { get => this.table[key]; set => this.table[key] = value; }
	
	/// <summary>Gets the size of the table</summary>
	public int Count => this.table.Count;
	
	/// <summary>Gets the actual table itself</summary>
	public Dictionary<string, object> Table => this.table;
	
	/// <summary>A base constructor to create a save table</summary>
	public SaveTable() {}
	
	/// <summary>A constructor that copies over the save table into the new save table</summary>
	/// <param name="table">The save table to copy over</param>
	public SaveTable(SaveTable table)
	{
		this.table = new Dictionary<string, object>(table.table);
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Gets the type from the given string</summary>
	/// <param name="typeID">The full name of the type to create from</param>
	/// <returns>Returns the type, returns null if it cannot be found anywhere</returns>
	public static System.Type GetTypeFromString(string typeID)
	{
		System.Type type = System.Type.GetType(typeID);
		
		// The types aren't found within the same assemblies, so first look for it in the UnityEngine library
		if(type == null)
		{
			type = System.Type.GetType($"{typeID}, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		}
		// Otherwise, if nothing is found scramble to find a type within the currently executing assemblies (should work)
		if(type == null)
		{
			foreach(Assembly asm in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				type = System.Type.GetType($"{typeID}, {asm.FullName}");
				if(type != null) { break; }
			}
		}
		// If all else, the object is a lost cause, just return null
		if(type == null) { return null; }
		
		return type;
	}
	
	/// <summary>Gets the enumerator for the save table</summary>
	/// <returns>Returns the enumerator for the save table</returns>
	public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => this.table.GetEnumerator();
	
	/// <summary>Gets the enumerator for the save table</summary>
	/// <returns>Returns the enumerator for the save table</returns>
	IEnumerator IEnumerable.GetEnumerator() => this.table.GetEnumerator();
	
	/// <summary>Adds in the object with a key associated with it</summary>
	/// <param name="key">The key associated with the object, typically the type of the object along with a unique ID</param>
	/// <param name="value">The object itself</param>
	public void Add(string key, object value)
	{
		if(!table.ContainsKey(key))
		{
			this.table.Add(key, value);
		}
		else
		{
			table[key] = value;
		}
	}
	
	/// <summary>Removes the object from the save table given the associated key</summary>
	/// <param name="key">The key associated with the object, typically the type of the object along with a unique ID</param>
	public void Remove(string key) => this.table.Remove(key);
	
	/// <summary>Finds if the given key is found within the save table</summary>
	/// <param name="key">The key associated with the object, typically the type of the object along with a unique ID</param>
	/// <returns>Returns true if the given key is found within the save table</returns>
	public bool ContainsKey(string key) => this.table.ContainsKey(key);
	
	/// <summary>Clears the save table</summary>
	public void Clear() => this.table.Clear();
	
	/// <summary>Saves the table onto the given path</summary>
	/// <param name="path">The file path to save the table onto</param>
	/// <returns>Returns true if the table was successfully saved</returns>
	public bool Save(string path)
	{
		try
		{
			string json = " ";
			
			foreach(KeyValuePair<string, object> pair in this.table)
			{
				json += $@"""{pair.Key.Replace("\"", "\\\"")}"": {this.FlattenObject(pair.Value)},";
			}
			json = $"{{ {json.Substring(0, json.Length - 1)} }}";
			
			File.WriteAllText(path, json);
		}
		catch(System.Exception e)
		{
			GD.PrintErr(e.ToString());
			return false;
		}
		
		return true;
	}
	
	/// <summary>Loads the data from the given path onto the save table</summary>
	/// <param name="path">The file path to get the data from</param>
	/// <returns>Returns true if the save data has been successfully loaded</returns>
	public bool Load(string path)
	{
		if(!File.Exists(path)) { return false; }
		
		try
		{
			string json = File.ReadAllText(path);
			Dictionary<string, object> inflated = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
			
			foreach(KeyValuePair<string, object> pair in inflated)
			{
				string type = this.GetTypeFromKey(pair.Key);
				
				if(pair.Value is null)
				{
					this.Add(pair.Key, null);
					continue;
				}
				
				if(pair.Value is JObject)
				{
					JObject jObj = pair.Value as JObject;
					
					if(pair.Key.StartsWith("System.Collections.Hashtable"))
					{
						Hashtable table = jObj.ToObject<Hashtable>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
						
						this.Add(pair.Key, table);
						continue;
					}
					if(pair.Key.StartsWith("System.Collections.Generic.Dictionary"))
					{
						System.Type objType = SaveTable.GetTypeFromString(type);
						IDictionary dict = SaveTable.TryToCreateInstance(objType) as IDictionary;
						
						if(dict == null)
						{
							dict = jObj.ToObject<IDictionary>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
							
							this.Add(pair.Key, dict);
							continue;
						}
						else
						{
							IDictionary table = jObj.ToObject<IDictionary>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
							System.Type schema = typeof(object);
							bool isUpdatedSchema = false;
							
							foreach(DictionaryEntry entry in table)
							{
								if(entry.Key.ToString().StartsWith("$"))
								{
									if(entry.Key.ToString().StartsWith("$schema"))
									{
										schema = SaveTable.GetTypeFromString(entry.Value.ToString());
										isUpdatedSchema = true;
									}
									continue;
								}
								
								System.Type valueType = objType.GenericTypeArguments[1];
								
								if(valueType.IsPrimitive || valueType as System.IConvertible != null)
								{
									dict.Add(entry.Key, System.Convert.ChangeType(entry.Value, valueType));
								}
								else
								{
									if(isUpdatedSchema && entry.Value.GetType() == typeof(JObject))
									{
										dict.Add(entry.Key, this.InflateObject(entry.Value as JObject, schema.FullName));
									}
									else
									{
										dict.Add(entry.Key, entry.Value);
									}
								}
							}
							
							this.Add(pair.Key, dict);
							continue;
						}
					}
					this.Add(pair.Key, this.InflateObject(pair.Value as JObject, type));
				}
				else if(pair.Value is JArray)
				{
					int i = 0;
					JArray arr = pair.Value as JArray;
					System.Type itemType = SaveTable.GetTypeFromString(type.Replace("[]", ""));
					
					if(pair.Key.StartsWith("System.Collections.Hashtable"))
					{
						Hashtable table = arr.ToObject<Hashtable>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
						
						this.Add(pair.Key, table);
						continue;
					}
					if(pair.Key.StartsWith("System.Collections.Generic.Dictionary"))
					{
						System.Type objType = SaveTable.GetTypeFromString(type);
						IDictionary dict = SaveTable.TryToCreateInstance(objType) as IDictionary;
						
						if(dict == null)
						{
							dict = arr.ToObject<IDictionary>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
							
							this.Add(pair.Key, dict);
							continue;
						}
						else
						{
							Hashtable table = arr.ToObject<Hashtable>(new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
							
							foreach(DictionaryEntry entry in table)
							{
								System.Type valueType = objType.GenericTypeArguments[1];
								
								if(valueType.IsPrimitive || valueType as System.IConvertible != null)
								{
									dict.Add(entry.Key, System.Convert.ChangeType(entry.Value, valueType));
								}
								else
								{
									dict.Add(entry.Key, entry.Value);
								}
							}
							
							this.Add(pair.Key, dict);
							continue;
						}
					}
					
					if(pair.Key.StartsWith("System.Byte"))
					{
						Add(pair.Key, System.Convert.FromBase64String(pair.Value as string));
						continue;
					}
					
					if(itemType == null)
					{
						continue;
					}
					
					if(type.Contains("[]"))
					{
						object[] objs = new object[arr.Count];
						foreach(JToken item in arr)
						{
							if(itemType != null && (itemType.IsPrimitive || itemType == typeof(string)))
							{
								objs[i++] = item.ToObject(itemType);
							}
							else
							{
								objs[i++] = this.InflateObject(item.ToObject<JObject>(), type.Replace("[]", ""));
							}
						}
						this.Add(pair.Key, objs);
					}
					else
					{
						this.Add(pair.Key, arr.ToObject(itemType));
					}
				}
				else
				{
					switch(SaveTable.TrimFullName(type, true))
					{
						case "System.TimeSpan": this.Add(pair.Key, System.TimeSpan.FromTicks((long)pair.Value)); break;
						case "System.DateTime": this.Add(pair.Key, new System.DateTime((long)pair.Value)); break;
						case "System.Guid": this.Add(pair.Key, System.Guid.Parse(pair.Value.ToString())); break;
						case "System.Uri": this.Add(pair.Key, new System.Uri(pair.Value.ToString())); break;
						case "System.Byte[]": this.Add(pair.Key, System.Convert.FromBase64String(pair.Value as string)); break;
						default: this.Add(pair.Key, this.InflatePrimitive(pair.Value, type)); break;
					}
				}
			}
		}
		catch(System.Exception e)
		{
			GD.PrintErr(e.ToString());
			return false;
		}
		return true;
	}
	
	/// <summary>Saves the data asynchronously into the file path</summary>
	/// <param name="path">The path to save to</param>
	/// <param name="progress">Gets the progress of the saving process</param>
	/// <param name="success">Called at the end of the saving process</param>
	/// <returns>Returns a coroutine-able yield list</returns>
	public IEnumerator<double> SaveAsync(string path, System.Action<int, int> progress, System.Action success)
	{
		string json = " ";
		int size = this.table.Count;
		int index = 0;
		
		progress?.Invoke(index, size);
		yield return Timing.WaitForOneFrame;
		
		foreach(KeyValuePair<string, object> pair in this.table)
		{
			IEnumerator<double> delta = this.FlattenObjectAsync(pair.Value, content => {
				json += $@"""{pair.Key.Replace("\"", "\\\"")}"": {content},";
			});
			
			yield return delta.Current;
			while(delta.MoveNext())
			{
				yield return delta.Current;
			}
			progress?.Invoke(++index, size);
			yield return Timing.WaitForOneFrame;
		}
		json = $"{{ {json.Substring(0, json.Length - 1)} }}";
		yield return Timing.WaitForOneFrame;
		
		File.WriteAllText(path, json);
		
		yield return Timing.WaitForOneFrame;
		success?.Invoke();
	}
	
	/// <summary>Loads the data asynchronously from the file path</summary>
	/// <param name="path">The path to load form</param>
	/// <param name="progress">Gets the progress of the loading process</param>
	/// <param name="success">Called at the end of the loading process</param>
	/// <returns>Returns a coroutine-able yield list</returns>
	public IEnumerator<double> LoadAsync(string path, System.Action<int, int> progress, System.Action success)
	{
		if(!File.Exists(path))
		{
			success?.Invoke();
			yield break;
		}
		
		string json = File.ReadAllText(path);
		Dictionary<string, object> inflated = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
		int size = inflated.Count;
		int index = 0;
		
		progress?.Invoke(index, size);
		yield return Timing.WaitForOneFrame;
		
		foreach(KeyValuePair<string, object> pair in inflated)
		{
			string type = this.GetTypeFromKey(pair.Key);
			
			if(pair.Value is null)
			{
				this.Add(pair.Key, null);
				continue;
			}
			
			if(pair.Value is JObject)
			{
				JObject jObj = pair.Value as JObject;
				
				if(pair.Key.StartsWith("System.Collections.Generic.Dictionary") || pair.Key.StartsWith("System.Collections.Hashtable"))
				{
					System.Type objType = SaveTable.GetTypeFromString(type);
					object dict = jObj.ToObject(objType, new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
					
					this.Add(pair.Key, dict);
					progress?.Invoke(++index, size);
					yield return Timing.WaitForOneFrame;
					continue;
				}
				
				IEnumerator<double> delta = this.InflateObjectAsync(pair.Value as JObject, type, content => {
					this.Add(pair.Key, pair.Value);
				});
				
				yield return delta.Current;
				while(delta.MoveNext())
				{
					yield return delta.Current;
				}
			}
			else if(pair.Value is JArray)
			{
				int i = 0;
				JArray arr = pair.Value as JArray;
				System.Type itemType = SaveTable.GetTypeFromString(type.Replace("[]", ""));
				
				if(itemType == null)
				{
					continue;
				}
				
				if(type.Contains("[]"))
				{
					object[] objs = new object[arr.Count];
					foreach(JToken item in arr)
					{
						if(itemType != null && (itemType.IsPrimitive || itemType == typeof(string)))
						{
							objs[i++] = item.ToObject(itemType);
						}
						else
						{
							IEnumerator<double> delta = this.InflateObjectAsync(item.ToObject<JObject>(), type.Replace("[]", ""), content => {
								objs[i++] = content;
							});
							
							yield return delta.Current;
							while(delta.MoveNext())
							{
								yield return delta.Current;
							}
						}
					}
					this.Add(pair.Key, objs);
				}
				else
				{
					this.Add(pair.Key, arr.ToObject(itemType));
				}
			}
			else
			{
				switch(SaveTable.TrimFullName(type, true))
				{
					case "System.TimeSpan": this.Add(pair.Key, System.TimeSpan.FromTicks((long)pair.Value)); break;
					case "System.DateTime": this.Add(pair.Key, new System.DateTime((long)pair.Value)); break;
					case "System.Guid": this.Add(pair.Key, System.Guid.Parse(pair.Value.ToString())); break;
					case "System.Uri": this.Add(pair.Key, new System.Uri(pair.Value.ToString())); break;
					case "System.Byte[]": this.Add(pair.Key, System.Convert.FromBase64String(pair.Value as string)); break;
					default: this.Add(pair.Key, this.InflatePrimitive(pair.Value, type)); break;
				}
			}
			
			progress?.Invoke(++index, size);
			yield return Timing.WaitForOneFrame;
		}
		
		yield return Timing.WaitForOneFrame;
		success?.Invoke();
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Inflates the primitive data given the object and it's data type</summary>
	/// <param name="item">The general data to inflate</param>
	/// <param name="typeID">The ID of the type to create</param>
	/// <returns>Returns the inflated data</returns>
	private object InflatePrimitive(object item, string typeID)
	{
		if(item is null) { return null; }
		
		System.Type type = System.Type.GetType(typeID);
		
		return System.Convert.ChangeType(item, type);
	}
	
	/// <summary>Gets the object type from the given key</summary>
	/// <param name="keyID">The key to extract the typing from</param>
	/// <returns>Returns the object type in string form</returns>
	private string GetTypeFromKey(string keyID)
	{
		int index = keyID.IndexOf(':');
		
		return index == -1 ? keyID : keyID.Substring(0, index);
	}
	
	/// <summary>Gets the object name from the given key</summary>
	/// <param name="keyID">The key to extract the name from</param>
	/// <returns>Returns the object name</returns>
	private string GetNameFromKey(string keyID)
	{
		int index = keyID.IndexOf(':');
		
		return index == -1 ? keyID : keyID.Substring(index + 1);
	}
	
	/// <summary>Inflates an array from the json array</summary>
	/// <param name="arr">The json array to inflate</param>
	/// <param name="type">The type to instantiate into</param>
	/// <returns>Returns the inflated array</returns>
	private object InflateArray(JArray arr, System.Type type)
	{
		bool isList = type.FullName.StartsWith("System.Collections.Generic.List");
		System.Type elemType = isList ? type.GenericTypeArguments[0] : type.GetElementType();
		System.Array results = System.Array.CreateInstance(elemType, arr.Count);
		int i = 0;
		
		foreach(JToken token in arr)
		{
			if(token.Type == JTokenType.Object)
			{
				results.SetValue(this.InflateObject(token.ToObject<JObject>(), elemType.FullName), i);
			}
			else if(token.Type == JTokenType.Array)
			{
				results.SetValue(this.InflateArray(token.ToObject<JArray>(), type), i);
			}
			else
			{
				results.SetValue(this.InflatePrimitive(token.ToObject(elemType), elemType.FullName), i);
			}
			++i;
		}
		
		if(isList)
		{
			IList list = (IList)TryToCreateInstance(type);
			
			foreach(object item in results)
			{
				list.Add(item);
			}
			
			return list;
		}
		
		return results;
	}
	
	/// <summary>Tries to create an instance of the given type</summary>
	/// <param name="type">The type to create</param>
	/// <returns>Returns an instantiated object for the type</returns>
	internal static object TryToCreateInstance(System.Type type)
	{
		object obj = null;
		
		try
		{
			obj = System.Activator.CreateInstance(type);
		}
		catch
		{
			try
			{
				obj = System.Activator.CreateInstance(type, true);
			}
			catch
			{
				try
				{
					obj = FormatterServices.GetUninitializedObject(type);
				}
				catch(System.Exception e)
				{
					GD.PrintErr($"Could not create object for type: {type}\n{e}");
					return null;
				}
			}
		}
		
		return obj;
	}
	
	/// <summary>Inflates the object; creating it and injecting it with real data from the stringified data</summary>
	/// <param name="item">The json object containing the stringified data</param>
	/// <param name="typeID">The ID of the type to create from</param>
	/// <returns>Returns the inflated (created) object</returns>
	private object InflateObject(JObject item, string typeID)
	{
		if(item is null) { return null; }
		
		System.Type type = GetTypeFromString(typeID);
		
		if(type == null) { return null; }
		
		object obj = SaveTable.TryToCreateInstance(type);
		
		if(obj == null)
		{
			GD.PrintErr($"Could not create object for type: {typeID}");
			return null;	
		}
		
		foreach(KeyValuePair<string, JToken> pair in item)
		{
			if(pair.Value is null) { continue; }
			
			FieldInfo field = type.GetField(this.GetNameFromKey(pair.Key), BindingFlagsToLookFor);
			
			if(field == null)
			{
				PropertyInfo property = type.GetProperty(this.GetNameFromKey(pair.Key), BindingFlagsToLookFor);
				
				if(property == null)
				{
					continue;
				}
				
				System.Type propertyType = property.PropertyType;
				
				if(propertyType.IsPrimitive || propertyType == typeof(string))
				{
					property.SetValue(obj, pair.Value.ToObject(propertyType));
				}
				else if(pair.Value.ToString() != "")
				{
					if(propertyType == typeof(System.TimeSpan))
					{
						property.SetValue(obj, System.TimeSpan.FromTicks(pair.Value.ToObject<long>()));
					}
					else if(propertyType == typeof(System.DateTime))
					{
						property.SetValue(obj, new System.DateTime(pair.Value.ToObject<long>()));
					}
					else if(propertyType == typeof(System.Guid))
					{
						property.SetValue(obj, System.Guid.Parse(pair.Value.ToObject<string>()));
					}
					else if(propertyType == typeof(System.Uri))
					{
						property.SetValue(obj, new System.Uri(pair.Value.ToObject<string>()));
					}
					else if(propertyType == typeof(byte[]))
					{
						property.SetValue(obj, System.Convert.FromBase64String(pair.Value.ToObject<string>()));
					}
					else
					{
						if(pair.Value.Type == JTokenType.Object)
						{
							property.SetValue(obj, this.InflateObject(pair.Value.ToObject<JObject>(), SaveTable.TrimFullName(propertyType.AssemblyQualifiedName)));
						}
						else
						{
							property.SetValue(obj, this.InflateArray(pair.Value.ToObject<JArray>(), propertyType));
						}
					}
				}
				
				continue;
			}
			
			System.Type fieldType = field.FieldType;
			
			if(fieldType.IsPrimitive || fieldType == typeof(string))
			{
				field.SetValue(obj, pair.Value.ToObject(fieldType));
			}
			else if(pair.Value.ToString() != "")
			{
				if(fieldType == typeof(System.TimeSpan))
				{
					field.SetValue(obj, System.TimeSpan.FromTicks(pair.Value.ToObject<long>()));
				}
				else if(fieldType == typeof(System.DateTime))
				{
					field.SetValue(obj, new System.DateTime(pair.Value.ToObject<long>()));
				}
				else if(fieldType == typeof(System.Guid))
				{
					field.SetValue(obj, System.Guid.Parse(pair.Value.ToObject<string>()));
				}
				else if(fieldType == typeof(System.Uri))
				{
					field.SetValue(obj, new System.Uri(pair.Value.ToObject<string>()));
				}
				else if(fieldType == typeof(byte[]))
				{
					field.SetValue(obj, System.Convert.FromBase64String(pair.Value.ToObject<string>()));
				}
				else
				{
					if(pair.Value.Type == JTokenType.Object)
					{
						field.SetValue(obj, this.InflateObject(pair.Value.ToObject<JObject>(), SaveTable.TrimFullName(fieldType.AssemblyQualifiedName)));
					}
					else
					{
						field.SetValue(obj, this.InflateArray(pair.Value.ToObject<JArray>(), fieldType));
					}
				}
			}
		}
		
		return obj;
	}
	
	/// <summary>Flattens the object into a serializable json format</summary>
	/// <param name="item">The item to flatten</param>
	/// <returns>Returns the stringified object</returns>
	private string FlattenObject(object item)
	{
		if(item is null) { return "null"; }
		else if(item is byte[]) { return $@"""{System.Convert.ToBase64String(item as byte[])}"""; }
		else if(
			item is bool
			|| item is byte
			|| item is sbyte
			|| item is ushort
			|| item is short
			|| item is uint
			|| item is int
			|| item is float
			|| item is ulong
			|| item is long
			|| item is double
			|| item is decimal
		) { return item.ToString().ToLower(); }
		else if(item is string || item is System.Guid || item is System.Uri) { return $@"""{item}"""; }
		else if(item is System.TimeSpan) { return ((System.TimeSpan)item).Ticks.ToString(); }
		else if(item is System.DateTime) { return ((System.DateTime)item).Ticks.ToString(); }
		else if(item as IDictionary != null)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
			};
			IDictionary dict = item as IDictionary;
			System.Type type = typeof(object);
			
			foreach(object val in dict.Values)
			{
				type = val.GetType();
				break;
			}
			
			string json = $"\"$schema\":\"{type}\"";
			
			foreach(DictionaryEntry entry in dict)
			{
				json += $@", ""{entry.Key}"": {this.FlattenObject(entry.Value)}";
			}
			
			json = $"{{ {json} }}";
			
			return json;//JsonConvert.SerializeObject(item as IDictionary, settings);
		}
		else if(item as ICollection != null) { return this.FlattenArray(item as ICollection); }
		else
		{
			MemberInfo[] members = item.GetType().FindMembers(
				SaveTable.MemberTypesToLookFor,
				SaveTable.BindingFlagsToLookFor,
				SaveTable.FilterMembers,
				item
			);
			string json = " ";
			
			foreach(MemberInfo member in members)
			{
				if(member.MemberType == MemberTypes.Field)
				{
					FieldInfo field = member as FieldInfo;
					
					json += $@"""{SaveTable.TrimFullName(field.FieldType.AssemblyQualifiedName)}:{field.Name}"": {this.FlattenObject(field.GetValue(item))},";
				}
				else
				{
					PropertyInfo property = member as PropertyInfo;
					
					json += $@"""{SaveTable.TrimFullName(property.PropertyType.AssemblyQualifiedName)}:{property.Name}"": {this.FlattenObject(property.GetValue(item))},";
				}
			}
			return $@"{{ {json.Substring(0, json.Length - 1)} }}";
		}
	}
	
	/// <summary>Flattens the object into a serializable json format</summary>
	/// <param name="item">The item to flatten</param>
	/// <param name="completedCallback">A callback for when trying to return back the flattened object text</param>
	/// <returns>Returns the stringified object</returns>
	private IEnumerator<double> FlattenObjectAsync(object item, System.Action<string> completedCallback)
	{
		if(item is null)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback("null");
		}
		else if(item is byte[])
		{
			yield return Timing.WaitForOneFrame;
			completedCallback($@"""{System.Convert.ToBase64String(item as byte[])}""");
		}
		else if(
			item is bool
			|| item is byte
			|| item is sbyte
			|| item is ushort
			|| item is short
			|| item is uint
			|| item is int
			|| item is float
			|| item is ulong
			|| item is long
			|| item is double
			|| item is decimal
		)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback(item.ToString().ToLower());
		}
		else if(item is string || item is System.Guid || item is System.Uri)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback($@"""{item}""");
		}
		else if(item is System.TimeSpan)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback(((System.TimeSpan)item).Ticks.ToString());
		}
		else if(item is System.DateTime)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback(((System.DateTime)item).Ticks.ToString());
		}
		else if(item as IDictionary != null)
		{
			yield return Timing.WaitForOneFrame;
			completedCallback(JsonConvert.SerializeObject(item as IDictionary, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
		}
		else if(item as ICollection != null)
		{
			yield return Timing.WaitForOneFrame;
			
			IEnumerator<double> delta = this.FlattenArrayAsync(item as ICollection, json => completedCallback(json));
			
			yield return delta.Current;
			while(delta.MoveNext())
			{
				yield return delta.Current;
			}
		}
		else
		{
			MemberInfo[] members = item.GetType().FindMembers(
				SaveTable.MemberTypesToLookFor,
				SaveTable.BindingFlagsToLookFor,
				SaveTable.FilterMembers,
				item
			);
			string json = " ";
			
			foreach(MemberInfo member in members)
			{
				if(member.MemberType == MemberTypes.Field)
				{
					FieldInfo field = member as FieldInfo;
					
					yield return Timing.WaitForOneFrame;
					
					IEnumerator<double> delta = this.FlattenObjectAsync(field.GetValue(item), content => {
						json += $@"""{SaveTable.TrimFullName(field.FieldType.AssemblyQualifiedName)}:{field.Name}"": {content},";
					});
					
					yield return delta.Current;
					while(delta.MoveNext())
					{
						yield return delta.Current;
					}
				}
				else
				{
					PropertyInfo property = member as PropertyInfo;
					
					yield return Timing.WaitForOneFrame;
					IEnumerator<double> delta = this.FlattenObjectAsync(property.GetValue(item), content => {
						json += $@"""{SaveTable.TrimFullName(property.PropertyType.AssemblyQualifiedName)}:{property.Name}"": {content},";
					});
					
					yield return delta.Current;
					while(delta.MoveNext())
					{
						yield return delta.Current;
					}
				}
			}
			
			yield return Timing.WaitForOneFrame;
			completedCallback($@"{{ {json.Substring(0, json.Length - 1)} }}");
			yield return Timing.WaitForOneFrame;
		}
	}
	
	/// <summary>Finds all the relevant members within an object that can be serialized in the eyes of Unity</summary>
	/// <param name="member">The member of the object to judge</param>
	/// <param name="item">Not used, data passed on from the query</param>
	/// <returns>Returns true if the member is fit to be serialized (in terms of Unity)</returns>
	internal static bool FilterMembers(MemberInfo member, object item)
	{
		if(member.MemberType == MemberTypes.Field)
		{
			FieldInfo field = member as FieldInfo;
			
			return field.IsPublic || member.GetCustomAttribute<ExportAttribute>() != null;
		}
		if(member.MemberType == MemberTypes.Property)
		{
			PropertyInfo property = member as PropertyInfo;
			MethodInfo getter = property.GetMethod;
			MethodInfo setter = property.SetMethod;
			
			return getter != null && setter != null && getter.IsPublic && setter.IsPublic && property.GetIndexParameters().Length == 0;
		}
		return false;
	}
	
	/// <summary>Trims the type's full name</summary>
	/// <param name="fullName">The full name of the type</param>
	/// <param name="hardReplace">Set to true to trim away the assembly location within the name</param>
	/// <returns>Returns the trimmed type's full name</returns>
	internal static string TrimFullName(string fullName, bool hardReplace = false)
	{
		if(hardReplace)
		{
			return Regex.Replace(fullName, @",\s[^,]+,\sVersion=[^,]+,\sCulture=[^,]+,\sPublicKeyToken=[^\]]+", "");
		}
		return fullName;
	}
	
	/// <summary>Flattens the array into a string</summary>
	/// <param name="collection">The list of items to flatten into an array</param>
	/// <returns>Returns the stringified array</returns>
	private string FlattenArray(ICollection collection)
	{
		string json = " ";
		
		foreach(object item in collection)
		{
			json += $"{this.FlattenObject(item)},";
		}
		
		return $"[{json.Substring(0, json.Length - 1)}]";
	}
	
	/// <summary>Flattens the array into a string</summary>
	/// <param name="collection">The list of items to flatten into an array</param>
	/// <param name="completedCallback">A callback for when trying to return back the flattened object text</param>
	/// <returns>Returns the stringified array</returns>
	private IEnumerator<double> FlattenArrayAsync(ICollection collection, System.Action<string> completedCallback)
	{
		string json = " ";
		
		foreach(object item in collection)
		{
			IEnumerator<double> delta = this.FlattenObjectAsync(item, content => {
				json += $"{content},";
			});
			
			yield return delta.Current;
			while(delta.MoveNext())
			{
				yield return delta.Current;
			}
		}
		
		yield return Timing.WaitForOneFrame;
		completedCallback($"[{json.Substring(0, json.Length - 1)}]");
	}
	
	/// <summary>Inflates the object; creating it and injecting it with real data from the stringified data</summary>
	/// <param name="item">The json object containing the stringified data</param>
	/// <param name="typeID">The ID of the type to create from</param>
	/// <param name="completedCallback">A callback for when trying to return back the flattened object text</param>
	/// <returns>Returns the inflated (created) object</returns>
	private IEnumerator<double> InflateObjectAsync(JObject item, string typeID, System.Action<object> completedCallback)
	{
		System.Type type = GetTypeFromString(typeID);
		
		if(type == null) { yield break; }
		
		object obj = SaveTable.TryToCreateInstance(type);
		
		if(obj == null)
		{
			GD.PrintErr($"Could not create object for type: {typeID}");
			yield break;	
		}
		
		foreach(KeyValuePair<string, JToken> pair in item)
		{
			
			FieldInfo field = type.GetField(this.GetNameFromKey(pair.Key), BindingFlagsToLookFor);
			
			if(field == null)
			{
				PropertyInfo property = type.GetProperty(this.GetNameFromKey(pair.Key), BindingFlagsToLookFor);
				
				if(property == null)
				{
					continue;
				}
				
				System.Type propertyType = property.PropertyType;
				
				if(propertyType.IsPrimitive || propertyType == typeof(string))
				{
					yield return Timing.WaitForOneFrame;
					property.SetValue(obj, pair.Value.ToObject(propertyType));
				}
				else if(pair.Value.ToString() != "")
				{
					if(propertyType == typeof(System.TimeSpan))
					{
						yield return Timing.WaitForOneFrame;
						property.SetValue(obj, System.TimeSpan.FromTicks(pair.Value.ToObject<long>()));
					}
					else if(propertyType == typeof(System.DateTime))
					{
						yield return Timing.WaitForOneFrame;
						property.SetValue(obj, new System.DateTime(pair.Value.ToObject<long>()));
					}
					else if(propertyType == typeof(System.Guid))
					{
						yield return Timing.WaitForOneFrame;
						property.SetValue(obj, System.Guid.Parse(pair.Value.ToObject<string>()));
					}
					else if(propertyType == typeof(System.Uri))
					{
						yield return Timing.WaitForOneFrame;
						property.SetValue(obj, new System.Uri(pair.Value.ToObject<string>()));
					}
					else if(propertyType == typeof(byte[]))
					{
						yield return Timing.WaitForOneFrame;
						property.SetValue(obj, System.Convert.FromBase64String(pair.Value.ToObject<string>()));
					}
					else
					{
						if(pair.Value.Type == JTokenType.Object)
						{
							yield return Timing.WaitForOneFrame;
							
							IEnumerator<double> delta = this.InflateObjectAsync(pair.Value.ToObject<JObject>(), SaveTable.TrimFullName(propertyType.AssemblyQualifiedName), content => {
								property.SetValue(obj, content);
							});
							
							yield return delta.Current;
							while(delta.MoveNext())
							{
								yield return delta.Current;
							}
						}
						else
						{
							yield return Timing.WaitForOneFrame;
							
							IEnumerator<double> delta = this.InflateArrayAsync(pair.Value.ToObject<JArray>(), propertyType, content => {
								property.SetValue(obj, content);
							});
							
							yield return delta.Current;
							while(delta.MoveNext())
							{
								yield return delta.Current;
							}
						}
					}
				}
				
				continue;
			}
			
			System.Type fieldType = field.FieldType;
			
			if(fieldType.IsPrimitive || fieldType == typeof(string))
			{
				yield return Timing.WaitForOneFrame;
				field.SetValue(obj, pair.Value.ToObject(fieldType));
			}
			else if(pair.Value.ToString() != "")
			{
				if(fieldType == typeof(System.TimeSpan))
				{
					yield return Timing.WaitForOneFrame;
					field.SetValue(obj, System.TimeSpan.FromTicks(pair.Value.ToObject<long>()));
				}
				else if(fieldType == typeof(System.DateTime))
				{
					yield return Timing.WaitForOneFrame;
					field.SetValue(obj, new System.DateTime(pair.Value.ToObject<long>()));
				}
				else if(fieldType == typeof(System.Guid))
				{
					yield return Timing.WaitForOneFrame;
					field.SetValue(obj, System.Guid.Parse(pair.Value.ToObject<string>()));
				}
				else if(fieldType == typeof(System.Uri))
				{
					yield return Timing.WaitForOneFrame;
					field.SetValue(obj, new System.Uri(pair.Value.ToObject<string>()));
				}
				else if(fieldType == typeof(byte[]))
				{
					yield return Timing.WaitForOneFrame;
					field.SetValue(obj, System.Convert.FromBase64String(pair.Value.ToObject<string>()));
				}
				else
				{
					if(pair.Value.Type == JTokenType.Object)
					{
						yield return Timing.WaitForOneFrame;
						
						IEnumerator<double> delta = this.InflateObjectAsync(pair.Value.ToObject<JObject>(), SaveTable.TrimFullName(fieldType.AssemblyQualifiedName), content => {
							field.SetValue(obj, content);
						});
						
						yield return delta.Current;
						while(delta.MoveNext())
						{
							yield return delta.Current;
						}
					}
					else
					{
						yield return Timing.WaitForOneFrame;
						
						IEnumerator<double> delta = this.InflateArrayAsync(pair.Value.ToObject<JArray>(), fieldType, content => {
							field.SetValue(obj, content);
						});
						
						yield return delta.Current;
						while(delta.MoveNext())
						{
							yield return delta.Current;
						}
					}
				}
			}
		}
		
		yield return Timing.WaitForOneFrame;
		completedCallback(obj);
		yield return Timing.WaitForOneFrame;
	}
	
	/// <summary>Inflates an array from the json array</summary>
	/// <param name="arr">The json array to inflate</param>
	/// <param name="type">The type to instantiate into</param>
	/// <param name="completedCallback">A callback for when trying to return back the flattened object text</param>
	/// <returns>Returns the inflated array</returns>
	private IEnumerator<double> InflateArrayAsync(JArray arr, System.Type type, System.Action<object> completedCallback)
	{
		bool isList = type.FullName.StartsWith("System.Collections.Generic.List");
		System.Type elemType = isList ? type.GenericTypeArguments[0] : type.GetElementType();
		System.Array results = System.Array.CreateInstance(elemType, arr.Count);
		int i = 0;
		
		foreach(JToken token in arr)
		{
			if(token.Type == JTokenType.Object)
			{
				IEnumerator<double> delta = this.InflateObjectAsync(token.ToObject<JObject>(), elemType.FullName, content => {
					results.SetValue(content, i);
				});
				
				yield return delta.Current;
				while(delta.MoveNext())
				{
					yield return delta.Current;
				}
			}
			else if(token.Type == JTokenType.Array)
			{
				IEnumerator<double> delta = this.InflateArrayAsync(token.ToObject<JArray>(), type, content => {
					results.SetValue(content, i);
				});
				
				yield return delta.Current;
				while(delta.MoveNext())
				{
					yield return delta.Current;
				}
			}
			else
			{
				yield return Timing.WaitForOneFrame;
				results.SetValue(this.InflatePrimitive(token.ToObject(elemType), elemType.FullName), i);
			}
			++i;
		}
		
		if(isList)
		{
			IList list = (IList)TryToCreateInstance(type);
			
			foreach(object item in results)
			{
				yield return Timing.WaitForOneFrame;
				list.Add(item);
			}
			
			completedCallback(list);
			yield break;
		}
		
		yield return Timing.WaitForOneFrame;
		completedCallback(results);
		yield return Timing.WaitForOneFrame;
	}
	
	#endregion // Private Methods
}
