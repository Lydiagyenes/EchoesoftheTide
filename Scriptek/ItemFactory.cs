using GDS.Core;

// Ez egy "extension method" gyűjtemény lesz
public static class ItemFactory
{
    // Létrehoz egy 'Item'-et egy 'ItemBase'-ből és egy mennyiségből
    public static Item Create(this ItemBase itemBase, int quantity = 1)
    {
        return new Item() { Base = itemBase, Quant = quantity };
    }
}
