/// <summary>
/// Implement this on any manager that needs to persist data
/// (inventory, equipment, chest/key unlocks, quest flags, etc).
/// SaveManager collects/distributes data through this interface,
/// so it never needs to know the internal fields of each system.
/// </summary>
public interface ISaveable
{
    // A short, unique key identifying this saveable's data block
    // e.g. "inventory", "equipment", "chests", "quests"
    string SaveKey { get; }

    // Return this system's data as a JSON-serializable object.
    object CaptureState();

    // Restore this system's data from the previously saved object.
    // jsonBlob is the raw JSON string for this key — deserialize it
    // into whatever type CaptureState() returned.
    void RestoreState(string jsonBlob);
}