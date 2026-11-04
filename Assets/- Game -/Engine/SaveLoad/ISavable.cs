using System;

public interface ISavable
{
    event Action OnSaveRequired;
    
    void Load();
    void Save();
}