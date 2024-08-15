using Sandbox;
using System;
using System.Threading.Tasks;

public sealed class InventoryComponent : Component
{
	[Sync]
	public NetDictionary<int,ItemStackRaw> Items {  get; set; }

	public int Size => Slots.Count;

	[Property]
	public Dictionary<int,SlotData> Slots { get; set; }

	public InventoryComponent()
	{
		Items = new NetDictionary<int, ItemStackRaw>();
		
	}
	public bool SlotSupports(int slot, SlotType mask, ItemStack item )
	{
		return (Slots[slot].SlotType & mask) != SlotType.None && ((item == null ? true : Slots[slot].Filter.Contains(item.ItemType.Id)) || Slots[slot].Filter.Count == 0);
	}
	public ItemStack AddItem( ItemStack item , SlotType mask = SlotType.Any )
	{
		if ( item == null )
			return null;
		ItemStack itemsLeft = item.Clone();
		for ( int i = 0; i < Size; i++ )
		{
			if ( !SlotSupports(i, mask, itemsLeft) )
				continue;
			if ( GetAt( i ) == null )
			{
				SetAt( i, itemsLeft );
				return null;
			}
			else
			{
				if ( GetAt(i).Stacks( itemsLeft ) )
				{
					int removeCount = Math.Min( itemsLeft.Count, GetAt(i).ItemType.StackSize - GetAt(i).Count );
					itemsLeft.Count -= removeCount;
					ItemStack stack = GetAt( i );
					stack.Count += removeCount;
					SetAt( i, stack );
					if ( itemsLeft.Count <= 0 )
						return null;
				}
			}
		}
		return itemsLeft;
	}
	public int CountItems( ItemStack item, SlotType mask = SlotType.Any )
	{
		int count = 0;
		for ( int i = 0; i < Size; i++ )
		{
			if ( !SlotSupports( i, mask, null ) )
				continue;
			ItemStack foundItem = GetAt(i);
			if ( foundItem != null && foundItem.Stacks( item ) )
			{
				count += foundItem.Count;
			}
		}
		return count;
	}
	public int CountFree( ItemStack item, SlotType mask = SlotType.Any )
	{
		int count = 0;
		for ( int i = 0; i < Size; i++ )
		{
			if ( !SlotSupports( i, mask, item ) )
				continue;
			ItemStack foundItem = GetAt( i );
			if(foundItem == null )
			{
				count += item.ItemType.StackSize;
			} else if(foundItem.Stacks(item) ) {
				count += foundItem.ItemType.StackSize-foundItem.Count;
			}
		}
		return count;
	}
	public ItemStack RemoveItem( ItemStack item, SlotType mask = SlotType.Any )
	{
		if ( item == null ) return null;
		ItemStack itemsLeft = item.Clone();
		for ( int i = 0; i < Size; i++ )
		{
			if ( !SlotSupports(i, mask, null) )
				continue;
			if ( GetAt( i ) != null && GetAt(i).Stacks( itemsLeft ) )
			{
				int removed = Math.Min( itemsLeft.Count, GetAt(i).Count );
				itemsLeft.Count -= removed;
				ItemStack slot = GetAt( i );
				slot.Count -= removed;
				SetAt( i, slot );
				if ( itemsLeft.Count <= 0 )
					return null;
			}

		}
		return itemsLeft;
	}
	public ItemStack GetAt( int index )
	{
		ItemStackRaw stack = null;
		Items.TryGetValue( index, out stack );
		return stack?.ToStack();
		//Items.TryAdd( index, null );
		//return Items[index]?.ToStack();
	}
	public ItemStack SetAt( int index, ItemStack item )
	{
		if ( item != null && item.Count <= 0 )
		{
			item = null;
		}
		ItemStack previous = GetAt( index );
		Items[index] = ItemStackRaw.FromStack(item);
		return previous;
	}
	
	[Authority]
	public void NetAddItem(ItemStackRaw item, SlotType mask = SlotType.Any )
	{
		if(item != null)
			AddItem( item.ToStack(), mask );
	}
	[Authority]
	public void NetAddItemPreferSlot( ItemStackRaw item, int slot, SlotType mask = SlotType.Any )
	{
		if ( item != null )
		{
			ItemStack stack = item.ToStack();
			if(slot >= 0 && slot < Size )
			{
				if ( SlotSupports( slot, mask, stack ) )
				{
					ItemStack stackOld = GetAt( slot );
					if ( stackOld == null )
					{
						SetAt( slot, stack );
						return;
					}
					else if ( stackOld != null && stackOld.Stacks( stack ) )
					{
						int addCount = Math.Min( stack.Count, stackOld.ItemType.StackSize - stackOld.Count );
						stack.Count -= addCount;
						stackOld.Count += addCount;
						SetAt( slot, stackOld );
					}
				}
			}
			AddItem( stack, mask );
		}
	}
	[Authority]
	public void NetRemoveItem( ItemStackRaw item, SlotType mask = SlotType.Any )
	{
		if ( item != null )
			RemoveItem( item.ToStack(), mask );
	}
	[Authority]
	public void NetRemoveFromSlot(int slot, int count )
	{
		if( slot >= 0 && slot < Size )
		{
			ItemStack item = GetAt( slot );
			if(item != null )
			{
				item.Count -= count;
				SetAt( slot, item );
			}
		}
	}
	public struct SlotData
	{
		[Property]
		[BitFlags]
		public SlotType SlotType { get; set; }
		[Property]
		public List<string> Filter { get; set; }
	}

	[Flags]
	public enum SlotType
	{
		Input = 1,
		Output = 2,
		MachineInput = 4,
		MachineOutput = 8,
		Any = -1,
		None = 0
	}
}
