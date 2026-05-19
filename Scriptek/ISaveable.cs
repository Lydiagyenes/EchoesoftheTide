
public interface ISaveable
{
    // A SaveData a 'ref' kulcsszóval módosítja a központi gameData objektumot
    void SaveData(ref GameData data);
    
    // A LoadData csak olvassa a gameData objektumot
    void LoadData(GameData data);
}
