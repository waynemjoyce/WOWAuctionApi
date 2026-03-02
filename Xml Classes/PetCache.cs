using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WOWApi;

namespace WOWApi
{ }
public class PetCache
{
    public List<Pet> Pets = new List<Pet>();

    [XmlIgnore]
    public List<long> PetIds = new List<long>();

    public void Save()
    {
        SaveToFile(Paths.PetCachePath);
    }

    public void ClearItems()
    {
        Pets.Clear();
    }

    public void AddPet(Pet itemToAdd)
    {
        Pets.Add(itemToAdd);
    }

    public void SaveToFile(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(PetCache));
        System.IO.FileStream file = System.IO.File.Create(filePath);
        serializer.Serialize(file, this);
        file.Close();
    }

    public void FillPetIds()
    {
        PetIds.Clear();

        foreach (Pet pt in Pets)
        {
            PetIds.Add(pt.Id);
        }
    }

    public static PetCache Load()
    {
        return PetCache.LoadFromFile(Paths.PetCachePath);
    }
    public static PetCache LoadFromFile(string fileName)
    {
        PetCache returnIc = new PetCache();
        System.IO.Stream file = File.OpenRead(fileName);
        XmlSerializer serializer = new XmlSerializer(typeof(PetCache));
        returnIc = (PetCache)serializer.Deserialize(file);
        file.Close();
        return returnIc;
    }

    public static PetCache LoadFromXml(string xml)
    {
        if (xml == String.Empty)
        {
            return null;
        }
        else
        {
            PetCache returnScript = new PetCache();
            var serializer = new XmlSerializer(typeof(Config));
            using (var reader = new StringReader(xml))
            {
                returnScript = (PetCache)serializer.Deserialize(reader);
            }
            return returnScript;
        }
    }

}

public class Pet
{
    [XmlAttribute]
    public long Id;
    [XmlAttribute]
    public string Name;
    [XmlAttribute]
    public string BattlePetType;
    [XmlAttribute]
    public bool IsTradable;
    [XmlAttribute]
    public bool IsBattlePet;
    [XmlAttribute]
    public bool IsCapturable;
    [XmlAttribute]
    public bool IsAllianceOnly;
    [XmlAttribute]
    public bool IsHordeOnly;
    [XmlAttribute]
    public string Description;
    [XmlAttribute]
    public string Source;
}
