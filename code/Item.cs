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
		public ItemStack( Item item )
		{
			ItemType = item;
		}
		public static ItemStack Create(string id )
		{
			Log.Info( Game.ActiveScene.GetAllComponents<ItemRegistry>().First().Items.Values.First());
			return new ItemStack( Game.ActiveScene.GetAllComponents<ItemRegistry>().First().Items[id] );
		}
	}
	public class Inventory
	{
		public ItemStack[] Items;
		public Inventory( int size )
		{
			Items = new ItemStack[size];
		}
		public void addItem( ItemStack item )
		{
			for ( int i = 0; i < Items.Length; i++ ) {
				if ( Items[i] == null ) {
					Items[i] = item;
					return;
				}
			}		
		}
	}
}
