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
	public class Inventory
	{
		public ItemStack[] Items;
		public Inventory( int size )
		{
			Items = new ItemStack[size];
		}
		public ItemStack AddItem( ItemStack item )
		{
			if ( item == null )
				return null;
			ItemStack itemsLeft = item.Clone();
			for ( int i = 0; i < Items.Length; i++ )
			{
				if ( GetAt( i ) == null )
				{
					Items[i] = item;
					return null;
				}
				else
				{
					if ( Items[i].Stacks( itemsLeft ) )
					{
						int removeCount = Math.Min( itemsLeft.Count, Items[i].ItemType.StackSize - Items[i].Count );
						itemsLeft.Count -= removeCount;
						Items[i].Count += removeCount;
						if ( itemsLeft.Count <= 0 )
							return null;
					}
				}
			}
			return itemsLeft;
		}
		public int CountItems(ItemStack item )
		{
			int count = 0;
			for ( int i = 0; i < Items.Length; i++ )
			{
				ItemStack foundItem = Items[i];
				if(foundItem != null && foundItem.Stacks( item ) )
				{
					count += foundItem.Count;
				}
			}
			return count;
		}
		public ItemStack RemoveItem(ItemStack item )
		{
			if ( item == null ) return null;
			ItemStack itemsLeft = item.Clone();
			for ( int i = 0; i < Items.Length; i++ )
			{
				if ( GetAt( i ) != null && Items[i].Stacks(itemsLeft) )
				{
					int removed = Math.Min( itemsLeft.Count, Items[i].Count );
					itemsLeft.Count -= removed;
					Items[i].Count -= removed;
					if(itemsLeft.Count <= 0 )
						return null;
				}
					
			}
			return itemsLeft;
		}
		public ItemStack GetAt( int index )
		{
			if (Items[index] == null )
				return null;
			if (Items[index].Count <= 0 )
			{
				Items[index] = null;
			}
			return Items[index];
		}
		public ItemStack SetAt(int index, ItemStack item )
		{
			if(item != null && item.Count <= 0 )
			{
				item = null;
			}
			ItemStack previous = GetAt(index);
			Items[index] = item;
			return previous;
		}
		public bool CanCraft(Recipe recipe )
		{
			foreach( RecipeItem recipeItem in recipe.Inputs )
			{
				ItemStack stack = recipeItem.ToStack();
				if(CountItems(stack) < stack.Count )
				{
					return false;
				}
			}
			return true;
		}
		public void Craft(Recipe recipe )
		{
			if ( !CanCraft( recipe ) )
			{
				return;
			}
			foreach ( RecipeItem recipeItem in recipe.Inputs )
			{
				RemoveItem(recipeItem.ToStack());
			}
			foreach ( RecipeItem recipeItem in recipe.Outputs )
			{
				AddItem( recipeItem.ToStack() );
			}
		}
	}
	public class Recipe
	{
		public string RecipeType {  get; set; }
		public List<RecipeItem> Inputs {  get; set; }
		public List<RecipeItem> Outputs {  get; set; }
		
		public Recipe()
		{
			RecipeType = "crafting";
			Inputs = new List<RecipeItem>();
			Outputs = new List<RecipeItem>();
		}
	}
	public class RecipeItem
	{
		public string Id { get; set; }
		public int Count { get; set; }
		
		public RecipeItem()
		{
			Id = "";
			Count = 1;
		}
		public ItemStack ToStack()
		{
			return ItemStack.Create( Id, Count );
		}
	}
}
