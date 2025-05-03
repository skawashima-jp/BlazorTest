using BlazorAppMhwSkill.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAppMhwSkill.Models
{
    internal class Armor: DownloadModelBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Part {  get; set; }
        public int Rare { get; set; }
        public int Defence { get; set; }
        public int Fire { get; set; }
        public int Water { get; set; }
        public int Thunder { get; set; }
        public int Ice { get; set; }
        public int Dragon { get; set; }
        public int Slot1 { get; set; }
        public int Slot2 { get; set; }
        public int Slot3 { get; set; }
        public string SeriesSkill { get; set; } = string.Empty;
        public string GroupSkill {  get; set; } = string.Empty;

        public List<ArmorSkill> Skills { get; set; } = new List<ArmorSkill> { };

        public static string GetHeader()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Id");
            stringBuilder.Append("\t");
            stringBuilder.Append("Name");
            stringBuilder.Append("\t");
            stringBuilder.Append("Part");
            stringBuilder.Append("\t");
            stringBuilder.Append("Rare");
            stringBuilder.Append("\t");
            stringBuilder.Append("Defence");
            stringBuilder.Append("\t");
            stringBuilder.Append("Fire");
            stringBuilder.Append("\t");
            stringBuilder.Append("Water");
            stringBuilder.Append("\t");
            stringBuilder.Append("Thunder");
            stringBuilder.Append("\t");
            stringBuilder.Append("Ice");
            stringBuilder.Append("\t");
            stringBuilder.Append("Dragon");
            stringBuilder.Append("\t");
            stringBuilder.Append("Slot1");
            stringBuilder.Append("\t");
            stringBuilder.Append("Slot2");
            stringBuilder.Append("\t");
            stringBuilder.Append("Slot3");
            stringBuilder.Append("\t");
            stringBuilder.Append("SeriesSkill");
            stringBuilder.Append("\t");
            stringBuilder.Append("GroupSkill");

            return stringBuilder.ToString();

        }
        public string GetRow()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetRowWithoutSkill());
            bool isFirst = true;
            foreach (var entity in Skills)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    stringBuilder.Append("|");
                }
                stringBuilder.Append(entity.Name);
                stringBuilder.Append(":");
                stringBuilder.Append(entity.Level);
            }

            return stringBuilder.ToString();
        }

        public string GetRowWithoutSkill()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Id);
            stringBuilder.Append("\t");
            stringBuilder.Append(Name);
            stringBuilder.Append("\t");
            stringBuilder.Append(Part);
            stringBuilder.Append("\t");
            stringBuilder.Append(Rare);
            stringBuilder.Append("\t");
            stringBuilder.Append(Defence);
            stringBuilder.Append("\t");
            stringBuilder.Append(Fire);
            stringBuilder.Append("\t");
            stringBuilder.Append(Water);
            stringBuilder.Append("\t");
            stringBuilder.Append(Thunder);
            stringBuilder.Append("\t");
            stringBuilder.Append(Ice);
            stringBuilder.Append("\t");
            stringBuilder.Append(Dragon);
            stringBuilder.Append("\t");
            stringBuilder.Append(Slot1);
            stringBuilder.Append("\t");
            stringBuilder.Append(Slot2);
            stringBuilder.Append("\t");
            stringBuilder.Append(Slot3);
            stringBuilder.Append("\t");
            stringBuilder.Append(SeriesSkill);
            stringBuilder.Append("\t");
            stringBuilder.Append(GroupSkill);

            return stringBuilder.ToString();
        }

    }
}
