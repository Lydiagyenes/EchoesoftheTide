using GDS.Core;
using GDS.Core.Events;


    // A saját eseményünk, ami öröklődik a CustomEvent-ből
    public record ItemPickedUp(GDS.Core.ItemBase ItemBase, int Quantity) : CustomEvent;
