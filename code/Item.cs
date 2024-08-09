using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{

	public class Item
	{
		public string Id { get; set; }
		public string DisplayName {  get; set; }
		public int StackSize { get; set;}
		public GameObject Prefab { get; set; }
		public Item()
		{
			Id = "";
			DisplayName = "unknown";
			StackSize = 0;
			Prefab = null;
		}
	}
	public class ItemStack
	{
		public Item ItemType;
		public int Count;
		public ItemStack( Item item, int count )
		{
			ItemType = item;
			Count = count;
		}
		public static ItemStack Create(string id, int count )
		{
			return new ItemStack( Game.ActiveScene.GetAllComponents<ItemRegistry>().First().Items[id], count );
		}
		public bool Stacks(ItemStack other )
		{
			return ItemType == other.ItemType;
		}
		public ItemStack Clone()
		{
			return new ItemStack( ItemType, Count );
		}
	}
	public class Recipe
	{
		public string RecipeType {  get; set; }
		public List<ItemStackRaw> Inputs {  get; set; }
		public List<ItemStackRaw> Outputs {  get; set; }
		
		public Recipe()
		{
			RecipeType = "crafting";
			Inputs = new List<ItemStackRaw>();
			Outputs = new List<ItemStackRaw>();
		}
	}
	public class ItemStackRaw
	{
		public string Id { get; set; }
		public int Count { get; set; }
		
		public ItemStackRaw()
		{
			Id = "";
			Count = 1;
		}
		public static ItemStackRaw FromStack(ItemStack from )
		{
			if ( from == null )
				return null;
			ItemStackRaw item = new ItemStackRaw();
			item.Id = from.ItemType.Id;
			item.Count = from.Count;
			return item;
		}
		public ItemStack ToStack()
		{
			return ItemStack.Create( Id, Count );
		}
	}
}
