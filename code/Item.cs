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
		public ToolType ToolType { get; set; }
		public float UseTime { get; set; }
		public float Damage { get; set; }
		public Item()
		{
			Id = "";
			DisplayName = "unknown";
			StackSize = 0;
			Prefab = null;
			ToolType = ToolType.None;
			UseTime = 1;
			Damage = 10;
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
		public ItemStack Clone(int newCount)
		{
			return new ItemStack( ItemType, newCount );
		}
		public string AsString()
		{
			if(Count == 1 )
			{
				return ItemType.DisplayName;
			} else
			{
				return Count + " x " + ItemType.DisplayName;
			}
		}
	}
	public class Recipe
	{
		public string Id { get; set; }
		public string RecipeType {  get; set; }
		public List<RecipeInput> Inputs {  get; set; }
		public List<ItemStackRaw> Outputs {  get; set; }
		public float ProcessingTime {  get; set; }
		
		public Recipe()
		{
			Id = "";
			RecipeType = "crafting";
			Inputs = new List<RecipeInput>();
			Outputs = new List<ItemStackRaw>();
			ProcessingTime = 1;
		}
		public string DisplayString()
		{
			string outputString = "Craft " + String.Join( ", ", Outputs.AsEnumerable().Select( item => item.ToStack().AsString() ) ) + " with " + String.Join( ", ", Inputs.AsEnumerable().Where(item=>item.Consume).Select( item => item.ToStack().AsString() ) );
			if(Inputs.Where( input => !input.Consume ).Count() > 0 )
			{
				outputString += " using " + String.Join( ", ", Inputs.AsEnumerable().Where( item => !item.Consume ).Select( item => item.ToStack().AsString() ) );
			}
			return outputString;
		}
		public bool CanCraft( InventoryComponent inventory )
		{
			foreach ( RecipeInput recipeItem in Inputs )
			{
				ItemStack stack = recipeItem.ToStack();
				if ( inventory.CountItems( stack, InventoryComponent.SlotType.MachineInput ) < stack.Count )
				{
					return false;
				}
			}
			foreach ( ItemStackRaw outputItem in Outputs )
			{
				ItemStack stack = outputItem.ToStack();
				if ( inventory.CountFree( stack, InventoryComponent.SlotType.MachineOutput) < stack.Count )
				{
					return false;
				}
			}
			return true;
		}
		public bool Craft( InventoryComponent inventory )
		{
			if ( !CanCraft( inventory ) )
			{
				return false;
			}
			ConsumeInputs(inventory);
			AddOutputs( inventory );
			return true;
		}
		public void ConsumeInputs( InventoryComponent inventory )
		{
			foreach ( RecipeInput recipeItem in Inputs )
			{
				if ( recipeItem.Consume )
				{
					inventory.RemoveItem( recipeItem.ToStack(), InventoryComponent.SlotType.MachineInput );
				}
			}
		}
		public void AddOutputs(InventoryComponent inventory )
		{
			foreach ( ItemStackRaw recipeItem in Outputs )
			{
				inventory.AddItem( recipeItem.ToStack(), InventoryComponent.SlotType.MachineOutput );
			}
		}
	}
	public class RecipeInput
	{
		public string Id { get; set; }
		public int Count { get; set; }
		public bool Consume { get; set; }
		public RecipeInput()
		{
			Id = "";
			Count = 1;
			Consume = false;
		}
		public ItemStack ToStack()
		{
			return ItemStack.Create( Id, Count );
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
	public enum ToolType
	{
		None,
		Axe,
		Pickaxe,
		Knife,
		Shovel,
		Food,
		Fuel
	}
}
