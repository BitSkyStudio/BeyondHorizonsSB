using Sandbox;
using System;

public sealed class InventoryComponent : Component
{
	[Sync]
	public NetDictionary<int,ItemStackRaw> Items {  get; set; }

	[Sync]
	[Property]
	public int Size { get; set; }

	public InventoryComponent()
	{
		Items = new NetDictionary<int, ItemStackRaw>();
		
	}
	public ItemStack AddItem( ItemStack item )
	{
		if ( item == null )
			return null;
		ItemStack itemsLeft = item.Clone();
		for ( int i = 0; i < Items.Count; i++ )
		{
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
	public int CountItems( ItemStack item )
	{
		int count = 0;
		for ( int i = 0; i < Items.Count; i++ )
		{
			ItemStack foundItem = GetAt(i);
			if ( foundItem != null && foundItem.Stacks( item ) )
			{
				count += foundItem.Count;
			}
		}
		return count;
	}
	public ItemStack RemoveItem( ItemStack item )
	{
		if ( item == null ) return null;
		ItemStack itemsLeft = item.Clone();
		for ( int i = 0; i < Size; i++ )
		{
			if ( GetAt( i ) != null && GetAt(i).Stacks( itemsLeft ) )
			{
				int removed = Math.Min( itemsLeft.Count, GetAt(i).Count );
				Log.Info( "rem: " + removed );
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
		Items.TryAdd( index, null );
		return Items[index]?.ToStack();
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
	public void NetAddItem(ItemStackRaw item )
	{
		if(item != null)
			AddItem( item.ToStack() );
	}
	[Authority]
	public void NetAddItemPreferSlot( ItemStackRaw item, int slot )
	{
		if ( item != null )
		{
			ItemStack stack = item.ToStack();
			if(slot >= 0 && slot < Size )
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
			AddItem( stack );
		}
	}
	[Authority]
	public void NetRemoveItem( ItemStackRaw item )
	{
		if ( item != null )
			RemoveItem( item.ToStack() );
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
}
