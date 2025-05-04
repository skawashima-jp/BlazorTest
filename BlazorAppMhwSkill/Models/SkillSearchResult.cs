using BlazorAppMhwSkill.Logic;
using static BlazorAppMhwSkill.Pages.SelectSkill;
using System.Text;

namespace BlazorAppMhwSkill.Models
{
    public class SkillSearchResult : DownloadModelBase
    {
        public string Head { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Arm { get; set; } = string.Empty;
        public string Weist { get; set; } = string.Empty;
        public string Foot { get; set; } = string.Empty;

        public List<CondSkill> AddSkills { get; set; } = new List<CondSkill>();

        public string GetAddSkillsString()
        {
            return GetSkillsString(AddSkills);
        }
        public string GetAmariSkillsString()
        {
            return GetSkillsString(AmariSkills);
        }

        private string GetSkillsString(List<CondSkill> skills)
        {

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var skill in skills)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append("|");
                }
                sb.Append(skill.SkillName);
                sb.Append(":");
                sb.Append(skill.SkillLevel);
            }
            return sb.ToString();
        }

        public static string GetHeader()
        {
            return "頭\t体\t腕\t胴\t足\t追加スキル\t護石\t余りスキル\t3スロ余\t2スロ余\t1スロ余\t火\t水\t雷\t氷\t龍\tグループスキル";
        }

        public string GetRow()
        {
            const string DELIMITER = "\t";
            StringBuilder sb = new StringBuilder();
            sb.Append(Head);
            sb.Append(DELIMITER);
            sb.Append(Body);
            sb.Append(DELIMITER);
            sb.Append(Arm);
            sb.Append(DELIMITER);
            sb.Append(Weist);
            sb.Append(DELIMITER);
            sb.Append(Foot);
            sb.Append(DELIMITER);
            sb.Append(GetAddSkillsString());
            sb.Append(DELIMITER);
            sb.Append(UsedGoseki);
            sb.Append(DELIMITER);
            sb.Append(GetAmariSkillsString());
            sb.Append(DELIMITER);
            sb.Append(Slot3);
            sb.Append(DELIMITER);
            sb.Append(Slot2);
            sb.Append(DELIMITER);
            sb.Append(Slot1);
            sb.Append(DELIMITER);
            sb.Append(Fire);
            sb.Append(DELIMITER);
            sb.Append(Water);
            sb.Append(DELIMITER);
            sb.Append(Thunder);
            sb.Append(DELIMITER);
            sb.Append(Ice);
            sb.Append(DELIMITER);
            sb.Append(Dragon);
            sb.Append(DELIMITER);
            sb.Append(GroupSkill);
            return sb.ToString();
        }

        public int Slot3 { get; set; } = 0;
        public int Slot2 { get; set; } = 0;
        public int Slot1 { get; set; } = 0;

        public string UsedGoseki { get; set; } = string.Empty;
        public string GroupSkill { get; set; } = string.Empty;

        public List<CondSkill> AmariSkills { get; set; } = new List<CondSkill>();
        public int Fire { get; set; } = 0;
        public int Water { get; set; } = 0;
        public int Thunder { get; set; } = 0;
        public int Ice { get; set; } = 0;
        public int Dragon { get; set; } = 0;

    }

}
