namespace BlazorAppMhwSkill.Models
{
    /// <summary>
    /// 検索条件のスキル
    /// </summary>
    public class CondSkill
    {
        public string SkillName { get; set; } = "";

        public int SkillLevel { get; set; } = 0;

        public int Slot { get; set; } = 0;
    }
}
